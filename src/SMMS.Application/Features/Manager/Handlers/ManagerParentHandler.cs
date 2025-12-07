using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using MediatR;
using Microsoft.Extensions.Logging;
using SMMS.Application.Features.Manager.Commands;
using SMMS.Application.Features.Manager.DTOs;
using SMMS.Application.Features.Manager.Interfaces;
using SMMS.Application.Features.Manager.Queries;
using SMMS.Domain.Entities.auth;
using SMMS.Domain.Entities.school;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace SMMS.Application.Features.Manager.Handlers;

public class ManagerParentHandler :
    IRequestHandler<SearchParentsQuery, List<ParentAccountDto>>,
    IRequestHandler<GetParentsQuery, List<ParentAccountDto>>,
    IRequestHandler<CreateParentCommand, AccountDto>,
    IRequestHandler<UpdateParentCommand, AccountDto?>,
    IRequestHandler<ChangeParentStatusCommand, bool>,
    IRequestHandler<DeleteParentCommand, bool>,
    IRequestHandler<ImportParentsFromExcelCommand, List<AccountDto>>,
    IRequestHandler<GetParentExcelTemplateQuery, byte[]>
{
    private readonly IManagerAccountRepository _repo;
    private readonly ILogger<ManagerParentHandler> _logger;
    private readonly IManagerRepository _managerRepo;
    private readonly PasswordHasher<User> _passwordHasher;
    public ManagerParentHandler(
        IManagerAccountRepository repo,
         IManagerRepository managerRepo,
        ILogger<ManagerParentHandler> logger)
    {
        _repo = repo;
        _logger = logger;
        _managerRepo = managerRepo;
        _passwordHasher = new PasswordHasher<User>();
    }

   #region 🔍 SearchAsync

    public async Task<List<ParentAccountDto>> Handle(
        SearchParentsQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Keyword))
            return new List<ParentAccountDto>();

        var keyword = request.Keyword.Trim().ToLower();

        var query = _repo.Users
            .Include(u => u.Role)
            .Include(u => u.School)
            .Include(u => u.Students)
                .ThenInclude(s => s.StudentClasses)
                    .ThenInclude(sc => sc.Class)
            .Where(u =>
                u.SchoolId == request.SchoolId &&
                u.Role.RoleName.ToLower() == "parent" &&
                (
                    // Tìm theo thông tin phụ huynh
                    u.FullName.ToLower().Contains(keyword) ||
                    (u.Email != null && u.Email.ToLower().Contains(keyword)) ||
                    (u.Phone != null && u.Phone.ToLower().Contains(keyword)) ||

                    // Tìm theo thông tin con hoặc lớp học của con
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

                // ✅ Cập nhật: Lấy RelationName giống GetAll
                RelationName = u.Students.Any() ? u.Students.FirstOrDefault().RelationName : "Phụ huynh",

                // ✅ Cập nhật: Map danh sách con chi tiết giống GetAll
                Children = u.Students.Select(s => new ParentAccountDto.ParentStudentDetailDto
                {
                    FullName = s.FullName,
                    Gender = s.Gender,
                    DateOfBirth = s.DateOfBirth.HasValue
                        ? s.DateOfBirth.Value.ToDateTime(TimeOnly.MinValue)
                        : null,
                    ClassId = s.StudentClasses.Any() ? s.StudentClasses.FirstOrDefault().ClassId : (Guid?)null,
                    ClassName = s.StudentClasses.Any() && s.StudentClasses.FirstOrDefault().Class != null
                        ? s.StudentClasses.FirstOrDefault().Class.ClassName
                        : ""
                }).ToList()
            })
            .ToListAsync(cancellationToken);
    }

    #endregion

    private async Task<Dictionary<Guid, bool>> BuildStudentUnpaidMapAsync(
    IEnumerable<Guid> studentIds,
    CancellationToken ct)
    {
        var idList = studentIds.Distinct().ToList();
        if (!idList.Any()) return new Dictionary<Guid, bool>();

        // Lấy invoice theo học sinh và xem có Unpaid hay không
        var data = await _managerRepo.Invoices
            .Where(i => idList.Contains(i.StudentId))
            .GroupBy(i => i.StudentId)
            .Select(g => new
            {
                StudentId = g.Key,
                HasUnpaid = g.Any(x => x.Status == "Unpaid")
            })
            .ToListAsync(ct);

        return data.ToDictionary(x => x.StudentId, x => x.HasUnpaid);
    }


    #region 🟢 GetAllAsync

    public async Task<List<ParentAccountDto>> Handle(
        GetParentsQuery request,
        CancellationToken cancellationToken)
    {
        var schoolId = request.SchoolId;
        var classIdFilter = request.ClassId;

        var query = _repo.Users
            .Include(u => u.Role)
            .Include(u => u.School)
            .Include(u => u.Students)
                .ThenInclude(s => s.StudentClasses)
                    .ThenInclude(sc => sc.Class)
            .Where(u =>
                u.Role.RoleName.ToLower() == "parent" &&
                u.IsActive && // nếu chỉ muốn phụ huynh active
                              // ❗ chỉ tính các con active ở đúng school
                u.Students.Any(s => s.SchoolId == schoolId && s.IsActive)
            );

        if (classIdFilter.HasValue)
        {
            var classId = classIdFilter.Value;

            query = query.Where(u =>
                u.Students.Any(s =>
                    s.SchoolId == schoolId &&
                    s.IsActive &&
                    s.StudentClasses.Any(sc => sc.ClassId == classId)
                )
            );
        }
        // 1️⃣ Lấy list user trước
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync(cancellationToken);

        // 🔹 Lấy tất cả StudentId liên quan
        var allStudentIds = users
            .SelectMany(u => u.Students
                .Where(s =>
                    s.SchoolId == schoolId &&
                    s.IsActive &&
                    (!classIdFilter.HasValue ||
                     s.StudentClasses.Any(sc => sc.ClassId == classIdFilter.Value)))
                .Select(s => s.StudentId))
            .Distinct()
            .ToList();

        // 🔹 Map StudentId -> HasUnpaid (true/false)
        var studentUnpaidMap = await BuildStudentUnpaidMapAsync(allStudentIds, cancellationToken);

        // 2️⃣ Map sang DTO
        var result = users
            .Select(u =>
            {
                bool isDefaultPassword = false;

                if (!string.IsNullOrWhiteSpace(u.PasswordHash) &&
                    u.PasswordHash.StartsWith("AQAAAA", StringComparison.Ordinal))
                {
                    var verify = _passwordHasher.VerifyHashedPassword(u, u.PasswordHash, "@1");
                    isDefaultPassword = verify == PasswordVerificationResult.Success;
                }

                if (u.PasswordHash == "@1")
                {
                    isDefaultPassword = true;
                }

                var childrenInSchool = u.Students
                    .Where(s =>
                        s.SchoolId == schoolId &&
                        s.IsActive &&
                        (!classIdFilter.HasValue ||
                         s.StudentClasses.Any(sc => sc.ClassId == classIdFilter.Value)))
                    .ToList();

                var childIds = childrenInSchool.Select(s => s.StudentId).ToList();

                // 🔥 Tính trạng thái thanh toán cho phụ huynh
                var hasAnyInvoice = childIds.Any(id => studentUnpaidMap.ContainsKey(id))
                                    || childIds.Any(id => studentUnpaidMap.ContainsKey(id) == false);
                // Có học sinh nhưng không có record trong map => chưa có invoice nào

                var hasUnpaid = childIds.Any(id =>
                    studentUnpaidMap.TryGetValue(id, out var flag) && flag);

                string paymentStatus;
                if (!childIds.Any())
                {
                    paymentStatus = "Chưa tạo hóa đơn";
                }
                else if (hasUnpaid)
                {
                    paymentStatus = "Chưa thanh toán";
                }
                else if (childIds.Any(id => studentUnpaidMap.ContainsKey(id)))
                {
                    paymentStatus = "Đã thanh toán";
                }
                else
                {
                    paymentStatus = "Chưa tạo hóa đơn";
                }

                return new ParentAccountDto
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    Phone = u.Phone,
                    Role = u.Role.RoleName,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    SchoolName = u.School != null ? u.School.SchoolName : "(Chưa gán trường)",

                    IsDefaultPassword = isDefaultPassword,
                    PaymentStatus = paymentStatus,   // 👈 Gán trạng thái thanh toán

                    RelationName = childrenInSchool
                        .Select(s => s.RelationName ?? "Phụ huynh")
                        .FirstOrDefault() ?? "Phụ huynh",

                    Children = childrenInSchool
                        .Select(s => new ParentAccountDto.ParentStudentDetailDto
                        {
                            StudentId = s.StudentId,
                            FullName = s.FullName,
                            Gender = s.Gender,
                            DateOfBirth = s.DateOfBirth.HasValue
                                ? s.DateOfBirth.Value.ToDateTime(TimeOnly.MinValue)
                                : null,
                            ClassId = s.StudentClasses.FirstOrDefault()?.ClassId,
                            ClassName = s.StudentClasses.FirstOrDefault()?.Class?.ClassName
                                        ?? "Chưa xếp lớp"
                        })
                        .ToList()
                };
            })
            .ToList();

        return result;
    }

    #endregion

    #region 🟡 CreateAsync

    public async Task<AccountDto> Handle(
        CreateParentCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;

        var role = await _repo.Roles
            .FirstOrDefaultAsync(r => r.RoleName.ToLower() == "parent", cancellationToken);

        if (role == null)
            throw new InvalidOperationException("Không tìm thấy vai trò 'Parent'.");

        var normalizedEmail = string.IsNullOrWhiteSpace(request.Email)
            ? null
            : request.Email.Trim().ToLower();

        // 🔍 Tìm phụ huynh đã tồn tại theo email/phone (toàn hệ thống)
        var existingParent = await _repo.Users
            .FirstOrDefaultAsync(
                u =>
                    ((normalizedEmail != null && u.Email == normalizedEmail) ||
                     u.Phone == request.Phone),
                cancellationToken
            );

        User parent;

        if (existingParent != null)
        {
            // ✅ Đã có phụ huynh trong hệ thống
            // (tuỳ bạn có cần kiểm tra RoleId hay không)
            if (existingParent.RoleId != role.RoleId)
            {
                throw new InvalidOperationException("Tài khoản trùng thông tin nhưng không phải vai trò phụ huynh.");
            }

            parent = existingParent;

            // ❗ Không sửa password, không sửa email/phone
            // ❗ Tuỳ business, bạn có thể cân nhắc có nên sửa SchoolId hay không
            // Ví dụ: nếu 1 phụ huynh có thể thuộc nhiều trường thì field SchoolId trên User
            // không nên dùng để ràng buộc, mà nên tách bảng ParentSchool riêng.
        }
        else
        {
            // 🆕 Chưa có phụ huynh -> tạo mới
            parent = new User
            {
                UserId = Guid.NewGuid(),
                FullName = request.FullName.Trim(),
                Email = normalizedEmail,
                Phone = request.Phone.Trim(),
                RoleId = role.RoleId,
                SchoolId = request.SchoolId,   // trường đầu tiên mà phụ huynh được tạo
                LanguagePref = "vi",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.CreatedBy
            };

            // ✅ Hash password khi tạo mới
            parent.PasswordHash = _passwordHasher.HashPassword(parent, request.Password);

            await _repo.AddAsync(parent);
        }

        // 👶 Tạo (thêm) con – luôn chạy, dù là phụ huynh mới hay cũ
        foreach (var child in request.Children)
        {
            var student = new Student
            {
                StudentId = Guid.NewGuid(),
                FullName = child.FullName.Trim(),
                Gender = child.Gender,
                DateOfBirth = child.DateOfBirth != null
                    ? DateOnly.FromDateTime(child.DateOfBirth.Value)
                    : null,
                SchoolId = request.SchoolId,        // 🔁 trường hiện tại đang add (trường 2)
                ParentId = parent.UserId,           // 🔁 gắn với phụ huynh đã tìm được / vừa tạo
                RelationName = request.RelationName ?? "Phụ huynh",
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

    #endregion

    #region 🟠 UpdateAsync

    public async Task<AccountDto?> Handle(
        UpdateParentCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;

        var user = await _repo.Users
            .Include(u => u.Role)
            .Include(u => u.Students)
                .ThenInclude(s => s.StudentClasses)
            .FirstOrDefaultAsync(u => u.UserId == command.UserId, cancellationToken);

        if (user == null || user.Role.RoleName.ToLower() != "parent")
            return null;

        // 🔐 Chỉ cho phép cập nhật nếu phụ huynh còn dùng mật khẩu mặc định "@1"
        if (string.IsNullOrEmpty(user.PasswordHash))
            return null;

        var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, "@1");
        var isDefaultPassword = verifyResult == PasswordVerificationResult.Success;

        if (!isDefaultPassword)
            return null;

        // ✅ Đến đây chắc chắn user đang dùng mật khẩu @1 => cho phép update full

        // update parent
        if (!string.IsNullOrWhiteSpace(request.FullName))
            user.FullName = request.FullName.Trim();
        if (!string.IsNullOrWhiteSpace(request.Email))
            user.Email = request.Email.Trim().ToLower();
        if (!string.IsNullOrWhiteSpace(request.Phone))
            user.Phone = request.Phone.Trim();
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
        }
        if (!string.IsNullOrWhiteSpace(request.Gender))
            user.LanguagePref = request.Gender;

        user.UpdatedBy = request.UpdatedBy;
        user.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(user);

        // 🔄 Update / tạo con
        if (request.Children != null && request.Children.Any())
        {
            foreach (var childDto in request.Children)
            {
                Student? existingChild = null;

                // ⭐ ƯU TIÊN tìm theo StudentId
                if (childDto.StudentId.HasValue)
                {
                    existingChild = user.Students
                        .FirstOrDefault(s => s.StudentId == childDto.StudentId.Value);
                }

                // Nếu không có StudentId (cũ), fallback theo tên + parent (rủi ro nhưng tạm)

                if (existingChild != null)
                {
                    // 🔁 Cập nhật học sinh
                    if (!string.IsNullOrWhiteSpace(childDto.FullName))
                        existingChild.FullName = childDto.FullName.Trim();

                    if (!string.IsNullOrWhiteSpace(childDto.Gender))
                        existingChild.Gender = childDto.Gender;

                    if (childDto.DateOfBirth.HasValue)
                        existingChild.DateOfBirth = DateOnly.FromDateTime(childDto.DateOfBirth.Value);

                    existingChild.RelationName = request.RelationName ?? "Phụ huynh";
                    existingChild.UpdatedAt = DateTime.UtcNow;

                    await _repo.UpdateStudentAsync(existingChild);

                    // (option) nếu muốn update luôn lớp: xoá class cũ / thêm class mới ở đây
                    // ⬇⬇⬇ THÊM ĐOẠN NÀY Ở SAU VÒNG foreach ⬇⬇⬇

                    // Các StudentId còn muốn giữ lại (chỉ lấy những thằng có StudentId)
                    var keepIds = request.Children
                        .Where(c => c.StudentId.HasValue)
                        .Select(c => c.StudentId!.Value)
                        .ToHashSet();

                    // Những đứa đang tồn tại mà không còn trong danh sách keepIds => xoá
                    var childrenToDelete = user.Students
                        .Where(s => !keepIds.Contains(s.StudentId))
                        .ToList();

                    foreach (var child in childrenToDelete)
                    {
                        // nếu có StudentClasses và bạn muốn xoá luôn thì làm thêm:
                        // foreach (var sc in child.StudentClasses.ToList())
                        // {
                        //     await _repo.DeleteStudentClassAsync(sc);
                        // }

                        await _repo.DeleteStudentAsync(child); // hard delete

                        // hoặc soft delete:
                        // child.IsActive = false;
                        // await _repo.UpdateStudentAsync(child);
                    }
                }
                else
                {
                    // 🆕 Hoàn toàn không tìm thấy => tạo học sinh mới
                    var newStudent = new Student
                    {
                        StudentId = Guid.NewGuid(),
                        FullName = childDto.FullName!.Trim(),
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
                        ClassId = childDto.ClassId.Value,
                        JoinedDate = DateOnly.FromDateTime(DateTime.UtcNow),
                        RegistStatus = true
                    };

                    await _repo.AddStudentClassAsync(studentClass);
                }
            }
        }

        return new AccountDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Phone = user.Phone ?? string.Empty,
            Role = "Parent",
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    #endregion


    #region 🔵 ChangeStatusAsync

    public async Task<bool> Handle(
        ChangeParentStatusCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _repo.GetByIdAsync(command.UserId);
        if (user == null || user.Role.RoleName.ToLower() != "parent")
            return false;

        user.IsActive = command.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(user);
        return true;
    }

    #endregion

    #region 🔴 DeleteAsync

    public async Task<bool> Handle(
        DeleteParentCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _repo.Users
            .Include(u => u.Role)
            .Include(u => u.Students)
                .ThenInclude(s => s.StudentClasses)
            .FirstOrDefaultAsync(u => u.UserId == command.UserId, cancellationToken);

        if (user == null ||
            user.Role == null ||
            !string.Equals(user.Role.RoleName, "parent", StringComparison.OrdinalIgnoreCase))
            return false;

        // 🔸 Học sinh thuộc TRƯỜNG hiện tại
        var studentsInThisSchool = user.Students
            .Where(s => s.SchoolId == command.SchoolId)
            .ToList();

        if (!studentsInThisSchool.Any())
            return true; // không có con nào ở trường này nữa -> coi như xoá xong trong context trường này

        // 🔥 Xóa StudentClass + Student thuộc TRƯỜNG hiện tại
        foreach (var student in studentsInThisSchool)
        {
            var studentClassesToDelete = student.StudentClasses.ToList();

            foreach (var sc in studentClassesToDelete)
            {
                await _repo.DeleteStudentClassAsync(sc);
            }

            // xoá hẳn học sinh ở TRƯỜNG NÀY
            await _repo.DeleteStudentAsync(student);
        }
        await _repo.DeleteNotificationRecipientsByUserIdAsync(user.UserId);

        // ✅ Sau khi xoá con ở trường này, kiểm tra xem parent còn con ở trường nào khác không
        var hasAnyStudentOtherSchool = await _repo.Students
            .AnyAsync(s => s.ParentId == user.UserId, cancellationToken);

        if (!hasAnyStudentOtherSchool)
        {
            // ❌ Không còn bất kỳ con nào ở bất cứ trường nào -> xoá luôn tài khoản parent
            await _repo.DeleteAsync(user);
        }
        else
        {
            // ✔ Vẫn còn con ở trường khác -> chỉ cập nhật thời gian
            user.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(user);
        }

        return true;
    }

    #endregion
    #region 📥 ImportFromExcelAsync

    public async Task<List<AccountDto>> Handle(
        ImportParentsFromExcelCommand command,
        CancellationToken cancellationToken)
    {
        var (schoolId, file, createdBy) = (command.SchoolId, command.File, command.CreatedBy);
        var result = new List<AccountDto>();

        if (file == null || file.Length == 0)
            throw new InvalidOperationException("Không có file được tải lên.");

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream, cancellationToken);
        using var workbook = new XLWorkbook(stream);
        var sheet = workbook.Worksheet("Danh sách phụ huynh");

        if (sheet == null)
            throw new InvalidOperationException("Không tìm thấy sheet 'Danh sách phụ huynh' trong file Excel.");

        var role = await _repo.Roles
            .FirstOrDefaultAsync(r => r.RoleName.ToLower() == "parent", cancellationToken);
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
                if (string.IsNullOrWhiteSpace(password))
                    password = "@1";
                var genderParent = sheet.Cell(row, 5).GetString()?.Trim();
                var dobParent = sheet.Cell(row, 6).GetString()?.Trim();
                var relationName = sheet.Cell(row, 7).GetString()?.Trim();

                var fullNameChild = sheet.Cell(row, 8).GetString()?.Trim();
                var genderChild = sheet.Cell(row, 9).GetString()?.Trim();
                var dobChild = sheet.Cell(row, 10).GetString()?.Trim();
                var classIdStr = sheet.Cell(row, 11).GetString()?.Trim();

                if (string.IsNullOrWhiteSpace(fullNameParent) || string.IsNullOrWhiteSpace(phone))
                    throw new InvalidOperationException($"Thiếu thông tin bắt buộc tại dòng {row}: FullName_Parent hoặc Phone.");

                var normalizedEmail = string.IsNullOrWhiteSpace(email)
                ? null
                : email.ToLower();
                var exists = await _repo.Users.AnyAsync(
                    u => normalizedEmail != null && u.Email == normalizedEmail || u.Phone == phone,
                    cancellationToken);

                if (exists)
                    throw new InvalidOperationException(
                        normalizedEmail == null
                            ? "Số điện thoại đã tồn tại."
                            : "Email hoặc số điện thoại đã tồn tại."
                    );

                var parent = new User
                {
                    UserId = Guid.NewGuid(),
                    FullName = fullNameParent,
                    Email = normalizedEmail,
                    Phone = phone,
                    RoleId = role.RoleId,
                    SchoolId = schoolId,
                    LanguagePref = "vi",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                // ✅ hash password bằng PasswordHasher
                parent.PasswordHash = _passwordHasher.HashPassword(parent, password);
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

    #endregion

    #region 📄 GetExcelTemplateAsync

    public async Task<byte[]> Handle(
        GetParentExcelTemplateQuery request,
        CancellationToken cancellationToken)
    {
        using var workbook = new XLWorkbook();

        var sheet = workbook.Worksheets.Add("Danh sách phụ huynh");
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

        var headerRange = sheet.Range(1, 1, 1, headers.Length);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

        // 👇 Định dạng cả cột Phone (cột 3) là Text
        var phoneColumn = sheet.Column(3);
        phoneColumn.Style.NumberFormat.Format = "@"; // "@" = Text
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

        sheet.Columns().AdjustToContents();
        sheet.Rows().AdjustToContents();

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

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return await Task.FromResult(stream.ToArray());
    }

    #endregion
}
