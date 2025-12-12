using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.school.Interfaces;
using SMMS.Persistence.Data;
using SMMS.Domain.Entities.school;
using SMMS.Domain.Entities.auth;
using Microsoft.AspNetCore.Identity;
using DocumentFormat.OpenXml.ExtendedProperties;

namespace SMMS.Persistence.Repositories.schools
{
    public class SchoolRepository : ISchoolRepository
    {
        private readonly EduMealContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public SchoolRepository(EduMealContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        public IQueryable<School> GetAllSchools()
        {
            return _context.Schools
                .Include(s => s.Students)
                .Include(s => s.Users)
                .AsNoTracking();
        }

        public async Task<List<School>> GetAllAsync()
        {
            return await _context.Schools
                .Include(s => s.Students)
                .Select(s => new School
                {
                    SchoolId = s.SchoolId,
                    SchoolName = s.SchoolName,
                    ContactEmail = s.ContactEmail,
                    Hotline = s.Hotline,
                    SchoolAddress = s.SchoolAddress,
                    IsActive = s.IsActive,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    Students = s.Students
                })
                .ToListAsync();
        }

        public async Task<School?> GetByIdAsync(Guid id)
        {
            return await _context.Schools
                .Include(s => s.Students)
                .Include(s => s.Users)
                .FirstOrDefaultAsync(s => s.SchoolId == id);
        }

        public async Task AddAsync(School school)
        {
            // ❌ Không cho phép trùng Email trường
            var emailExists = await _context.Schools
                .AnyAsync(s => s.ContactEmail == school.ContactEmail);
            if (emailExists)
                throw new Exception("Email trường đã tồn tại. Vui lòng chọn email khác.");
            // ❌ Không cho phép trùng Hotline
            var phoneExists = await _context.Schools
                .AnyAsync(s => s.Hotline == school.Hotline);
            if (phoneExists)
                throw new Exception("Số điện thoại trường đã tồn tại. Vui lòng chọn số khác.");
            // 1. Thêm trường
            _context.Schools.Add(school);
            await _context.SaveChangesAsync();

            // 2. Tạo tài khoản Manager
            var manager = new User
            {
                UserId = Guid.NewGuid(),
                Email = school.ContactEmail ?? $"{school.SchoolName.Replace(" ", "").ToLower()}@school.local",
                FullName = $"{school.SchoolName} Manager",
                Phone = school.Hotline ?? "0982441549",
                LanguagePref = "vi",
                RoleId = 2,
                SchoolId = school.SchoolId,
                IsActive = true,
                CreatedAt = DateTime.Now,
                AccessFailedCount = 0,
                LockoutEnabled = false
            };

            manager.PasswordHash = _passwordHasher.HashPassword(manager, "@1");

            _context.Users.Add(manager);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(School school, bool? managerIsActive = null)
        {
            var manager = await _context.Users
             .FirstOrDefaultAsync(u => u.SchoolId == school.SchoolId && u.RoleId == 2);
            if (manager != null)
            {
                // Nếu Hotline của school thay đổi, update luôn phone của manager
                if (!string.IsNullOrEmpty(school.Hotline) && school.Hotline != manager.Phone)
                {
                    manager.Phone = school.Hotline;
                    manager.UpdatedAt = DateTime.Now;
                }
                if (!string.IsNullOrEmpty(school.ContactEmail) && school.ContactEmail != manager.Email)
                {
                    manager.Email = school.ContactEmail;
                    manager.UpdatedAt = DateTime.Now;
                }
                // Nếu cần update trạng thái manager
                if (managerIsActive.HasValue)
                {
                    manager.IsActive = managerIsActive.Value;
                    manager.UpdatedAt = DateTime.Now;
                }

                _context.Users.Update(manager);
            }

            _context.Schools.Update(school);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.Schools.FindAsync(id);
            if (entity != null)
            {
                _context.Schools.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<bool> UpdateManagerStatusAsync(Guid schoolId, bool isActive)
        {
            // Tìm Manager của trường (RoleId = 2)
            var manager = await _context.Users
                .FirstOrDefaultAsync(u => u.SchoolId == schoolId && u.RoleId == 2);

            if (manager == null)
                throw new Exception("Trường này chưa có Manager.");

            manager.IsActive = isActive;
            manager.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool?> GetManagerStatusAsync(Guid schoolId)
        {
            var manager = await _context.Users
                .FirstOrDefaultAsync(u => u.SchoolId == schoolId && u.RoleId == 2);

            if (manager == null)
                return null; // Không có manager

            return manager.IsActive;
        }

        public async Task<bool> AnyNeedRebuildAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Schools.AnyAsync(s => s.NeedRebuildAiIndex == false, cancellationToken);
            // schools có NeedRebuildAiIndex = false nghia la truong do can dc build lai AI index
        }
    }
}
