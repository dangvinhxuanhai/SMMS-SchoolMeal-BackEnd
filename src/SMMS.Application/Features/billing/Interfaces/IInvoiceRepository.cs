using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SMMS.Application.Features.billing.DTOs;
using SMMS.Domain.Entities.billing;

namespace SMMS.Application.Features.billing.Interfaces
{
    public interface IInvoiceRepository
    {
        Task<Invoice?> GetByIdAsync(long invoiceId, CancellationToken ct);
        Task<IEnumerable<InvoiceDto>> GetInvoicesByParentAsync(Guid studentId);
        Task<InvoiceDetailDto?> GetInvoiceDetailAsync(long invoiceId, Guid studentId);
        Task<IEnumerable<InvoiceDto>> GetUnpaidInvoicesAsync(Guid studentId);
    }
}
