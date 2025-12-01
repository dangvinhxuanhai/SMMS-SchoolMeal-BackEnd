using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using SMMS.Application.Features.Manager.DTOs;

namespace SMMS.Application.Features.Manager.Commands;
// 🟡 Tạo phụ huynh + con + gán lớp
public record CreateParentCommand(CreateParentRequest Request)
    : IRequest<AccountDto>;

// 🟠 Cập nhật phụ huynh + con + lớp
public record UpdateParentCommand(Guid UserId, UpdateParentRequest Request)
    : IRequest<AccountDto?>;

// 🔵 Đổi trạng thái kích hoạt
public record ChangeParentStatusCommand(Guid UserId, bool IsActive)
    : IRequest<bool>;

// 🔴 Xóa tài khoản phụ huynh + con + lớp
//public record DeleteParentCommand(Guid UserId)
//    : IRequest<bool>;
// Xóa quan hệ phụ huynh–học sinh trong 1 trường (không xóa account global)
public record DeleteParentCommand(Guid UserId, Guid SchoolId)
    : IRequest<bool>;
// 📥 Import từ Excel
public record ImportParentsFromExcelCommand(Guid SchoolId, IFormFile File, string CreatedBy)
    : IRequest<List<AccountDto>>;
