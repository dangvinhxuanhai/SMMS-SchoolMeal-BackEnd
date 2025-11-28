using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.Wardens.DTOs;
using SMMS.Application.Features.Wardens.Interfaces;
using SMMS.Domain.Entities.school;
using SMMS.Persistence.Data;

namespace SMMS.Persistence.Repositories.Wardens
{
    public class WardensHealthRepository : IWardensHealthRepository
    {
        private readonly EduMealContext _context;

        public WardensHealthRepository(EduMealContext context)
        {
            _context = context;
        }

        #region Helper

        private static double? CalcBmi(double? heightCm, double? weightKg)
        {
            if (heightCm is null or <= 0 || weightKg is null or <= 0)
                return null;

            var bmi = weightKg.Value / Math.Pow(heightCm.Value / 100d, 2);
            return Math.Round(bmi, 2);
        }

        private static string? GetBmiCategory(double? bmi)
        {
            if (bmi is null) return null;

            return bmi.Value switch
            {
                < 18.5 => "Underweight",
                < 25 => "Normal",
                < 30 => "Overweight",
                _ => "Obese"
            };
        }

        private static StudentHealthDto MapToDto(StudentHealthRecord h)
        {
            double? height = h.HeightCm.HasValue ? (double?)h.HeightCm.Value : null;
            double? weight = h.WeightKg.HasValue ? (double?)h.WeightKg.Value : null;
            var bmi = CalcBmi(height, weight);

            return new StudentHealthDto
            {
                StudentId = h.StudentId,
                StudentName = h.Student?.FullName ?? string.Empty,
                HeightCm = height,
                WeightKg = weight,
                BMI = bmi,
                BMICategory = GetBmiCategory(bmi),
                RecordDate = h.RecordAt.ToDateTime(TimeOnly.MinValue),
                RecordId = h.RecordId
            };
        }

        #endregion

        #region Summary & danh sách theo lớp

        public async Task<HealthSummaryDto> GetHealthSummaryAsync(
            Guid wardenId,
            CancellationToken cancellationToken = default)
        {
            // Các lớp mà giáo viên phụ trách
            var classIds = await _context.Classes
                .Where(c => c.TeacherId == wardenId)
                .Select(c => c.ClassId)
                .ToListAsync(cancellationToken);

            // Học sinh trong các lớp đó
            var studentIds = await _context.StudentClasses
                .Where(sc => classIds.Contains(sc.ClassId))
                .Select(sc => sc.StudentId)
                .Distinct()
                .ToListAsync(cancellationToken);

            var healthRecords = await _context.StudentHealthRecords
                .Where(h => studentIds.Contains(h.StudentId))
                .ToListAsync(cancellationToken);

            var totalStudents = studentIds.Count;
            var normalWeight = 0;
            var underweight = 0;
            var overweight = 0;
            var obese = 0;
            double totalBmi = 0;
            var validCount = 0;

            foreach (var record in healthRecords)
            {
                var bmi = CalcBmi(
                    record.HeightCm.HasValue ? (double?)record.HeightCm.Value : null,
                    record.WeightKg.HasValue ? (double?)record.WeightKg.Value : null);

                if (bmi is null) continue;

                totalBmi += bmi.Value;
                validCount++;

                if (bmi < 18.5) underweight++;
                else if (bmi < 25) normalWeight++;
                else if (bmi < 30) overweight++;
                else obese++;
            }

            return new HealthSummaryDto
            {
                TotalStudents = totalStudents,
                NormalWeight = normalWeight,
                Underweight = underweight,
                Overweight = overweight,
                Obese = obese,
                AverageBMI = validCount > 0 ? Math.Round(totalBmi / validCount, 2) : 0
            };
        }

        public async Task<IEnumerable<StudentHealthDto>> GetStudentsHealthAsync(
            Guid classId,
            CancellationToken cancellationToken = default)
        {
            var query =
                from sc in _context.StudentClasses
                join s in _context.Students on sc.StudentId equals s.StudentId
                join h in _context.StudentHealthRecords on s.StudentId equals h.StudentId into healthJoin
                from health in healthJoin
                    .OrderByDescending(x => x.RecordAt)
                    .Take(1)
                    .DefaultIfEmpty()
                where sc.ClassId == classId
                select new { s, health };

            var list = await query.ToListAsync(cancellationToken);

            return list.Select(x =>
            {
                if (x.health == null)
                {
                    return new StudentHealthDto
                    {
                        StudentId = x.s.StudentId,
                        StudentName = x.s.FullName,
                        HeightCm = null,
                        WeightKg = null,
                        BMI = null,
                        BMICategory = null,
                        RecordDate = DateTime.MinValue,
                        RecordId = Guid.Empty
                    };
                }

                var dto = MapToDto(x.health);
                dto.StudentName = x.s.FullName;
                return dto;
            });
        }

        #endregion

        #region CRUD BMI theo student

        public async Task<IEnumerable<StudentHealthDto>> GetStudentBmiHistoryAsync(
            Guid studentId,
            CancellationToken cancellationToken = default)
        {
            var records = await _context.StudentHealthRecords
                .Include(h => h.Student)
                .Where(h => h.StudentId == studentId)
                .OrderByDescending(h => h.RecordAt)
                .ToListAsync(cancellationToken);

            return records.Select(MapToDto);
        }

        public async Task<StudentHealthDto> CreateStudentBmiAsync(
            Guid studentId,
            double heightCm,
            double weightKg,
            DateTime recordDate,
            CancellationToken cancellationToken = default)
        {
            // đảm bảo student tồn tại
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentId == studentId, cancellationToken);

            if (student == null)
                throw new ArgumentException("Student not found");

            var record = new StudentHealthRecord
            {
                RecordId = Guid.NewGuid(),
                StudentId = studentId,
                RecordAt = DateOnly.FromDateTime(recordDate),
                HeightCm = (decimal)heightCm,
                WeightKg = (decimal)weightKg,
                YearId = null
            };

            await _context.StudentHealthRecords.AddAsync(record, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            record.Student = student; // để map tên

            return MapToDto(record);
        }

        public async Task<StudentHealthDto?> UpdateStudentBmiAsync(
            Guid recordId,
            double heightCm,
            double weightKg,
            DateTime recordDate,
            CancellationToken cancellationToken = default)
        {
            var record = await _context.StudentHealthRecords
                .Include(h => h.Student)
                .FirstOrDefaultAsync(h => h.RecordId == recordId, cancellationToken);

            if (record == null)
                return null;

            record.HeightCm = (decimal)heightCm;
            record.WeightKg = (decimal)weightKg;
            record.RecordAt = DateOnly.FromDateTime(recordDate);

            await _context.SaveChangesAsync(cancellationToken);

            return MapToDto(record);
        }

        public async Task<bool> DeleteStudentBmiAsync(
            Guid recordId,
            CancellationToken cancellationToken = default)
        {
            var record = await _context.StudentHealthRecords
                .FirstOrDefaultAsync(h => h.RecordId == recordId, cancellationToken);

            if (record == null)
                return false;

            _context.StudentHealthRecords.Remove(record);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        #endregion
    }
}



