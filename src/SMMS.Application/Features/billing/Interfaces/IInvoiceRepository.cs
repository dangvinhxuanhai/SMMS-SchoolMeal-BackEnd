using SMMS.Application.Features.billing.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMMS.Application.Features.billing.Interfaces
{
    public interface IInvoiceRepository
    {
        Task<IEnumerable<InvoiceDto>> GetInvoicesByParentAsync(Guid studentId);
        Task<InvoiceDto?> GetInvoiceDetailAsync(long invoiceId, Guid studentId);
        Task<IEnumerable<InvoiceDto>> GetUnpaidInvoicesAsync(Guid studentId);
    }
}
