using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.school.DTOs;
using SMMS.Application.Features.school.Interfaces;
using SMMS.Persistence.Data;

namespace SMMS.Persistence.Repositories.Schools
{
    public class StudentHealthRepository : IStudentHealthRepository
    {
        private readonly EduMealContext _dbContext;

        public StudentHealthRepository(EduMealContext dbContext)
        {
            _dbContext = dbContext;
        }

        // ✅ Lấy BMI hiện tại (năm học mới nhất)
        public async Task<StudentBMIResultDto?> GetCurrentBMIAsync(Guid studentId, Guid parentId)
        {
            var student = await _dbContext.Students
                .Include(s => s.Parent)
                .FirstOrDefaultAsync(s => s.StudentId == studentId && s.ParentId == parentId);

            if (student == null)
                throw new UnauthorizedAccessException("Bạn không có quyền xem thông tin học sinh này.");

            var record = await _dbContext.StudentHealthRecords
                .Include(r => r.Year)
                .Where(r => r.StudentId == studentId)
                .OrderByDescending(r => r.Year.YearName)
                .ThenByDescending(r => r.RecordAt)
                .FirstOrDefaultAsync();

            if (record == null)
                return null;

            var bmi = CalculateBMI(record.HeightCm ?? 0, record.WeightKg ?? 0);

            return new StudentBMIResultDto
            {
                StudentId = student.StudentId,
                StudentName = student.FullName,
                AcademicYear = record.Year?.YearName ?? "Chưa có năm học",
                HeightCm = record.HeightCm ?? 0,
                WeightKg = record.WeightKg ?? 0,
                BMI = bmi,
                BMIStatus = GetBMIStatus(bmi),
                RecordAt = record.RecordAt.ToDateTime(TimeOnly.MinValue)
            };
        }

        // ✅ Lấy BMI theo các năm học
        public async Task<IEnumerable<StudentBMIResultDto>> GetBMIByYearsAsync(Guid studentId, Guid parentId, string? yearFilter = null)
        {
            var student = await _dbContext.Students
                .Include(s => s.Parent)
                .FirstOrDefaultAsync(s => s.StudentId == studentId && s.ParentId == parentId);

            if (student == null)
                throw new UnauthorizedAccessException("Bạn không có quyền xem thông tin học sinh này.");

            var query = _dbContext.StudentHealthRecords
                .Include(r => r.Year)
                .Where(r => r.StudentId == studentId);

            if (!string.IsNullOrEmpty(yearFilter))
                query = query.Where(r => r.Year.YearName == yearFilter);

            var records = await query
                .OrderByDescending(r => r.Year.YearName)
                .ThenByDescending(r => r.RecordAt)
                .ToListAsync();

            return records.Select(r =>
            {
                var bmi = CalculateBMI(r.HeightCm ?? 0, r.WeightKg ?? 0);
                return new StudentBMIResultDto
                {
                    StudentId = r.StudentId,
                    StudentName = student.FullName,
                    AcademicYear = r.Year?.YearName ?? "Chưa có năm học",
                    HeightCm = r.HeightCm ?? 0,
                    WeightKg = r.WeightKg ?? 0,
                    BMI = bmi,
                    BMIStatus = GetBMIStatus(bmi),
                    RecordAt = r.RecordAt.ToDateTime(TimeOnly.MinValue)

                };
            });
        }



        // ✅ Hàm tính BMI
        private static decimal CalculateBMI(decimal heightCm, decimal weightKg)
        {
            if (heightCm <= 0) return 0;
            var heightM = heightCm / 100;
            return Math.Round(weightKg / (heightM * heightM), 1);
        }

        // ✅ Phân loại BMI
        private static string GetBMIStatus(decimal bmi)
        {
            if (bmi == 0) return "Không có dữ liệu";
            if (bmi < 18.5m) return "Thiếu cân";
            if (bmi < 25m) return "Bình thường";
            if (bmi < 30m) return "Thừa cân";
            return "Béo phì";
        }

        public async Task<double?> GetAverageBmiForFirstClassAsync(
            Guid schoolId,
            CancellationToken cancellationToken = default)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            // 1. Lấy class đầu tiên của school
            var firstClass = await _dbContext.Classes
                .Where(c => c.SchoolId == schoolId && c.IsActive)
                .OrderBy(c => c.ClassName)     // hoặc CreatedAt nếu anh có
                .FirstOrDefaultAsync(cancellationToken);

            if (firstClass == null)
                return null;

            // 2. Lấy các StudentId còn đang học lớp đó
            var studentIds = await _dbContext.StudentClasses
                .Where(sc => sc.ClassId == firstClass.ClassId &&
                             (sc.LeftDate == null || sc.LeftDate > today))
                .Select(sc => sc.StudentId)
                .ToListAsync(cancellationToken);

            if (!studentIds.Any())
                return null;

            // 3. Lấy health record mới nhất cho từng học sinh
            var latestRecords = await _dbContext.StudentHealthRecords
                .Where(r => studentIds.Contains(r.StudentId) &&
                            r.HeightCm.HasValue &&
                            r.WeightKg.HasValue)
                .GroupBy(r => r.StudentId)
                .Select(g => g
                    .OrderByDescending(x => x.RecordAt)
                    .FirstOrDefault()!)
                .ToListAsync(cancellationToken);

            if (!latestRecords.Any())
                return null;

            // 4. Tính BMI từng học sinh
            var bmiList = latestRecords
                .Select(r =>
                {
                    var h = (double)r.HeightCm!.Value / 100.0; // m
                    var w = (double)r.WeightKg!.Value;         // kg
                    if (h <= 0 || w <= 0) return (double?)null;
                    return w / (h * h);
                })
                .Where(b => b.HasValue)
                .Select(b => b!.Value)
                .ToList();

            if (!bmiList.Any())
                return null;

            return bmiList.Average();
        }
    }
}
