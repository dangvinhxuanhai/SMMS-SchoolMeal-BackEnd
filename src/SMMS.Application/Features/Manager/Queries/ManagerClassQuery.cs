using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Manager.DTOs;

namespace SMMS.Application.Features.Manager.Queries;
// 🟢 Lấy tất cả lớp theo school
public record GetAllClassesQuery(Guid SchoolId)
    : IRequest<List<ClassDto>>;

// 🟣 Trạng thái phân công giáo viên
public record GetTeacherAssignmentStatusQuery(Guid SchoolId)
    : IRequest<object>; // giữ kiểu object giống service cũ
