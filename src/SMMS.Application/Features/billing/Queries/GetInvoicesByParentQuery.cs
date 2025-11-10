using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.billing.DTOs;

namespace SMMS.Application.Features.billing.Queries
{
    // ✅ Query: Lấy danh sách hóa đơn của phụ huynh
    public record GetInvoicesByParentQuery(Guid ParentId) : IRequest<IEnumerable<InvoiceDto>>;

    // ✅ Query: Lấy chi tiết hóa đơn cụ thể
    public record GetInvoiceDetailQuery(long InvoiceId, Guid ParentId) : IRequest<InvoiceDto?>;
}
