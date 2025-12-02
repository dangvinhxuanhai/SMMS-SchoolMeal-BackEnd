using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.auth.DTOs;
using SMMS.Application.Features.auth.Interfaces;
using SMMS.Application.Features.Identity.Interfaces;
using SMMS.Domain.Entities.auth;
using SMMS.Domain.Entities.nutrition;
using SMMS.Domain.Entities.school;
using SMMS.Persistence.Data;
using  SMMS.Persistence.Repositories.Skeleton;
namespace SMMS.Persistence.Repositories.auth
{
    public class UserProfileRepository : Repository<User>, IUserProfileRepository
    {
        private readonly IFileStorageService _fileStorageService;

        public UserProfileRepository(
            EduMealContext dbContext,
            IFileStorageService fileStorageService)
            : base(dbContext)
        {
            _fileStorageService = fileStorageService;
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
                    ClassName = className ?? "Chưa xếp lớp", // Xử lý null
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
                // FIX: Thêm prefix vào avatar cha
                AvatarUrl =user.AvatarUrl,
                Children = childrenWithAllergies
            };
        }

        public async Task<bool> UpdateUserProfileAsync(Guid userId, UpdateUserProfileDto dto)
        {
            var user = await GetByIdAsync(userId);
            if (user == null)
                throw new Exception($"Không tìm thấy người dùng với ID: {userId}");

            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.Phone = dto.Phone;
            user.DateOfBirth = dto.DateOfBirth;
            if (dto.AvatarFile != null)
            {
                user.AvatarUrl = await UploadUserAvatarAsync(dto.AvatarFile, userId);
            }
            user.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return true;
        }
        public async Task<string> UploadUserAvatarAsync(IFormFile file, Guid userId)
        {
            if (file == null || file.Length == 0)
                return null;
            // Lấy thông tin file
            var fileName = file.FileName;
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var fileData = ms.ToArray();
            // Tạo tên file mới để tránh trùng
            var newFileName = $"user_{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}";
            // Upload lên storage
            var avatarUrl = await _fileStorageService.SaveFileAsync(
                fileName,
                fileData,
                "edu-meal/user-avatars", // folder lưu avatar user
                newFileName
            );

            // Cập nhật URL avatar vào DB
            var user = await _dbContext.Users.FindAsync(userId); // hoặc table Parent nếu riêng
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
            if (file == null || file.Length == 0)
                return null;

            var fileName = file.FileName;
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var fileData = ms.ToArray();

            var newFileName = $"student_{studentId}_{DateTime.UtcNow:yyyyMMddHHmmss}";

            var url = await _fileStorageService.SaveFileAsync(
                fileName,
                fileData,
                "edu-meal/student-avatars",
                newFileName
            );

            // Chỉ trả URL, không SaveChangesAsync() ở đây
            return url;
        }


        public async Task<ChildProfileResponseDto> UpdateChildInfoAsync(Guid parentId, ChildProfileDto childDto)
        {
            var student = await _dbContext.Students
                .Include(s => s.StudentAllergens)
                .Include(s => s.StudentClasses)
                .ThenInclude(sc => sc.Class)
                .FirstOrDefaultAsync(s => s.StudentId == childDto.StudentId && s.ParentId == parentId);

            if (student == null) return null;
            if (!string.IsNullOrEmpty(childDto.FullName))
                student.FullName = childDto.FullName;

            if (!string.IsNullOrEmpty(childDto.Relation))
                student.RelationName = childDto.Relation;

            if (childDto.DateOfBirth.HasValue)
            {
                student.DateOfBirth = childDto.DateOfBirth.Value;
            }

            if (!string.IsNullOrEmpty(childDto.Gender))
            {
                student.Gender = childDto.Gender;
            }

            if (childDto.AvatarFile != null)
            {
                student.AvatarUrl = await UploadChildAvatarAsync(childDto.AvatarFile, student.StudentId);
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
                ClassName = className // ✅ Gán giá trị className vào response trả về
            };;
        }
        private async Task UpdateChildAllergiesAsync(Student student, List<string> allergyFoods)
        {
            var existingAllergies = await _dbContext.StudentAllergens
                .Where(sa => sa.StudentId == student.StudentId)
                .ToListAsync();

            if (existingAllergies.Any())
            {
                _dbContext.StudentAllergens.RemoveRange(existingAllergies);
            }

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
