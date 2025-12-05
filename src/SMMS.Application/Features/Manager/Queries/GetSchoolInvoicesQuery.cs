using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Manager.DTOs;

namespace SMMS.Application.Features.Manager.Queries;
public record GetSchoolInvoicesQuery(
    Guid SchoolId,
    short? MonthNo,
    int? Year,
    string? Status
) : IRequest<IReadOnlyList<InvoiceDto1>>;

public record GetSchoolInvoiceByIdQuery(
    Guid SchoolId,
    long InvoiceId
) : IRequest<InvoiceDto1?>;
