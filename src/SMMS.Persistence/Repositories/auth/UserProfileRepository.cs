using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.auth.DTOs;
using SMMS.Application.Features.auth.Interfaces;
using SMMS.Domain.Entities.auth;
using SMMS.Domain.Entities.nutrition;
using SMMS.Domain.Entities.school;
using SMMS.Persistence.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SMMS.Persistence.Repositories.Skeleton;

namespace SMMS.Persistence.Repositories.auth
{
    public class UserProfileRepository : Repository<User>, IUserProfileRepository
    {
        // 1. Thay IFileStorageService bằng CloudinaryService
        private readonly CloudinaryService _cloudinaryService;
        private readonly EduMealContext _dbContext;

        public UserProfileRepository(
            EduMealContext dbContext,
            CloudinaryService cloudinaryService) // Inject CloudinaryService
            : base(dbContext)
        {
            _dbContext = dbContext;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<UserProfileResponseDto> GetUserProfileAsync(Guid userId)
        {
            var user = await _dbContext.Users
                .Include(u => u.Students)
                    .ThenInclude(s => s.StudentAllergens)
                    .ThenInclude(sa => sa.Allergen)
                .Include(u => u.Students)
                    .ThenInclude(s => s.StudentClasses)
                    .ThenInclude(sc => sc.Class)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                throw new Exception($"Không tìm thấy người dùng với ID: {userId}");

            var childrenWithAllergies = new List<ChildProfileResponseDto>();

            foreach (var student in user.Students)
            {
                var allergenNames = student.StudentAllergens?
                    .Select(sa => sa.Allergen?.AllergenName)
                    .Where(name => !string.IsNullOrEmpty(name))
                    .ToList() ?? new List<string>();

                var className = student.StudentClasses?
                    .Where(sc => sc.LeftDate == null)
                    .FirstOrDefault()?.Class?.ClassName;

                childrenWithAllergies.Add(new ChildProfileResponseDto
                {
                    StudentId = student.StudentId,
                    FullName = student.FullName,
                    AvatarUrl = student.AvatarUrl,
                    Relation = student.RelationName,
                    AllergyFoods = allergenNames,
                    ClassName = className ?? "Chưa xếp lớp",
                    DateOfBirth = student.DateOfBirth,
                    Gender = student.Gender,
                });
            }

            return new UserProfileResponseDto
            {
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                DateOfBirth = user.DateOfBirth.ToString(),
                AvatarUrl = user.AvatarUrl,
                Children = childrenWithAllergies
            };
        }

        public async Task<bool> UpdateUserProfileAsync(Guid userId, UpdateUserProfileDto dto)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
                throw new Exception($"Không tìm thấy người dùng với ID: {userId}");

            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.Phone = dto.Phone;
            user.DateOfBirth = dto.DateOfBirth;

            if (dto.AvatarFile != null)
            {
                // Upload file mới và cập nhật URL
                user.AvatarUrl = await UploadUserAvatarAsync(dto.AvatarFile, userId);
            }
            else if (!string.IsNullOrEmpty(dto.AvatarUrl))
            {
                // Nếu frontend gửi string URL (đã upload trước đó hoặc link cũ)
                user.AvatarUrl = dto.AvatarUrl;
            }

            user.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return true;
        }

        // --- CẬP NHẬT LOGIC UPLOAD CLOUDINARY ---
        public async Task<string> UploadUserAvatarAsync(IFormFile file, Guid userId)
        {
            // Gọi CloudinaryService với folder "edu-meal/user-avatars"
            var avatarUrl = await _cloudinaryService.UploadImageAsync(file, "edu-meal/user-avatars");

            if (string.IsNullOrEmpty(avatarUrl))
                throw new Exception("Lỗi khi upload ảnh lên Cloudinary");

            // Cập nhật URL vào DB ngay lập tức (tuỳ chọn, vì hàm UpdateUserProfileAsync cũng sẽ save)
            var user = await _dbContext.Users.FindAsync(userId);
            if (user != null)
            {
                user.AvatarUrl = avatarUrl;
                user.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
            }

            return avatarUrl;
        }

        public async Task<string> UploadChildAvatarAsync(IFormFile file, Guid studentId)
        {
            // Gọi CloudinaryService với folder "edu-meal/student-avatars"
            var avatarUrl = await _cloudinaryService.UploadImageAsync(file, "edu-meal/student-avatars");

            if (string.IsNullOrEmpty(avatarUrl))
                throw new Exception("Lỗi khi upload ảnh lên Cloudinary");

            // Hàm này chỉ trả về URL để hàm UpdateChildInfoAsync dùng,
            // hoặc update DB nếu cần thiết (ở đây update luôn cho chắc)
            var student = await _dbContext.Students.FindAsync(studentId);
            if (student != null)
            {
                student.AvatarUrl = avatarUrl;
                student.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
            }

            return avatarUrl;
        }

