using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SMMS.Application.Features.billing.DTOs;
using BillingInvoice = SMMS.Domain.Entities.billing.Invoice;

namespace SMMS.Application.Features.billing.Interfaces
{
    public interface IInvoiceRepository
    {
        Task<BillingInvoice?> GetByIdAsync(long invoiceId, CancellationToken ct);
        Task<IEnumerable<InvoiceDto>> GetInvoicesByParentAsync(Guid studentId);
        Task<InvoiceDetailDto?> GetInvoiceDetailAsync(long invoiceId, Guid studentId);
        Task<IEnumerable<InvoiceDto>> GetUnpaidInvoicesAsync(Guid studentId);
    }
}
