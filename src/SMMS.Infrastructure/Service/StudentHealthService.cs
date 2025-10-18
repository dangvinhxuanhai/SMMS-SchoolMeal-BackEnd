using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.school.DTOs;
using SMMS.Application.Features.school.Interfaces;
using SMMS.Persistence.DbContextSite;

namespace SMMS.Infrastructure.Service
{
    public class StudentHealthService : IStudentHealthService
    {
        private readonly EduMealContext _dbContext;

        public StudentHealthService(EduMealContext dbContext)
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
    }
}
