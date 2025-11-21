using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Entities.school;
using SMMS.Domain.Entities.nutrition;
using SMMS.Domain.Entities.billing;
using SMMS.Application.Features.Wardens.DTOs;
using SMMS.Application.Features.Wardens.Interfaces;
using SMMS.Persistence.DbContextSite;

namespace SMMS.Application.Features.Wardens.Services;

public class WardensService : IWardensService
{
    private readonly EduMealContext _context;

    public WardensService(EduMealContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ClassDto>> GetClassesAsync(Guid wardenId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var classes = await (
            from c in _context.Classes
            join t in _context.Teachers on c.TeacherId equals t.TeacherId
            join u in _context.Users on t.TeacherId equals u.UserId
            where c.TeacherId == wardenId && c.IsActive
            select new
            {
                c.ClassId,
                c.ClassName,
                SchoolName = c.School.SchoolName,
                WardenId = c.TeacherId,
                WardenName = u.FullName
            })
            .ToListAsync();

        var result = new List<ClassDto>();

        foreach (var cls in classes)
        {
            // Tổng số học sinh
            var totalStudents = await _context.StudentClasses
                .CountAsync(sc => sc.ClassId == cls.ClassId);

            // Số học sinh vắng hôm nay
            var absentCount = await (
                from sc in _context.StudentClasses
                join a in _context.Attendances on sc.StudentId equals a.StudentId
                where sc.ClassId == cls.ClassId && a.AbsentDate == today
                select a
            ).CountAsync();

            // Số học sinh có mặt
            var presentCount = totalStudents - absentCount;

            result.Add(new ClassDto
            {
                ClassId = cls.ClassId,
                ClassName = cls.ClassName,
                SchoolName = cls.SchoolName,
                WardenId = cls.WardenId ?? Guid.Empty,
                WardenName = cls.WardenName,
                TotalStudents = totalStudents,
                PresentToday = presentCount,
                AbsentToday = absentCount
            });
        }

        return result;
    }

    public async Task<ClassAttendanceDto> GetClassAttendanceAsync(Guid classId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var classInfo = await _context.Classes
            .Where(c => c.ClassId == classId)
            .Select(c => new { c.ClassId, c.ClassName })
            .FirstOrDefaultAsync();

        if (classInfo == null)
            throw new ArgumentException("Class not found");

        var students = await (
            from sc in _context.StudentClasses
            join s in _context.Students on sc.StudentId equals s.StudentId
            where sc.ClassId == classId
            select new StudentAttendanceDto
            {
                StudentId = s.StudentId,
                StudentName = s.FullName,
                Status = _context.Attendances
                    .Any(a => a.StudentId == s.StudentId && a.AbsentDate == today) ? "Absent" : "Present",
                Reason = _context.Attendances
                    .Where(a => a.StudentId == s.StudentId && a.AbsentDate == today)
                    .Select(a => a.Reason)
                    .FirstOrDefault(),
                CreatedAt = _context.Attendances
                    .Where(a => a.StudentId == s.StudentId && a.AbsentDate == today)
                    .Select(a => a.CreatedAt)
                    .FirstOrDefault()
            })
            .ToListAsync();

        var summary = new AttendanceSummaryDto
        {
            TotalStudents = students.Count,
            Present = students.Count(s => s.Status == "Present"),
            Absent = students.Count(s => s.Status == "Absent"),
            Late = 0, // Không có thông tin về Late trong model hiện tại
            AttendanceRate = students.Count > 0 ?
                Math.Round((double)students.Count(s => s.Status == "Present") / students.Count * 100, 2) : 0
        };

        return new ClassAttendanceDto
        {
            ClassId = classInfo.ClassId,
            ClassName = classInfo.ClassName,
            Students = students,
            Summary = summary
        };
    }

    public async Task<byte[]> ExportAttendanceReportAsync(Guid classId)
    {
        var attendanceData = await GetClassAttendanceAsync(classId);

        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Attendance Report");

        // Headers
        worksheet.Cell(1, 1).Value = "Student Name";
        worksheet.Cell(1, 2).Value = "Status";
        worksheet.Cell(1, 3).Value = "Reason";
        worksheet.Cell(1, 4).Value = "Time";

        // Data
        for (int i = 0; i < attendanceData.Students.Count; i++)
        {
            var student = attendanceData.Students[i];
            worksheet.Cell(i + 2, 1).Value = student.StudentName;
            worksheet.Cell(i + 2, 2).Value = student.Status;
            worksheet.Cell(i + 2, 3).Value = student.Reason ?? "";
            worksheet.Cell(i + 2, 4).Value = student.CreatedAt.ToString("HH:mm");
        }

        // Summary
        var summaryRow = attendanceData.Students.Count + 3;
        worksheet.Cell(summaryRow, 1).Value = "Total Students:";
        worksheet.Cell(summaryRow, 2).Value = attendanceData.Summary.TotalStudents;
        worksheet.Cell(summaryRow + 1, 1).Value = "Present:";
        worksheet.Cell(summaryRow + 1, 2).Value = attendanceData.Summary.Present;
        worksheet.Cell(summaryRow + 2, 1).Value = "Absent:";
        worksheet.Cell(summaryRow + 2, 2).Value = attendanceData.Summary.Absent;
        worksheet.Cell(summaryRow + 3, 1).Value = "Attendance Rate:";
        worksheet.Cell(summaryRow + 3, 2).Value = $"{attendanceData.Summary.AttendanceRate}%";

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<IEnumerable<StudentDto>> GetStudentsInClassAsync(Guid classId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var students = await (from sc in _context.StudentClasses
                              join s in _context.Students on sc.StudentId equals s.StudentId
                              join p in _context.Users on s.ParentId equals p.UserId into parentJoin
                              from parent in parentJoin.DefaultIfEmpty()
                              where sc.ClassId == classId
                              select new
                              {
                                  s.StudentId,
                                  s.FullName,
                                  s.Gender,
                                  s.DateOfBirth,
                                  ParentName = parent.FullName,
                                  ParentPhone = parent.Phone,
                                  s.IsActive,
                                  Allergies = (from sa in _context.StudentAllergens
                                               join al in _context.Allergens on sa.AllergenId equals al.AllergenId
                                               where sa.StudentId == s.StudentId
                                               select al.AllergenName).ToList(),
                                  IsAbsent = _context.Attendances
                                      .Any(a => a.StudentId == s.StudentId && a.AbsentDate == today)
                              }).ToListAsync();

        return students.Select(s => new StudentDto
        {
            StudentId = s.StudentId,
            FullName = s.FullName,
            Gender = s.Gender,
            DateOfBirth = s.DateOfBirth,
            IsActive = s.IsActive,
            ParentName = s.ParentName,
            ParentPhone = s.ParentPhone,
            Allergies = s.Allergies,
            IsAbsent = s.IsAbsent
        });
    }

    public async Task<HealthSummaryDto> GetHealthSummaryAsync(Guid wardenId)
    {
        var classes = await _context.Classes
            .Where(c => c.TeacherId == wardenId)
            .Select(c => c.ClassId)
            .ToListAsync();

        var studentIds = await _context.StudentClasses
            .Where(sc => classes.Contains(sc.ClassId))
            .Select(sc => sc.StudentId)
            .ToListAsync();

        var healthRecords = await _context.StudentHealthRecords
            .Where(h => studentIds.Contains(h.StudentId))
            .ToListAsync();

        var totalStudents = studentIds.Count;
        var normalWeight = 0;
        var underweight = 0;
        var overweight = 0;
        var obese = 0;
        var totalBMI = 0.0;
        var validBMICount = 0;

        foreach (var record in healthRecords)
        {
            if (record.HeightCm.HasValue && record.WeightKg.HasValue)
            {
                var bmi = (double)record.WeightKg.Value / Math.Pow((double)record.HeightCm.Value / 100, 2);
                totalBMI += bmi;
                validBMICount++;

                if (bmi < 18.5) underweight++;
                else if (bmi < 25) normalWeight++;
                else if (bmi < 30) overweight++;
                else obese++;
            }
        }

        return new HealthSummaryDto
        {
            TotalStudents = totalStudents,
            NormalWeight = normalWeight,
            Underweight = underweight,
            Overweight = overweight,
            Obese = obese,
            AverageBMI = validBMICount > 0 ? Math.Round(totalBMI / validBMICount, 2) : 0
        };
    }

    public async Task<IEnumerable<StudentHealthDto>> GetStudentsHealthAsync(Guid classId)
    {
        return await (
            from sc in _context.StudentClasses
            join s in _context.Students on sc.StudentId equals s.StudentId
            join h in _context.StudentHealthRecords on s.StudentId equals h.StudentId into healthJoin
            from health in healthJoin.DefaultIfEmpty()
            where sc.ClassId == classId
            select new StudentHealthDto
            {
                StudentId = s.StudentId,
                StudentName = s.FullName,
                HeightCm = health != null ? (double?)health.HeightCm : null,
                WeightKg = health != null ? (double?)health.WeightKg : null,
                BMI = health != null && health.HeightCm.HasValue && health.WeightKg.HasValue ?
                    Math.Round((double)health.WeightKg.Value / Math.Pow((double)health.HeightCm.Value / 100, 2), 2) : null,
                BMICategory = health != null && health.HeightCm.HasValue && health.WeightKg.HasValue ?
                    GetBMICategory((double)health.WeightKg.Value / Math.Pow((double)health.HeightCm.Value / 100, 2)) : null,
                RecordDate = health != null ? health.RecordAt.ToDateTime(TimeOnly.MinValue) : DateTime.MinValue
            })
            .ToListAsync();
    }

    public async Task<DashboardDto> GetDashboardAsync(Guid wardenId)
    {
        var classes = await _context.Classes
            .Where(c => c.TeacherId == wardenId)
            .Select(c => c.ClassId)
            .ToListAsync();

        var totalStudents = await _context.StudentClasses
            .CountAsync(sc => classes.Contains(sc.ClassId));

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var absentToday = await _context.Attendances
            .CountAsync(a => a.Student.StudentClasses.Any(sc => classes.Contains(sc.ClassId)) &&
                           a.AbsentDate == today);

        var presentToday = totalStudents - absentToday;
        var attendanceRate = totalStudents > 0 ? Math.Round((double)presentToday / totalStudents * 100, 2) : 0;

        var recentActivities = new List<RecentActivityDto>
        {
            new() { Activity = "Attendance marked", Timestamp = DateTime.UtcNow.AddHours(-1), Type = "Attendance" },
            new() { Activity = "Health record updated", Timestamp = DateTime.UtcNow.AddHours(-2), Type = "Health" },
            new() { Activity = "Student enrolled", Timestamp = DateTime.UtcNow.AddHours(-3), Type = "Enrollment" }
        };

        return new DashboardDto
        {
            TotalClasses = classes.Count,
            TotalStudents = totalStudents,
            PresentToday = presentToday,
            AbsentToday = absentToday,
            AttendanceRate = attendanceRate,
            RecentActivities = recentActivities
        };
    }

    public async Task<IEnumerable<NotificationDto>> GetNotificationsAsync(Guid wardenId)
    {
        return await (
            from nr in _context.NotificationRecipients
            join n in _context.Notifications on nr.NotificationId equals n.NotificationId
            where nr.UserId == wardenId
            orderby n.CreatedAt descending
            select new NotificationDto
            {
                NotificationId = n.NotificationId,
                Title = n.Title,
                Content = n.Content,
                CreatedAt = n.CreatedAt,
                IsRead = nr.IsRead,
                SendType = n.SendType
            })
            .Take(10)
            .ToListAsync();
    }

    public async Task<byte[]> ExportClassStudentsAsync(Guid classId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // 1️⃣ Lấy danh sách học sinh
        var students = await (from sc in _context.StudentClasses
                              join s in _context.Students on sc.StudentId equals s.StudentId
                              join p in _context.Users on s.ParentId equals p.UserId into parentJoin
                              from parent in parentJoin.DefaultIfEmpty()
                              where sc.ClassId == classId
                              select new
                              {
                                  s.StudentId,
                                  s.FullName,
                                  s.Gender,
                                  s.DateOfBirth,
                                  ParentName = parent.FullName,
                                  ParentPhone = parent.Phone,
                                  Allergies = (from sa in _context.StudentAllergens
                                               join al in _context.Allergens on sa.AllergenId equals al.AllergenId
                                               where sa.StudentId == s.StudentId
                                               select al.AllergenName).ToList(),
                                  IsAbsent = _context.Attendances
                                      .Any(a => a.StudentId == s.StudentId && a.AbsentDate == today),
                                  AbsentReason = _context.Attendances
                                      .Where(a => a.StudentId == s.StudentId && a.AbsentDate == today)
                                      .Select(a => a.Reason)
                                      .FirstOrDefault()
                              }).ToListAsync();

        // 2️⃣ Tạo workbook Excel
        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Danh sách học sinh");

        // 3️⃣ Header
        worksheet.Cell(1, 1).Value = "STT";
        worksheet.Cell(1, 2).Value = "Họ tên học sinh";
        worksheet.Cell(1, 3).Value = "Giới tính";
        worksheet.Cell(1, 4).Value = "Ngày sinh";
        worksheet.Cell(1, 5).Value = "Phụ huynh";
        worksheet.Cell(1, 6).Value = "Số điện thoại";
        worksheet.Cell(1, 7).Value = "Dị ứng";

        var headerRange = worksheet.Range("A1:G1");
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;

        // 4️⃣ Ghi dữ liệu
        int row = 2;
        int index = 1;

        foreach (var s in students)
        {
            worksheet.Cell(row, 1).Value = index++;
            worksheet.Cell(row, 2).Value = s.FullName;
            worksheet.Cell(row, 3).Value = s.Gender == "M" ? "Nam" : "Nữ";
            worksheet.Cell(row, 4).Value = s.DateOfBirth?.ToString("dd/MM/yyyy");
            worksheet.Cell(row, 5).Value = s.ParentName;
            worksheet.Cell(row, 6).Value = s.ParentPhone;
            worksheet.Cell(row, 7).Value = s.Allergies.Any()
                ? string.Join(", ", s.Allergies)
                : s.AbsentReason ?? "-";

            row++;
        }

        worksheet.Columns().AdjustToContents();

        // 5️⃣ Trả về file
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return stream.ToArray();
    }

    public async Task<byte[]> ExportClassHealthAsync(Guid classId)
    {
        // 1️⃣ Lấy dữ liệu mới nhất theo học sinh
        var healthData = await (
            from sc in _context.StudentClasses
            join s in _context.Students on sc.StudentId equals s.StudentId
            join h in _context.StudentHealthRecords on s.StudentId equals h.StudentId into healthJoin
            from health in healthJoin
                .OrderByDescending(x => x.RecordAt)
                .Take(1)
                .DefaultIfEmpty()
            where sc.ClassId == classId
            select new
            {
                s.FullName,
                HeightCm = health.HeightCm,
                WeightKg = health.WeightKg,
                health.RecordAt
            })
            .ToListAsync();

        // 2️⃣ Xử lý dữ liệu BMI & trạng thái
        var records = healthData.Select(x =>
        {
            double bmi = 0;
            string status = "Chưa có dữ liệu";
            if (x.HeightCm != null && x.WeightKg != null && x.HeightCm > 0)
            {
                bmi = Math.Round(
                    Convert.ToDouble(x.WeightKg.Value) /
                    Math.Pow(Convert.ToDouble(x.HeightCm.Value) / 100d, 2),
                    1);

                status = bmi switch
                {
                    <= 14 => "Thiếu cân",
                    <= 17 => "Bình thường",
                    _ => "Thừa cân / Béo phì"
                };
            }

            return new
            {
                x.FullName,
                Height = x.HeightCm != null ? $"{x.HeightCm} cm" : "-",
                Weight = x.WeightKg != null ? $"{x.WeightKg} kg" : "-",
                Bmi = bmi > 0 ? bmi.ToString("0.0") : "-",
                Status = status,
                RecordDate = x.RecordAt.ToString("dd/MM/yyyy")
            };
        }).ToList();

        // 3️⃣ Tạo workbook Excel
        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var ws = workbook.Worksheets.Add("Chỉ số BMI học sinh");

        // Header
        ws.Cell(1, 1).Value = "STT";
        ws.Cell(1, 2).Value = "Học sinh";
        ws.Cell(1, 3).Value = "Chiều cao";
        ws.Cell(1, 4).Value = "Cân nặng";
        ws.Cell(1, 5).Value = "BMI";
        ws.Cell(1, 6).Value = "Trạng thái";
        ws.Cell(1, 7).Value = "Ngày đo";

        var headerRange = ws.Range("A1:G1");
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;
        headerRange.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

        // 4️⃣ Ghi dữ liệu
        int row = 2;
        int index = 1;

        foreach (var item in records)
        {
            ws.Cell(row, 1).Value = index++;
            ws.Cell(row, 2).Value = item.FullName;
            ws.Cell(row, 3).Value = item.Height;
            ws.Cell(row, 4).Value = item.Weight;
            ws.Cell(row, 5).Value = item.Bmi;
            ws.Cell(row, 6).Value = item.Status;
            ws.Cell(row, 7).Value = item.RecordDate;

            // Màu nền theo trạng thái
            var statusCell = ws.Cell(row, 6);
            switch (item.Status)
            {
                case "Thiếu cân":
                    statusCell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightYellow;
                    break;
                case "Bình thường":
                    statusCell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGreen;
                    break;
                case "Thừa cân / Béo phì":
                    statusCell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightPink;
                    break;
            }

            row++;
        }

        ws.Columns().AdjustToContents();

        // 5️⃣ Xuất file Excel
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return stream.ToArray();
    }

    private static string GetBMICategory(double bmi)
    {
        return bmi switch
        {
            < 18.5 => "Underweight",
            < 25 => "Normal",
            < 30 => "Overweight",
            _ => "Obese"
        };
    }

    public async Task<object> GetHealthRecordsAsync(Guid classId)
    {
        // 1️⃣ Lấy dữ liệu BMI trước
        var temp = await (
            from sc in _context.StudentClasses
            join s in _context.Students on sc.StudentId equals s.StudentId
            join h in _context.StudentHealthRecords on s.StudentId equals h.StudentId into healthJoin
            from health in healthJoin
                .OrderByDescending(x => x.RecordAt)
                .Take(1)
                .DefaultIfEmpty()
            where sc.ClassId == classId
            select new
            {
                s.StudentId,
                s.FullName,
                s.Gender,
                s.DateOfBirth,
                HeightCm = health.HeightCm,
                WeightKg = health.WeightKg
            })
            .ToListAsync();

        // 2️⃣ Xử lý tính toán & phân loại sau khi EF đã load xong
        var latestRecords = temp.Select(x =>
        {
            double bmi = 0;
            string status = "Chưa có dữ liệu";

            if (x.HeightCm != null && x.WeightKg != null && x.HeightCm > 0)
            {
                bmi = Math.Round(
                    Convert.ToDouble(x.WeightKg.Value) /
                    Math.Pow(Convert.ToDouble(x.HeightCm.Value) / 100d, 2),
                    1);

                status = bmi switch
                {
                    <= 14 => "Thiếu cân",
                    <= 17 => "Bình thường",
                    _ => "Thừa cân / Béo phì"
                };
            }

            return new
            {
                x.StudentId,
                x.FullName,
                x.Gender,
                x.DateOfBirth,
                x.HeightCm,
                x.WeightKg,
                BMI = bmi,
                Status = status
            };
        }).ToList();

        return latestRecords;
    }
    public async Task<object> SearchAsync(Guid classId, string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return new { Students = new List<object>() };

        keyword = keyword.Trim().ToLower();

        // 🔍 Tìm học sinh hoặc phụ huynh theo tên trong lớp cụ thể
        var students = await (
            from sc in _context.StudentClasses
            join s in _context.Students on sc.StudentId equals s.StudentId
            join p in _context.Users on s.ParentId equals p.UserId into parentJoin
            from parent in parentJoin.DefaultIfEmpty()
            where sc.ClassId == classId &&
                  (
                      s.FullName.ToLower().Contains(keyword) ||
                      (parent.FullName != null && parent.FullName.ToLower().Contains(keyword))
                  )
            select new
            {
                s.StudentId,
                s.FullName,
                s.Gender,
                s.DateOfBirth,
                ParentName = parent.FullName,
                ClassName = sc.Class.ClassName,
                SchoolName = sc.Class.School.SchoolName
            }).ToListAsync();

        return new { Students = students };
    }



}
