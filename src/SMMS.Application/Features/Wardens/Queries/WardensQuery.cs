using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Wardens.DTOs;

namespace SMMS.Application.Features.Wardens.Queries;
// 1️⃣ Danh sách lớp của giám thị
public record GetWardenClassesQuery(Guid WardenId)
    : IRequest<IEnumerable<ClassDto>>;

// 2️⃣ Điểm danh chi tiết 1 lớp
public record GetClassAttendanceQuery(Guid ClassId)
    : IRequest<ClassAttendanceDto>;

// 3️⃣ Export báo cáo điểm danh 1 lớp (Excel)
public record ExportAttendanceReportQuery(Guid ClassId)
    : IRequest<byte[]>;

// 4️⃣ Danh sách học sinh trong lớp
public record GetStudentsInClassQuery(Guid ClassId)
    : IRequest<IEnumerable<StudentDto>>;

// 5️⃣ Tổng hợp sức khỏe tất cả học sinh thuộc các lớp của giám thị
public record GetHealthSummaryQuery(Guid WardenId)
    : IRequest<HealthSummaryDto>;
public record GetStudentBmiHistoryQuery(Guid StudentId)
    : IRequest<IEnumerable<StudentHealthDto>>;
// 6️⃣ Sức khỏe học sinh trong 1 lớp
public record GetStudentsHealthQuery(Guid ClassId)
    : IRequest<IEnumerable<StudentHealthDto>>;

// 7️⃣ Dashboard của giám thị
public record GetWardenDashboardQuery(Guid WardenId)
    : IRequest<DashboardDto>;

// 8️⃣ Thông báo của giám thị
public record GetWardenNotificationsQuery(Guid WardenId)
    : IRequest<IEnumerable<NotificationDto>>;

// 9️⃣ Export danh sách học sinh trong lớp (Excel)
public record ExportClassStudentsQuery(Guid ClassId)
    : IRequest<byte[]>;

// 🔟 Export sức khỏe học sinh trong lớp (Excel)
public record ExportClassHealthQuery(Guid ClassId)
    : IRequest<byte[]>;

// 1️⃣1️⃣ Lấy health records (object)
public record GetHealthRecordsQuery(Guid ClassId)
    : IRequest<object>;

// 1️⃣2️⃣ Search học sinh/phụ huynh trong lớp
public record SearchStudentsInClassQuery(Guid ClassId, string Keyword)
    : IRequest<object>;
