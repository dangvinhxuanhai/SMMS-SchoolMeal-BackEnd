using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Manager.Commands;
using SMMS.Application.Features.Manager.DTOs;
using SMMS.Application.Features.Manager.Interfaces;
using SMMS.Application.Features.Manager.Queries;
using SMMS.Domain.Entities.auth;
using SMMS.Domain.Entities.school;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace SMMS.Application.Features.Manager.Handlers;

public class ManagerAccountHandler :
    // Queries
    IRequestHandler<SearchAccountsQuery, List<AccountDto>>,
    IRequestHandler<FilterByRoleQuery, List<AccountDto>>,
    IRequestHandler<GetAllStaffQuery, List<AccountDto>>,
    // Commands
    IRequestHandler<CreateAccountCommand, AccountDto>,
    IRequestHandler<UpdateAccountCommand, AccountDto?>,
    IRequestHandler<ChangeStatusCommand, bool>,
    IRequestHandler<DeleteAccountCommand, bool>
{
    private readonly IManagerAccountRepository _repo;
    private readonly PasswordHasher<User> _passwordHasher;
    public ManagerAccountHandler(IManagerAccountRepository repo)
    {
        _repo = repo;
        _passwordHasher = new PasswordHasher<User>();
    }

    #region QUERY HANDLERS

    // 🔍 Search accounts
    public async Task<List<AccountDto>> Handle(
        SearchAccountsQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Keyword))
            return new List<AccountDto>();

        var keyword = request.Keyword.Trim().ToLower();

        return await _repo.Users
            .Include(u => u.Role)
            .Where(u => u.SchoolId == request.SchoolId &&
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
            .ToListAsync(cancellationToken);
    }

    // 🔍 Filter by role
    public async Task<List<AccountDto>> Handle(
        FilterByRoleQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Role))
            throw new ArgumentException("Role không được để trống.", nameof(request.Role));

        var role = request.Role.Trim().ToLower();

        return await _repo.Users
            .Include(u => u.Role)
            .Where(u => u.SchoolId == request.SchoolId &&
                        u.Role.RoleName.ToLower() == role)
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
            .ToListAsync(cancellationToken);
    }

    // 👨‍🍳👮‍♂️ Get all staff (KitchenStaff + Warden + Teacher)
    public async Task<List<AccountDto>> Handle(
        GetAllStaffQuery request,
        CancellationToken cancellationToken)
    {
        var staffRoles = new[] { "kitchenstaff", "warden", "teacher" };

        return await _repo.Users
            .Include(u => u.Role)
            .Where(u => u.SchoolId == request.SchoolId &&
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
            .ToListAsync(cancellationToken);
    }

    #endregion

    #region COMMAND HANDLERS

    // ➕ Create account
    public async Task<AccountDto> Handle(
        CreateAccountCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;

        // Kiểm tra trùng email / phone
        var exists = await _repo.Users.AnyAsync(
            u => u.Email == request.Email || u.Phone == request.Phone,
            cancellationToken);

        if (exists)
            throw new InvalidOperationException("Email hoặc số điện thoại đã tồn tại.");

        // Tìm Role
        var role = await _repo.Roles
            .FirstOrDefaultAsync(r => r.RoleName.ToLower() == request.Role.ToLower(),
                cancellationToken);

        if (role == null)
            throw new InvalidOperationException("Không tìm thấy vai trò hợp lệ.");

        // Tạo User
        var user = new User
        {
            UserId = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = request.Email?.Trim().ToLower(),
            Phone = request.Phone.Trim(),
            RoleId = role.RoleId,
            LanguagePref = "vi",
            SchoolId = request.SchoolId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy
        };
        // ✅ Hash password bằng Identity
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
        // 🔄 sử dụng đúng method trong IManagerAccountRepository
        await _repo.AddAsync(user);

        // Nếu là teacher/warden -> thêm Teacher
        if (role.RoleName.Equals("teacher", StringComparison.OrdinalIgnoreCase) ||
            role.RoleName.Equals("warden", StringComparison.OrdinalIgnoreCase))
        {
            var teacher = new Teacher
            {
                TeacherId = user.UserId,
                EmployeeCode = "EMP-" + DateTime.UtcNow.Ticks.ToString()[^6..],
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

    // ✏️ Update account
    public async Task<AccountDto?> Handle(
        UpdateAccountCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;

        // GetByIdAsync không có CancellationToken trong interface
        var user = await _repo.GetByIdAsync(command.UserId);
        if (user == null)
            return null;

        Role? role = null;

        if (!string.IsNullOrWhiteSpace(request.FullName))
            user.FullName = request.FullName.Trim();

        if (!string.IsNullOrWhiteSpace(request.Email))
            user.Email = request.Email.Trim().ToLower();

        if (!string.IsNullOrWhiteSpace(request.Phone))
            user.Phone = request.Phone.Trim();

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            // ✅ Hash lại password nếu có đổi
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
        }

        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            role = await _repo.Roles
                .FirstOrDefaultAsync(r => r.RoleName == request.Role, cancellationToken);

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
            Email = user.Email ?? string.Empty,
            Phone = user.Phone ?? string.Empty,
            Role = role?.RoleName ?? user.Role?.RoleName ?? "(unknown)",
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    // 🔁 Change status
    public async Task<bool> Handle(
        ChangeStatusCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _repo.GetByIdAsync(command.UserId);
        if (user == null)
            return false;

        user.IsActive = command.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(user);
        return true;
    }

    // ❌ Delete account
    public async Task<bool> Handle(
        DeleteAccountCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _repo.GetByIdAsync(command.UserId);
        if (user == null)
            return false;

        await _repo.DeleteAsync(user);
        return true;
    }

    #endregion
}
