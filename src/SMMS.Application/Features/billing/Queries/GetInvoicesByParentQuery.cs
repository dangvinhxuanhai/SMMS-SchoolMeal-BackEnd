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
    public record GetInvoicesByParentQuery(Guid StudenId) : IRequest<IEnumerable<InvoiceDto>>;
    // ✅ Query: Lấy chi tiết hóa đơn cụ thể
    public record GetInvoiceDetailQuery(long InvoiceId, Guid StudenId) : IRequest<InvoiceDetailDto?>;
    // Lấy hóa đơn chưa thanh toán 
    public record GetUnpaidInvoicesQuery(Guid StudentId) : IRequest<IEnumerable<InvoiceDto>>;
}
