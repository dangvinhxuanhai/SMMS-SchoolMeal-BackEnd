using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using MediatR;
using SMMS.Application.Features.Wardens.DTOs;
using SMMS.Application.Features.Wardens.Interfaces;
using SMMS.Application.Features.Wardens.Queries;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Application.Features.Wardens.Handlers;
public class WardensHandler :
    IRequestHandler<GetWardenClassesQuery, IEnumerable<ClassDto>>,
    IRequestHandler<GetClassAttendanceQuery, ClassAttendanceDto>,
    IRequestHandler<ExportAttendanceReportQuery, byte[]>,
    IRequestHandler<GetStudentsInClassQuery, IEnumerable<StudentDto>>,

    IRequestHandler<GetWardenDashboardQuery, DashboardDto>,
    IRequestHandler<GetWardenNotificationsQuery, IEnumerable<NotificationDto>>,
    IRequestHandler<ExportClassStudentsQuery, byte[]>,
    IRequestHandler<ExportClassHealthQuery, byte[]>,
    IRequestHandler<GetHealthRecordsQuery, object>,
    IRequestHandler<SearchStudentsInClassQuery, object>

{
    private readonly IWardensRepository _repo;

    public WardensHandler(IWardensRepository repo)
    {
        _repo = repo;
    }

    #region 1️⃣ GetClassesAsync

    public async Task<IEnumerable<ClassDto>> Handle(
        GetWardenClassesQuery request,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var classes = await (
            from c in _repo.Classes
            join t in _repo.Teachers on c.TeacherId equals t.TeacherId
            join u in _repo.Users on t.TeacherId equals u.UserId
            where c.TeacherId == request.WardenId && c.IsActive
            select new
            {
                c.ClassId,
                c.ClassName,
                SchoolName = c.School.SchoolName,
                WardenId = c.TeacherId,
                WardenName = u.FullName
            })
            .ToListAsync(cancellationToken);

        var result = new List<ClassDto>();

        foreach (var cls in classes)
        {
            var totalStudents = await _repo.StudentClasses
                .CountAsync(sc => sc.ClassId == cls.ClassId, cancellationToken);

            var absentCount = await (
                from sc in _repo.StudentClasses
                join a in _repo.Attendances on sc.StudentId equals a.StudentId
                where sc.ClassId == cls.ClassId && a.AbsentDate == today
                select a
            ).CountAsync(cancellationToken);

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
                AbsentToday = absentCount,
                AttendanceRate = totalStudents > 0
                    ? Math.Round((double)presentCount / totalStudents * 100, 2)
                    : 0
            });
        }

        return result;
    }

    #endregion

    #region 2️⃣ GetClassAttendanceAsync

    public async Task<ClassAttendanceDto> Handle(
        GetClassAttendanceQuery request,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var classInfo = await _repo.Classes
            .Where(c => c.ClassId == request.ClassId)
            .Select(c => new { c.ClassId, c.ClassName })
            .FirstOrDefaultAsync(cancellationToken);

        if (classInfo == null)
            throw new ArgumentException("Class not found");

        var students = await (
            from sc in _repo.StudentClasses
            join s in _repo.Students on sc.StudentId equals s.StudentId
            where sc.ClassId == request.ClassId
            select new StudentAttendanceDto
            {
                StudentId = s.StudentId,
                StudentName = s.FullName,
                Status = _repo.Attendances
                    .Any(a => a.StudentId == s.StudentId && a.AbsentDate == today)
                    ? "Absent"
                    : "Present",
                Reason = _repo.Attendances
                    .Where(a => a.StudentId == s.StudentId && a.AbsentDate == today)
                    .Select(a => a.Reason)
                    .FirstOrDefault(),
                CreatedAt = _repo.Attendances
                    .Where(a => a.StudentId == s.StudentId && a.AbsentDate == today)
                    .Select(a => a.CreatedAt)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        var summary = new AttendanceSummaryDto
        {
            TotalStudents = students.Count,
            Present = students.Count(s => s.Status == "Present"),
            Absent = students.Count(s => s.Status == "Absent"),
            Late = 0,
            AttendanceRate = students.Count > 0
                ? Math.Round((double)students.Count(s => s.Status == "Present") / students.Count * 100, 2)
                : 0
        };

        return new ClassAttendanceDto
        {
            ClassId = classInfo.ClassId,
            ClassName = classInfo.ClassName,
            Students = students,
            Summary = summary
        };
    }

    #endregion

    #region 3️⃣ ExportAttendanceReportAsync

    public async Task<byte[]> Handle(
        ExportAttendanceReportQuery request,
        CancellationToken cancellationToken)
    {
        var attendanceData = await Handle(
            new GetClassAttendanceQuery(request.ClassId),
            cancellationToken);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Attendance Report");

        worksheet.Cell(1, 1).Value = "Student Name";
        worksheet.Cell(1, 2).Value = "Status";
        worksheet.Cell(1, 3).Value = "Reason";
        worksheet.Cell(1, 4).Value = "Time";

        for (int i = 0; i < attendanceData.Students.Count; i++)
        {
            var student = attendanceData.Students[i];
            worksheet.Cell(i + 2, 1).Value = student.StudentName;
            worksheet.Cell(i + 2, 2).Value = student.Status;
            worksheet.Cell(i + 2, 3).Value = student.Reason ?? "";
            worksheet.Cell(i + 2, 4).Value = student.CreatedAt.ToString("HH:mm");
        }

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

    #endregion

    #region 4️⃣ GetStudentsInClassAsync

    public async Task<IEnumerable<StudentDto>> Handle(
        GetStudentsInClassQuery request,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var students = await (
            from sc in _repo.StudentClasses
            join s in _repo.Students on sc.StudentId equals s.StudentId
            join p in _repo.Users on s.ParentId equals p.UserId into parentJoin
            from parent in parentJoin.DefaultIfEmpty()
            where sc.ClassId == request.ClassId
            select new
            {
                s.StudentId,
                s.FullName,
                s.Gender,
                s.DateOfBirth,
                s.RelationName,
                ParentName = parent.FullName,
                ParentPhone = parent.Phone,
                s.IsActive,
                Allergies = (from sa in _repo.StudentAllergens
                             join al in _repo.Allergens on sa.AllergenId equals al.AllergenId
                             where sa.StudentId == s.StudentId
                             select al.AllergenName).ToList(),
                IsAbsent = _repo.Attendances
                    .Any(a => a.StudentId == s.StudentId && a.AbsentDate == today)
            }).ToListAsync(cancellationToken);

        return students.Select(s => new StudentDto
        {
            StudentId = s.StudentId,
            FullName = s.FullName,
            Gender = s.Gender,
            DateOfBirth = s.DateOfBirth,
            IsActive = s.IsActive,
            RelationName = s.RelationName,
            ParentName = s.ParentName,
            ParentPhone = s.ParentPhone,
            Allergies = s.Allergies,
            IsAbsent = s.IsAbsent
        });
    }

    #endregion

    #region 5️⃣ GetHealthSummaryAsync

    public async Task<HealthSummaryDto> Handle(
        GetHealthSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var classes = await _repo.Classes
            .Where(c => c.TeacherId == request.WardenId)
            .Select(c => c.ClassId)
            .ToListAsync(cancellationToken);

        var studentIds = await _repo.StudentClasses
            .Where(sc => classes.Contains(sc.ClassId))
            .Select(sc => sc.StudentId)
            .ToListAsync(cancellationToken);

        var healthRecords = await _repo.StudentHealthRecords
            .Where(h => studentIds.Contains(h.StudentId))
            .ToListAsync(cancellationToken);

        var totalStudents = studentIds.Count;
        var normalWeight = 0;
        var underweight = 0;
        var overweight = 0;
        var obese = 0;
        double totalBMI = 0.0;
        var validBMICount = 0;

        foreach (var record in healthRecords)
        {
            if (record.HeightCm.HasValue && record.WeightKg.HasValue && record.HeightCm > 0)
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

    #endregion

    #region 6️⃣ GetStudentsHealthAsync

    public async Task<IEnumerable<StudentHealthDto>> Handle(
        GetStudentsHealthQuery request,
        CancellationToken cancellationToken)
    {
        return await (
            from sc in _repo.StudentClasses
            join s in _repo.Students on sc.StudentId equals s.StudentId
            join h in _repo.StudentHealthRecords on s.StudentId equals h.StudentId into healthJoin
            from health in healthJoin.DefaultIfEmpty()
            where sc.ClassId == request.ClassId
            select new StudentHealthDto
            {
                RecordId = health != null ? (Guid?)health.RecordId : null,
                StudentId = s.StudentId,
                StudentName = s.FullName,
                HeightCm = health != null ? (double?)health.HeightCm : null,
                WeightKg = health != null ? (double?)health.WeightKg : null,
                BMI = health != null && health.HeightCm.HasValue && health.WeightKg.HasValue
                    ? Math.Round(
                        (double)health.WeightKg.Value /
                        Math.Pow((double)health.HeightCm.Value / 100, 2), 2)
                    : null,
                BMICategory = health != null && health.HeightCm.HasValue && health.WeightKg.HasValue
                    ? GetBMICategory(
                        (double)health.WeightKg.Value /
                        Math.Pow((double)health.HeightCm.Value / 100, 2))
                    : null,
                RecordDate = health != null
                    ? health.RecordAt.ToDateTime(TimeOnly.MinValue)
                    : DateTime.MinValue
            })
            .ToListAsync(cancellationToken);
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

    #endregion

    #region 7️⃣ GetDashboardAsync

    public async Task<DashboardDto> Handle(
        GetWardenDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var classes = await _repo.Classes
            .Where(c => c.TeacherId == request.WardenId)
            .Select(c => c.ClassId)
            .ToListAsync(cancellationToken);

        var totalStudents = await _repo.StudentClasses
            .CountAsync(sc => classes.Contains(sc.ClassId), cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var absentToday = await _repo.Attendances
            .CountAsync(a => a.Student.StudentClasses.Any(sc => classes.Contains(sc.ClassId)) &&
                             a.AbsentDate == today,
                        cancellationToken);

        var presentToday = totalStudents - absentToday;
        var attendanceRate = totalStudents > 0
            ? Math.Round((double)presentToday / totalStudents * 100, 2)
            : 0;

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

    #endregion

    #region 8️⃣ GetNotificationsAsync

    public async Task<IEnumerable<NotificationDto>> Handle(
         GetWardenNotificationsQuery request,
         CancellationToken cancellationToken)
    {
        var query =
            from nr in _repo.NotificationRecipients      // phải là IQueryable<NotificationRecipient>
            join n in _repo.Notifications                // IQueryable<Notification>
                on nr.NotificationId equals n.NotificationId
            where nr.UserId == request.WardenId
            orderby n.CreatedAt descending
            select new NotificationDto
            {
                NotificationId = n.NotificationId,
                Title = n.Title,
                Content = n.Content,
                CreatedAt = n.CreatedAt,
                IsRead = nr.IsRead,
                SendType = n.SendType
            };

        return await query
            .Take(10)
            .ToListAsync(cancellationToken);
    }

    #endregion

    #region 9️⃣ ExportClassStudentsAsync

    public async Task<byte[]> Handle(
        ExportClassStudentsQuery request,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var students = await (
            from sc in _repo.StudentClasses
            join s in _repo.Students on sc.StudentId equals s.StudentId
            join p in _repo.Users on s.ParentId equals p.UserId into parentJoin
            from parent in parentJoin.DefaultIfEmpty()
            where sc.ClassId == request.ClassId
            select new
            {
                s.StudentId,
                s.FullName,
                s.Gender,
                s.DateOfBirth,
                ParentName = parent.FullName,
                ParentPhone = parent.Phone,
                Allergies = (from sa in _repo.StudentAllergens
                             join al in _repo.Allergens on sa.AllergenId equals al.AllergenId
                             where sa.StudentId == s.StudentId
                             select al.AllergenName).ToList(),
                IsAbsent = _repo.Attendances
                    .Any(a => a.StudentId == s.StudentId && a.AbsentDate == today),
                AbsentReason = _repo.Attendances
                    .Where(a => a.StudentId == s.StudentId && a.AbsentDate == today)
                    .Select(a => a.Reason)
                    .FirstOrDefault()
            }).ToListAsync(cancellationToken);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Danh sách học sinh");

        worksheet.Cell(1, 1).Value = "STT";
        worksheet.Cell(1, 2).Value = "Họ tên học sinh";
        worksheet.Cell(1, 3).Value = "Giới tính";
        worksheet.Cell(1, 4).Value = "Ngày sinh";
        worksheet.Cell(1, 5).Value = "Phụ huynh";
        worksheet.Cell(1, 6).Value = "Số điện thoại";
        worksheet.Cell(1, 7).Value = "Dị ứng";

        var headerRange = worksheet.Range("A1:G1");
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

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

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return stream.ToArray();
    }

    #endregion

    #region 🔟 ExportClassHealthAsync

    public async Task<byte[]> Handle(
        ExportClassHealthQuery request,
        CancellationToken cancellationToken)
    {
        var healthData = await (
            from sc in _repo.StudentClasses
            join s in _repo.Students on sc.StudentId equals s.StudentId
            join h in _repo.StudentHealthRecords on s.StudentId equals h.StudentId into healthJoin
            from health in healthJoin
                .OrderByDescending(x => x.RecordAt)
                .Take(1)
                .DefaultIfEmpty()
            where sc.ClassId == request.ClassId
            select new
            {
                s.FullName,
                HeightCm = health != null ? health.HeightCm : null,
                WeightKg = health != null ? health.WeightKg : null,
                RecordAt = (DateOnly?)health.RecordAt
            })
            .ToListAsync(cancellationToken);

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
                RecordDate = x.RecordAt != null
                    ? x.RecordAt.Value.ToString("dd/MM/yyyy")
                    : "-"
            };
        }).ToList();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Chỉ số BMI học sinh");

        ws.Cell(1, 1).Value = "STT";
        ws.Cell(1, 2).Value = "Học sinh";
        ws.Cell(1, 3).Value = "Chiều cao";
        ws.Cell(1, 4).Value = "Cân nặng";
        ws.Cell(1, 5).Value = "BMI";
        ws.Cell(1, 6).Value = "Trạng thái";
        ws.Cell(1, 7).Value = "Ngày đo";

        var headerRange = ws.Range("A1:G1");
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

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

            var statusCell = ws.Cell(row, 6);
            switch (item.Status)
            {
                case "Thiếu cân":
                    statusCell.Style.Fill.BackgroundColor = XLColor.LightYellow;
                    break;
                case "Bình thường":
                    statusCell.Style.Fill.BackgroundColor = XLColor.LightGreen;
                    break;
                case "Thừa cân / Béo phì":
                    statusCell.Style.Fill.BackgroundColor = XLColor.LightPink;
                    break;
            }

            row++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return stream.ToArray();
    }

    #endregion

    #region 1️⃣1️⃣ GetHealthRecordsAsync

    public async Task<object> Handle(
        GetHealthRecordsQuery request,
        CancellationToken cancellationToken)
    {
        var temp = await (
            from sc in _repo.StudentClasses
            join s in _repo.Students on sc.StudentId equals s.StudentId
            join h in _repo.StudentHealthRecords on s.StudentId equals h.StudentId into healthJoin
            from health in healthJoin
                .OrderByDescending(x => x.RecordAt)
                .Take(1)
                .DefaultIfEmpty()
            where sc.ClassId == request.ClassId
            select new
            {
                s.StudentId,
                s.FullName,
                s.Gender,
                s.DateOfBirth,
                HeightCm = health.HeightCm,
                WeightKg = health.WeightKg
            })
            .ToListAsync(cancellationToken);

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

    #endregion

    #region 1️⃣2️⃣ SearchAsync

    public async Task<object> Handle(
        SearchStudentsInClassQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Keyword))
            return new { Students = new List<object>() };

        var keyword = request.Keyword.Trim().ToLower();
        var words = keyword.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var students = await (
            from sc in _repo.StudentClasses
            join s in _repo.Students on sc.StudentId equals s.StudentId
            join p in _repo.Users on s.ParentId equals p.UserId into parentJoin
            from parent in parentJoin.DefaultIfEmpty()
            where sc.ClassId == request.ClassId
                  &&
                  words.All(w =>
                      s.FullName.ToLower().Contains(w)
                      || (parent.FullName != null && parent.FullName.ToLower().Contains(w))
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
            }).ToListAsync(cancellationToken);

        return new { Students = students };
    }

    #endregion
}
