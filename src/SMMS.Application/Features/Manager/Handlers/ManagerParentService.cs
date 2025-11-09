using ClosedXML.Excel;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.Manager.DTOs;
using SMMS.Application.Features.Manager.Interfaces;
using SMMS.Domain.Entities.auth;
using SMMS.Domain.Entities.school;
using System.Globalization;
using ExcelDataReader;
using Microsoft.Extensions.Logging;
namespace SMMS.Application.Features.Manager.Handlers;

public class ManagerParentService : IManagerParentService
{
    private readonly IManagerAccountRepository _repo;
    private readonly ILogger<ManagerParentService> _logger;

    public ManagerParentService(
        IManagerAccountRepository repo,
        ILogger<ManagerParentService> logger)
    {
        _repo = repo;
        _logger = logger;
    }
    // 🔍 Tìm kiếm phụ huynh theo tên, email, SĐT hoặc tên con
    public async Task<List<ParentAccountDto>> SearchAsync(Guid schoolId, string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return new List<ParentAccountDto>();

        keyword = keyword.Trim().ToLower();

        var query = _repo.Users
            .Include(u => u.Role)
            .Include(u => u.School)
            .Include(u => u.Students)
                .ThenInclude(s => s.StudentClasses)
                    .ThenInclude(sc => sc.Class)
            .Where(u =>
                u.SchoolId == schoolId &&
                u.Role.RoleName.ToLower() == "parent" &&
                (
                    u.FullName.ToLower().Contains(keyword) ||
                    u.Email.ToLower().Contains(keyword) ||
                    u.Phone.ToLower().Contains(keyword) ||
                    u.Students.Any(s =>
                        s.FullName.ToLower().Contains(keyword) ||
                        s.StudentClasses.Any(sc => sc.Class.ClassName.ToLower().Contains(keyword))
                    )
                ));

        return await query
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new ParentAccountDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.Phone,
                Role = u.Role.RoleName,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                SchoolName = u.School != null ? u.School.SchoolName : "(Chưa gán trường)",

                ChildrenNames = u.Students
                    .Select(s => s.FullName)
                    .ToList(),

                ClassName = u.Students
                    .SelectMany(s => s.StudentClasses)
                    .Where(sc => sc.Class != null)
                    .Select(sc => sc.Class.ClassName)
                    .Distinct()
                    .FirstOrDefault()
            })
            .ToListAsync();
    }
    // 🟢 Lấy danh sách phụ huynh theo trường hoặc lớp
    public async Task<List<ParentAccountDto>> GetAllAsync(Guid schoolId, Guid? classId = null)
    {
        var query = _repo.Users
            .Include(u => u.Role)
            .Include(u => u.School)
            .Include(u => u.Students)
                .ThenInclude(s => s.StudentClasses)
                    .ThenInclude(sc => sc.Class)
            .Where(u => u.SchoolId == schoolId && u.Role.RoleName.ToLower() == "parent");

        if (classId.HasValue)
        {
            query = query.Where(u => u.Students
                .Any(s => s.StudentClasses.Any(sc => sc.ClassId == classId)));
        }

        return await query
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new ParentAccountDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.Phone,
                Role = u.Role.RoleName,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                SchoolName = u.School != null ? u.School.SchoolName : "(Chưa gán trường)",

                // 🔹 Lấy danh sách tên con
                ChildrenNames = u.Students
                    .Select(s => s.FullName)
                    .ToList(),

                // 🔹 Lấy danh sách tên lớp mà con đang học
                ClassName = u.Students
                .SelectMany(s => s.StudentClasses)
                .Where(sc => sc.Class != null)
                .Select(sc => sc.Class.ClassName)
                .Distinct()
                .FirstOrDefault()
            })
            .ToListAsync();
    }

    // 🟡 Tạo tài khoản phụ huynh + con + gán lớp
    public async Task<AccountDto> CreateAsync(CreateParentRequest request)
    {
        var role = await _repo.Roles.FirstOrDefaultAsync(r => r.RoleName.ToLower() == "parent");
        if (role == null)
            throw new InvalidOperationException("Không tìm thấy vai trò 'Parent'.");

        var exists = await _repo.Users.AnyAsync(u =>
            u.Email == request.Email || u.Phone == request.Phone);
        if (exists)
            throw new InvalidOperationException("Email hoặc số điện thoại đã tồn tại.");

        var parent = new User
        {
            UserId = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = request.Email?.Trim().ToLower(),
            Phone = request.Phone.Trim(),
            PasswordHash = request.Password,
            RoleId = role.RoleId,
            SchoolId = request.SchoolId,
            LanguagePref = "vi",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy
        };
        await _repo.AddAsync(parent);

        // 🔹 Nếu có danh sách con, tạo từng đứa
        foreach (var child in request.Children)
        {
            var student = new Student
            {
                StudentId = Guid.NewGuid(),
                FullName = child.FullName.Trim(),
                Gender = child.Gender,
                DateOfBirth = child.DateOfBirth != null ? DateOnly.FromDateTime(child.DateOfBirth.Value) : null,
                SchoolId = request.SchoolId,
                ParentId = parent.UserId,
                RelationName = request.RelationName ?? "Phụ huynh", // ✅ chỉ cần 1 lần trong request
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            };
            await _repo.AddStudentAsync(student);

            var studentClass = new StudentClass
            {
                StudentId = student.StudentId,
                ClassId = child.ClassId,
                JoinedDate = DateOnly.FromDateTime(DateTime.UtcNow),
                RegistStatus = true
            };
            await _repo.AddStudentClassAsync(studentClass);
        }

        return new AccountDto
        {
            UserId = parent.UserId,
            FullName = parent.FullName,
            Email = parent.Email ?? string.Empty,
            Phone = parent.Phone,
            Role = "Parent",
            IsActive = parent.IsActive,
            CreatedAt = parent.CreatedAt
        };
    }


    // 🟠 Cập nhật thông tin phụ huynh + con + lớp
    public async Task<AccountDto?> UpdateAsync(Guid userId, UpdateParentRequest request)
    {
        // 🔹 Tìm phụ huynh
        var user = await _repo.Users
            .Include(u => u.Role)
            .Include(u => u.Students)
                .ThenInclude(s => s.StudentClasses)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null || user.Role.RoleName.ToLower() != "parent")
            return null;

        // 🔹 Cập nhật thông tin phụ huynh
        if (!string.IsNullOrWhiteSpace(request.FullName))
            user.FullName = request.FullName.Trim();
        if (!string.IsNullOrWhiteSpace(request.Email))
            user.Email = request.Email.Trim().ToLower();
        if (!string.IsNullOrWhiteSpace(request.Phone))
            user.Phone = request.Phone.Trim();
        if (!string.IsNullOrWhiteSpace(request.Password))
            user.PasswordHash = request.Password;
        if (!string.IsNullOrWhiteSpace(request.Gender))
            user.LanguagePref = request.Gender; // (hoặc trường giới tính riêng nếu có)

        user.UpdatedBy = request.UpdatedBy;
        user.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(user);

        // 🔹 Nếu có danh sách con gửi lên
        if (request.Children != null && request.Children.Any())
        {
            foreach (var childDto in request.Children)
            {
                // 🔸 Kiểm tra xem con đã tồn tại chưa
                var existingChild = user.Students.FirstOrDefault(s => s.FullName == childDto.FullName);

                if (existingChild != null)
                {
                    // Cập nhật thông tin con
                    if (!string.IsNullOrWhiteSpace(childDto.FullName))
                        existingChild.FullName = childDto.FullName.Trim();

                    if (!string.IsNullOrWhiteSpace(childDto.Gender))
                        existingChild.Gender = childDto.Gender;

                    if (childDto.DateOfBirth.HasValue)
                        existingChild.DateOfBirth = DateOnly.FromDateTime(childDto.DateOfBirth.Value);

                    existingChild.RelationName = request.RelationName ?? "Phụ huynh";
                    existingChild.UpdatedAt = DateTime.UtcNow;

                    await _repo.UpdateStudentAsync(existingChild);
                }
                else
                {
                    // 🔸 Nếu con chưa có → thêm mới
                    var newStudent = new Student
                    {
                        StudentId = Guid.NewGuid(),
                        FullName = childDto.FullName.Trim(),
                        Gender = childDto.Gender,
                        DateOfBirth = childDto.DateOfBirth != null
                            ? DateOnly.FromDateTime(childDto.DateOfBirth.Value)
                            : null,
                        SchoolId = user.SchoolId!.Value,
                        ParentId = user.UserId,
                        RelationName = request.RelationName ?? "Phụ huynh",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                    };

                    await _repo.AddStudentAsync(newStudent);

                    var studentClass = new StudentClass
                    {
                        StudentId = newStudent.StudentId,
                        ClassId = childDto.ClassId,
                        JoinedDate = DateOnly.FromDateTime(DateTime.UtcNow),
                        RegistStatus = true
                    };

                    await _repo.AddStudentClassAsync(studentClass);
                }
            }
        }

        // 🔹 Trả về DTO kết quả
        return new AccountDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Role = "Parent",
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    // 🔵 Đổi trạng thái kích hoạt
    public async Task<bool> ChangeStatusAsync(Guid userId, bool isActive)
    {
        var user = await _repo.GetByIdAsync(userId);
        if (user == null || user.Role.RoleName.ToLower() != "parent")
            return false;

        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(user);
        return true;
    }

    // 🔴 Xóa tài khoản
    public async Task<bool> DeleteAsync(Guid userId)
    {
        var user = await _repo.Users
            .Include(u => u.Students)
                .ThenInclude(s => s.StudentClasses)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
            return false;

        // 🧩 Tạo bản sao để tránh lỗi "Collection was modified"
        var studentsToDelete = user.Students.ToList();

        foreach (var student in studentsToDelete)
        {
            var studentClassesToDelete = student.StudentClasses.ToList();

            foreach (var sc in studentClassesToDelete)
            {
                await _repo.DeleteStudentClassAsync(sc);
            }

            await _repo.DeleteStudentAsync(student);
        }

        // 🧩 Sau khi xóa hết con, xóa luôn phụ huynh
        await _repo.DeleteAsync(user);

        return true;
    }
    public async Task<List<AccountDto>> ImportFromExcelAsync(Guid schoolId, IFormFile file, string createdBy)
    {
        var result = new List<AccountDto>();

        if (file == null || file.Length == 0)
            throw new InvalidOperationException("Không có file được tải lên.");

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        using var workbook = new XLWorkbook(stream);
        var sheet = workbook.Worksheet("Danh sách phụ huynh");

        if (sheet == null)
            throw new InvalidOperationException("Không tìm thấy sheet 'Danh sách phụ huynh' trong file Excel.");

        var role = await _repo.Roles.FirstOrDefaultAsync(r => r.RoleName.ToLower() == "parent");
        if (role == null)
            throw new InvalidOperationException("Không tìm thấy vai trò 'Parent'.");

        int row = 2;
        while (!string.IsNullOrWhiteSpace(sheet.Cell(row, 1).GetString()))
        {
            try
            {
                var fullNameParent = sheet.Cell(row, 1).GetString()?.Trim();
                var email = sheet.Cell(row, 2).GetString()?.Trim().ToLower();
                var phone = sheet.Cell(row, 3).GetString()?.Trim();
                var password = sheet.Cell(row, 4).GetString()?.Trim();
                var genderParent = sheet.Cell(row, 5).GetString()?.Trim();
                var dobParent = sheet.Cell(row, 6).GetString()?.Trim();
                var relationName = sheet.Cell(row, 7).GetString()?.Trim();

                var fullNameChild = sheet.Cell(row, 8).GetString()?.Trim();
                var genderChild = sheet.Cell(row, 9).GetString()?.Trim();
                var dobChild = sheet.Cell(row, 10).GetString()?.Trim();
                var classIdStr = sheet.Cell(row, 11).GetString()?.Trim();

                if (string.IsNullOrWhiteSpace(fullNameParent) || string.IsNullOrWhiteSpace(phone))
                    throw new InvalidOperationException($"Thiếu thông tin bắt buộc tại dòng {row}: FullName_Parent hoặc Phone.");

                var exists = await _repo.Users.AnyAsync(u => u.Email == email || u.Phone == phone);
                if (exists)
                    throw new InvalidOperationException($"Email hoặc số điện thoại đã tồn tại: {email ?? phone}");

                var parent = new User
                {
                    UserId = Guid.NewGuid(),
                    FullName = fullNameParent,
                    Email = email,
                    Phone = phone,
                    PasswordHash = password,
                    RoleId = role.RoleId,
                    SchoolId = schoolId,
                    LanguagePref = "vi",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await _repo.AddAsync(parent);

                if (!string.IsNullOrWhiteSpace(fullNameChild))
                {
                    var student = new Student
                    {
                        StudentId = Guid.NewGuid(),
                        FullName = fullNameChild,
                        Gender = genderChild,
                        DateOfBirth = !string.IsNullOrWhiteSpace(dobChild)
                            ? DateOnly.ParseExact(dobChild, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                            : null,
                        SchoolId = schoolId,
                        ParentId = parent.UserId,
                        RelationName = relationName ?? "Phụ huynh",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _repo.AddStudentAsync(student);

                    if (Guid.TryParse(classIdStr, out Guid classId))
                    {
                        var studentClass = new StudentClass
                        {
                            StudentId = student.StudentId,
                            ClassId = classId,
                            JoinedDate = DateOnly.FromDateTime(DateTime.UtcNow),
                            RegistStatus = true
                        };
                        await _repo.AddStudentClassAsync(studentClass);
                    }
                }

                result.Add(new AccountDto
                {
                    UserId = parent.UserId,
                    FullName = parent.FullName,
                    Email = parent.Email ?? string.Empty,
                    Phone = parent.Phone,
                    Role = "Parent",
                    IsActive = parent.IsActive,
                    CreatedAt = parent.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi tại dòng {row}: {ex.Message}");
            }

            row++;
        }

        return result;
    }

    public async Task<byte[]> GetExcelTemplateAsync()
    {
        using (var workbook = new XLWorkbook())
        {
            // 🟢 Sheet chính: Danh sách phụ huynh
            var sheet = workbook.Worksheets.Add("Danh sách phụ huynh");

            // 🧾 Tiêu đề cột (thông tin cần nhập)
            var headers = new[]
            {
            "FullName_Parent (Họ và tên phụ huynh)",
            "Email",
            "Phone",
            "Password(Nên để mặc định @1)",
            "Gender_Parent (M/F)",
            "DateOfBirth_Parent (dd/MM/yyyy)",
            "RelationName (Cha/Mẹ/Giám hộ)",
            "FullName_Child (Họ và tên con)",
            "Gender_Child (M/F)",
            "DateOfBirth_Child (dd/MM/yyyy)",
            "ClassId (ID lớp học)"
        };

            for (int i = 0; i < headers.Length; i++)
                sheet.Cell(1, i + 1).Value = headers[i];

            // 💅 Định dạng tiêu đề
            var headerRange = sheet.Range(1, 1, 1, headers.Length);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            // 📊 Dòng minh họa ví dụ
            sheet.Cell(2, 1).Value = "Nguyễn Văn A";
            sheet.Cell(2, 2).Value = "a@gmail.com";
            sheet.Cell(2, 3).Value = "0901234567";
            sheet.Cell(2, 4).Value = "@1";
            sheet.Cell(2, 5).Value = "M";
            sheet.Cell(2, 6).Value = "01/01/1980";
            sheet.Cell(2, 7).Value = "Cha";
            sheet.Cell(2, 8).Value = "Nguyễn Minh An";
            sheet.Cell(2, 9).Value = "M";
            sheet.Cell(2, 10).Value = "15/09/2015";
            sheet.Cell(2, 11).Value = "GUID của lớp học";

            // 📐 Tự động căn chỉnh độ rộng
            sheet.Columns().AdjustToContents();
            sheet.Rows().AdjustToContents();

            // 🟣 Sheet 2: Hướng dẫn nhập liệu
            var guide = workbook.Worksheets.Add("Hướng dẫn");
            var row = 1;

            guide.Cell(row++, 1).Value = "👉 HƯỚNG DẪN NHẬP FILE EXCEL";
            guide.Cell(row++, 1).Value = "- Không thay đổi tiêu đề cột ở sheet 'Danh sách phụ huynh'.";
            guide.Cell(row++, 1).Value = "- Cột 'RelationName': nhập Cha, Mẹ hoặc Giám hộ.";
            guide.Cell(row++, 1).Value = "- Cột 'Gender_Parent' và 'Gender_Child': chỉ nhập M hoặc F (Male/Female).";
            guide.Cell(row++, 1).Value = "- Cột 'DateOfBirth_*': định dạng dd/MM/yyyy (ngày/tháng/năm).";
            guide.Cell(row++, 1).Value = "- Cột 'ClassId': nhập GUID lớp học tương ứng trong hệ thống.";

            guide.Columns().AdjustToContents();
            guide.Rows().AdjustToContents();

            // 💾 Xuất file Excel ra dạng byte[]
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return await Task.FromResult(stream.ToArray());
            }
        }
    }


}
