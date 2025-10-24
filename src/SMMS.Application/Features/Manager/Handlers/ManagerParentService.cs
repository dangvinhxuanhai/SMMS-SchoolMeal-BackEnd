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
namespace SMMS.Application.Features.Manager.Handlers;

public class ManagerParentService : IManagerParentService
{
    private readonly IManagerAccountRepository _repo;

    public ManagerParentService(IManagerAccountRepository repo)
    {
        _repo = repo;
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
        if (file == null || file.Length == 0)
            throw new ArgumentException("File Excel không hợp lệ hoặc trống.");

        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        var resultList = new List<AccountDto>();

        using (var stream = file.OpenReadStream())
        using (var reader = ExcelReaderFactory.CreateReader(stream))
        {
            var dataSet = reader.AsDataSet();
            var table = dataSet.Tables[0];

            // ✅ Giả định dòng đầu tiên là tiêu đề
            for (int i = 1; i < table.Rows.Count; i++)
            {
                try
                {
                    // 🟢 Đọc dữ liệu
                    string fullNameParent = table.Rows[i][0]?.ToString()?.Trim() ?? "";
                    string email = table.Rows[i][1]?.ToString()?.Trim();
                    string phoneRaw = table.Rows[i][2]?.ToString()?.Trim();
                    string phone = phoneRaw?.Replace(" ", "").Replace("+", "");

                    // Excel đôi khi lưu số điện thoại dạng 9E+08 (double)
                    if (double.TryParse(phoneRaw, out var parsedNumber))
                        phone = parsedNumber.ToString("0");

                    string password = table.Rows[i][3]?.ToString()?.Trim() ?? "123456";
                    string genderParent = table.Rows[i][4]?.ToString()?.Trim();
                    string dobParentStr = table.Rows[i][5]?.ToString()?.Trim();
                    string relationName = table.Rows[i][6]?.ToString()?.Trim() ?? "Phụ huynh";
                    string fullNameChild = table.Rows[i][7]?.ToString()?.Trim();
                    string genderChild = table.Rows[i][8]?.ToString()?.Trim();
                    string dobChildStr = table.Rows[i][9]?.ToString()?.Trim();
                    string classIdStr = table.Rows[i][10]?.ToString()?.Trim();

                    if (string.IsNullOrWhiteSpace(fullNameParent) || string.IsNullOrWhiteSpace(phone))
                        continue;

                    Guid.TryParse(classIdStr, out Guid classId);

                    // 🔍 Kiểm tra trùng
                    var exists = await _repo.Users.AnyAsync(u => u.Email == email || u.Phone == phone);
                    if (exists) continue;

                    // 🧩 Lấy role Parent
                    var role = await _repo.Roles.FirstOrDefaultAsync(r => r.RoleName.ToLower() == "parent");
                    if (role == null)
                        throw new InvalidOperationException("Không tìm thấy vai trò 'Parent'.");

                    // 📅 Parse ngày sinh phụ huynh
                    DateOnly? dobParent = null;
                    if (!string.IsNullOrWhiteSpace(dobParentStr))
                    {
                        if (DateTime.TryParseExact(dobParentStr, new[] { "dd/MM/yyyy", "MM/dd/yyyy" },
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedParent))
                            dobParent = DateOnly.FromDateTime(parsedParent);
                    }

                    // ✅ Tạo phụ huynh
                    var parent = new User
                    {
                        UserId = Guid.NewGuid(),
                        FullName = fullNameParent,
                        Email = email?.ToLower(),
                        Phone = phone,
                        PasswordHash = password,
                        RoleId = role.RoleId,
                        SchoolId = schoolId,
                        LanguagePref = genderParent,
                        DateOfBirth = dobParent, // ✅ NGÀY SINH PHỤ HUYNH
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _repo.AddAsync(parent);

                    // 👶 Nếu có con
                    if (!string.IsNullOrWhiteSpace(fullNameChild))
                    {
                        DateOnly? dobChild = null;
                        if (!string.IsNullOrWhiteSpace(dobChildStr))
                        {
                            if (DateTime.TryParseExact(dobChildStr, new[] { "dd/MM/yyyy", "MM/dd/yyyy" },
                                CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedChild))
                                dobChild = DateOnly.FromDateTime(parsedChild);
                        }

                        var student = new Student
                        {
                            StudentId = Guid.NewGuid(),
                            FullName = fullNameChild,
                            Gender = genderChild,
                            DateOfBirth = dobChild,
                            SchoolId = schoolId,
                            ParentId = parent.UserId,
                            RelationName = relationName,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _repo.AddStudentAsync(student);

                        if (classId != Guid.Empty)
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

                    resultList.Add(new AccountDto
                    {
                        UserId = parent.UserId,
                        FullName = parent.FullName,
                        Email = parent.Email,
                        Phone = parent.Phone,
                        Role = "Parent",
                        IsActive = parent.IsActive,
                        CreatedAt = parent.CreatedAt
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Lỗi dòng {i + 1}: {ex.Message}");
                }
            }
        }

        return resultList;
    }

    public async Task<byte[]> GetExcelTemplateAsync()
    {
        using (var workbook = new XLWorkbook())
        {
            // 🟢 Sheet chính: Danh sách phụ huynh
            var sheet = workbook.Worksheets.Add("Danh sách phụ huynh");

            // 🧾 Tiêu đề cột (đầy đủ thông tin)
            sheet.Cell(1, 1).Value = "FullName_Parent (Họ và tên phụ huynh)";
            sheet.Cell(1, 2).Value = "Email";
            sheet.Cell(1, 3).Value = "Phone";
            sheet.Cell(1, 4).Value = "Password";
            sheet.Cell(1, 5).Value = "Gender_Parent (M/F)";
            sheet.Cell(1, 6).Value = "DateOfBirth_Parent (dd/MM/yyyy)";
            sheet.Cell(1, 7).Value = "RelationName (Cha/Mẹ/Giám hộ)";
            sheet.Cell(1, 8).Value = "FullName_Child (Họ và tên con)";
            sheet.Cell(1, 9).Value = "Gender_Child (M/F)";
            sheet.Cell(1, 10).Value = "DateOfBirth_Child (dd/MM/yyyy)";
            sheet.Cell(1, 11).Value = "ClassId (ID lớp học)";

            // 💅 Định dạng tiêu đề
            var header = sheet.Range("A1:K1");
            header.Style.Font.Bold = true;
            header.Style.Fill.BackgroundColor = XLColor.LightGray;
            header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // 📐 Căn chỉnh & tự động giãn cột
            sheet.Columns().AdjustToContents();

            // 🧩 Dòng ví dụ minh họa
            sheet.Cell(2, 1).Value = "Nguyễn Văn A";
            sheet.Cell(2, 2).Value = "a@gmail.com";
            sheet.Cell(2, 3).Value = "0901234567";
            sheet.Cell(2, 4).Value = "123456";
            sheet.Cell(2, 5).Value = "M";
            sheet.Cell(2, 6).Value = "01/01/1980";
            sheet.Cell(2, 7).Value = "Cha";
            sheet.Cell(2, 8).Value = "Nguyễn Minh An";
            sheet.Cell(2, 9).Value = "M";
            sheet.Cell(2, 10).Value = "15/09/2015";
            sheet.Cell(2, 11).Value = "GUID của lớp học";

            // 🟣 Sheet 2: Hướng dẫn
            var guide = workbook.Worksheets.Add("Hướng dẫn");

            guide.Cell(1, 1).Value = "👉 HƯỚNG DẪN NHẬP FILE EXCEL";
            guide.Cell(2, 1).Value = "- Vui lòng không thay đổi tiêu đề cột ở sheet 'Danh sách phụ huynh'";
            guide.Cell(3, 1).Value = "- Cột 'RelationName': nhập Cha, Mẹ hoặc Giám hộ";
            guide.Cell(4, 1).Value = "- Cột 'Gender_Parent' và 'Gender_Child': chỉ nhập Nam hoặc Nữ";
            guide.Cell(5, 1).Value = "- Cột 'DateOfBirth_*': định dạng ngày/tháng/năm (dd/MM/yyyy)";
            guide.Cell(6, 1).Value = "- Cột 'ClassId': sao chép ID lớp học tương ứng trong hệ thống";

            guide.Columns().AdjustToContents();

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return await Task.FromResult(stream.ToArray());
            }
        }
    }

}
