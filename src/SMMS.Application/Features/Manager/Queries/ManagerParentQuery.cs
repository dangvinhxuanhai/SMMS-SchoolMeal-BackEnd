using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Manager.DTOs;

namespace SMMS.Application.Features.Manager.Queries;
// 🔍 Tìm kiếm phụ huynh
public record SearchParentsQuery(Guid SchoolId, string Keyword)
    : IRequest<List<ParentAccountDto>>;

// 🟢 Lấy danh sách phụ huynh (theo trường / lớp)
public record GetParentsQuery(Guid SchoolId, Guid? ClassId)
    : IRequest<List<ParentAccountDto>>;
// 📄 Template Excel
public record GetParentExcelTemplateQuery() : IRequest<byte[]>;
