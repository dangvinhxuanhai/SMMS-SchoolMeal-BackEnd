using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.school.Interfaces;
using SMMS.Persistence.Data;
using SMMS.Domain.Entities.school;
using DocumentFormat.OpenXml.Math;
using SMMS.Domain.Entities.auth;
using Microsoft.AspNetCore.Identity;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using SMMS.Persistence.Service;
using Microsoft.AspNetCore.Http;
namespace SMMS.Persistence.Repositories.schools
{
    public class SchoolRepository : ISchoolRepository
    {
        private readonly EduMealContext _context;
        private readonly CloudinaryService _cloudinaryService;
        private readonly PasswordHasher<User> _passwordHasher;

        public SchoolRepository(EduMealContext context, CloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
            _passwordHasher = new PasswordHasher<User>();
        }
        public IQueryable<School> GetAllSchools()
        {
            return _context.Schools
                .Include(s => s.Students)
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
                .FirstOrDefaultAsync(s => s.SchoolId == id);
        }

        public async Task AddAsync(School school, IFormFile? schoolContract)
        {
            if (schoolContract != null)
            {
                var imageUrl = await _cloudinaryService.UploadImageAsync(schoolContract);
                if (!string.IsNullOrEmpty(imageUrl))
                    school.SchoolContract = imageUrl;
            }
            // 🏫 1. Thêm trường
            _context.Schools.Add(school);
            await _context.SaveChangesAsync();

            // 👨‍🏫 2. Tạo tài khoản Manager cho trường
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

            // 🔐 3. Hash mật khẩu mặc định "@1"
            manager.PasswordHash = _passwordHasher.HashPassword(manager, "@1");

            // 💾 4. Lưu vào DB
            _context.Users.Add(manager);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(School school, IFormFile? schoolContract)
        {
            if (schoolContract != null)
            {
                var imageUrl = await _cloudinaryService.UploadImageAsync(schoolContract);
                if (!string.IsNullOrEmpty(imageUrl))
                    school.SchoolContract = imageUrl;
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
    }
}