        public async Task<ChildProfileResponseDto> UpdateChildInfoAsync(Guid parentId, ChildProfileDto childDto)
        {
            var student = await _dbContext.Students
                .Include(s => s.StudentAllergens)
                .Include(s => s.StudentClasses)
                .ThenInclude(sc => sc.Class)
                .FirstOrDefaultAsync(s => s.StudentId == childDto.StudentId && s.ParentId == parentId);

            if (student == null) return null;

            if (!string.IsNullOrEmpty(childDto.FullName)) student.FullName = childDto.FullName;
            if (!string.IsNullOrEmpty(childDto.Relation)) student.RelationName = childDto.Relation;
            if (childDto.DateOfBirth.HasValue) student.DateOfBirth = childDto.DateOfBirth.Value;
            if (!string.IsNullOrEmpty(childDto.Gender)) student.Gender = childDto.Gender;

            // FIX: Logic cập nhật Avatar con tương tự cha
            if (childDto.AvatarFile != null)
            {
                student.AvatarUrl = await UploadChildAvatarAsync(childDto.AvatarFile, student.StudentId);
            }
            else if (!string.IsNullOrEmpty(childDto.AvatarUrl))
            {
                student.AvatarUrl = childDto.AvatarUrl;
            }

            await UpdateChildAllergiesAsync(student, childDto.AllergyFoods);

            student.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            var className = student.StudentClasses?
                .Where(sc => sc.LeftDate == null || sc.LeftDate > DateOnly.FromDateTime(DateTime.Now))
                .FirstOrDefault()?.Class?.ClassName;

            return new ChildProfileResponseDto
            {
                StudentId = student.StudentId,
                FullName = student.FullName,
                AvatarUrl = student.AvatarUrl,
                Relation = student.RelationName,
                AllergyFoods = childDto.AllergyFoods ?? new List<string>(),
                DateOfBirth = student.DateOfBirth,
                Gender = student.Gender,
                ClassName = className
            };
        }

        // ... Các hàm private UpdateChildAllergiesAsync và FindOrCreateAllergenAsync GIỮ NGUYÊN ...
        private async Task UpdateChildAllergiesAsync(Student student, List<string> allergyFoods)
        {
            var existingAllergies = await _dbContext.StudentAllergens
                .Where(sa => sa.StudentId == student.StudentId)
                .ToListAsync();

            if (existingAllergies.Any())
            {
                _dbContext.StudentAllergens.RemoveRange(existingAllergies);
            }

            if (allergyFoods == null) return; // Fix null check

            foreach (var foodName in allergyFoods.Where(f => !string.IsNullOrWhiteSpace(f)))
            {
                var allergen = await FindOrCreateAllergenAsync(foodName.Trim(), student.SchoolId);

                if (allergen != null)
                {
                    var studentAllergen = new StudentAllergen
                    {
                        StudentId = student.StudentId,
                        AllergenId = allergen.AllergenId,
                        DiagnosedAt = DateTime.UtcNow
                    };
                    await _dbContext.StudentAllergens.AddAsync(studentAllergen);
                }
            }
        }

        private async Task<Allergen> FindOrCreateAllergenAsync(string allergenName, Guid schoolId)
        {
            var existingAllergen = await _dbContext.Allergens
                .FirstOrDefaultAsync(a => a.AllergenName == allergenName && a.SchoolId == schoolId);

            if (existingAllergen != null)
                return existingAllergen;

            try
            {
                var newAllergen = new Allergen
                {
                    AllergenName = allergenName,
                    SchoolId = schoolId,
                    CreatedAt = DateTime.UtcNow
                };

                await _dbContext.Allergens.AddAsync(newAllergen);
                await _dbContext.SaveChangesAsync();

                return newAllergen;
            }
            catch (DbUpdateException ex)
            {
                var duplicateAllergen = await _dbContext.Allergens
                    .FirstOrDefaultAsync(a => a.AllergenName == allergenName && a.SchoolId == schoolId);

                if (duplicateAllergen != null)
                    return duplicateAllergen;

                throw new Exception($"Không thể tạo allergen mới: {ex.Message}");
            }
        }
    }
}
