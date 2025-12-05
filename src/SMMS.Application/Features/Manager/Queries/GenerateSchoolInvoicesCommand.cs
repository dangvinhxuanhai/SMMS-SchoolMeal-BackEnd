using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Manager.DTOs;

namespace SMMS.Application.Features.Manager.Queries;
public record GenerateSchoolInvoicesCommand(
    Guid SchoolId,
    GenerateSchoolInvoicesRequest Request
) : IRequest<IReadOnlyList<InvoiceDto1>>;

public record UpdateInvoiceCommand(
    Guid SchoolId,
    long InvoiceId,
    UpdateInvoiceRequest Request
) : IRequest<InvoiceDto1?>;

public record DeleteInvoiceCommand(
    Guid SchoolId,
    long InvoiceId
) : IRequest<bool>;
