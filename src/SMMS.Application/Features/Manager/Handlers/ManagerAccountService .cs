using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.Manager.DTOs;
using SMMS.Application.Features.Manager.Interfaces;
using SMMS.Domain.Entities.auth;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Entities.school;
namespace SMMS.Application.Features.Manager.Handlers;
public class ManagerAccountService : IManagerAccountService
{
    private readonly IManagerAccountRepository _repo;

    public ManagerAccountService(IManagerAccountRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<AccountDto>> SearchAccountsAsync(Guid schoolId, string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return new List<AccountDto>();

        keyword = keyword.Trim().ToLower();

        return await _repo.Users
            .Include(u => u.Role)
            .Where(u => u.SchoolId == schoolId &&
                (u.FullName.ToLower().Contains(keyword) ||
                 (u.Email != null && u.Email.ToLower().Contains(keyword)) ||
                 (u.Phone != null && u.Phone.Contains(keyword)) ||
                 (u.Role.RoleName.ToLower().Contains(keyword))))
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new AccountDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email ?? string.Empty,
                Phone = u.Phone,
                Role = u.Role.RoleName,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();
    }
    // 🟢 Lọc danh sách nhân viên theo Role (tách riêng API)
    public async Task<List<AccountDto>> FilterByRoleAsync(Guid schoolId, string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("Role không được để trống.", nameof(role));

        role = role.Trim().ToLower();

        return await _repo.Users
            .Include(u => u.Role)
            .Where(u => u.SchoolId == schoolId && u.Role.RoleName.ToLower() == role)
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new AccountDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email ?? string.Empty,
                Phone = u.Phone,
                Role = u.Role.RoleName,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();
    }

    // 🧑‍🍳👮‍♂️ Lấy danh sách toàn bộ nhân viên (KitchenStaff + Warden)
    public async Task<List<AccountDto>> GetAllAsync(Guid schoolId)
    {
        // Danh sách các vai trò staff cần lấy
        var staffRoles = new[] { "kitchenstaff", "warden","teacher" };

        return await _repo.Users
            .Include(u => u.Role)
            .Where(u => u.SchoolId == schoolId &&
                        staffRoles.Contains(u.Role.RoleName.ToLower()))
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new AccountDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email ?? string.Empty,
                Phone = u.Phone,
                Role = u.Role.RoleName,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<AccountDto> CreateAsync(CreateAccountRequest request)
    {
        // 🔹 Kiểm tra trùng email hoặc số điện thoại
        var exists = await _repo.Users.AnyAsync(u =>
            u.Email == request.Email || u.Phone == request.Phone);
        if (exists)
            throw new InvalidOperationException("Email hoặc số điện thoại đã tồn tại.");

        // 🔹 Tìm RoleId theo RoleName
        var role = await _repo.Roles
            .FirstOrDefaultAsync(r => r.RoleName.ToLower() == request.Role.ToLower());
        if (role == null)
            throw new InvalidOperationException("Không tìm thấy vai trò hợp lệ.");

        // 🔹 Tạo user cơ bản
        var user = new User
        {
            UserId = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = request.Email?.Trim().ToLower(),
            Phone = request.Phone.Trim(),
            PasswordHash = request.Password, // TODO: mã hóa
            RoleId = role.RoleId,
            LanguagePref = "vi",
            SchoolId = request.SchoolId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy
        };

        await _repo.AddAsync(user);

        // 🟡 Nếu là teacher hoặc warden → thêm vào bảng Teachers
        if (role.RoleName.Equals("teacher", StringComparison.OrdinalIgnoreCase) ||
         role.RoleName.Equals("warden", StringComparison.OrdinalIgnoreCase))
        {
            var teacher = new Teacher
            {
                TeacherId = user.UserId,
                EmployeeCode = "EMP-" + DateTime.UtcNow.Ticks.ToString()[^6..], // tạo mã nhân viên tạm
                HiredDate = DateOnly.FromDateTime(DateTime.UtcNow),
                IsActive = true
            };

            await _repo.AddTeacherAsync(teacher);
        }

        return new AccountDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Phone = user.Phone,
            Role = role.RoleName,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    // 🟠 Cập nhật tài khoản
    public async Task<AccountDto?> UpdateAsync(Guid userId, UpdateAccountRequest request)
    {
        var user = await _repo.GetByIdAsync(userId);
        if (user == null)
            return null;

        Role? role = null; // 🔹 Khai báo trước

        if (!string.IsNullOrWhiteSpace(request.FullName))
            user.FullName = request.FullName.Trim();
        if (!string.IsNullOrWhiteSpace(request.Email))
            user.Email = request.Email.Trim().ToLower();
        if (!string.IsNullOrWhiteSpace(request.Phone))
            user.Phone = request.Phone.Trim();
        if (!string.IsNullOrWhiteSpace(request.Password))
            user.PasswordHash = request.Password; // TODO: hash sau
        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            role = await _repo.Roles.FirstOrDefaultAsync(r => r.RoleName == request.Role);
            if (role != null)
                user.RoleId = role.RoleId;
        }

        user.UpdatedBy = request.UpdatedBy;
        user.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(user);

        return new AccountDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Role = role?.RoleName ?? user.Role?.RoleName ?? "(unknown)", // ✅ tránh null
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }


    // 🔵 Đổi trạng thái kích hoạt
    public async Task<bool> ChangeStatusAsync(Guid userId, bool isActive)
    {
        var user = await _repo.GetByIdAsync(userId);
        if (user == null)
            return false;

        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(user);
        return true;
    }

    // 🔴 Xóa tài khoản
    public async Task<bool> DeleteAsync(Guid userId)
    {
        var user = await _repo.GetByIdAsync(userId);
        if (user == null)
            return false;

        await _repo.DeleteAsync(user);
        return true;
    }
}
