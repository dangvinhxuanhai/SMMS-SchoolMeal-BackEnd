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
                    AllergyFoods = allergenNames,
                    ClassName = className
                });
            }

            return new UserProfileResponseDto
            {
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
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
            user.UpdatedAt = DateTime.UtcNow;

            foreach (var childDto in dto.Children)
            {
                await UpdateChildInfoAsync(userId, childDto);
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<string> UploadChildAvatarAsync(string fileName, byte[] fileData, Guid studentId)
        {
            if (fileData == null || fileData.Length == 0)
                return null;

            var newFileName = $"student_{studentId}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(fileName)}";
            return await _fileStorageService.SaveFileAsync(fileName, fileData, "student-avatars", newFileName);
        }

        private async Task UpdateChildInfoAsync(Guid parentId, ChildProfileDto childDto)
        {
            var student = await _dbContext.Students
                .Include(s => s.StudentAllergens)
                .FirstOrDefaultAsync(s => s.StudentId == childDto.StudentId && s.ParentId == parentId);

            if (student != null)
            {
                if (!string.IsNullOrEmpty(childDto.AvatarFileName) && childDto.AvatarFileData != null)
                {
                    student.AvatarUrl = await UploadChildAvatarAsync(
                        childDto.AvatarFileName,
                        childDto.AvatarFileData,
                        childDto.StudentId);
                }

                student.UpdatedAt = DateTime.UtcNow;
                await UpdateChildAllergiesAsync(student, childDto.AllergyFoods);
            }
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
