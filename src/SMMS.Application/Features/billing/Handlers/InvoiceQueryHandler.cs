using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.billing.DTOs;
using SMMS.Application.Features.billing.Interfaces;
using SMMS.Application.Features.billing.Queries;

namespace SMMS.Application.Features.billing.Handlers
{
    public class InvoiceQueryHandler :
        IRequestHandler<GetInvoicesByParentQuery, IEnumerable<InvoiceDto>>,
        IRequestHandler<GetInvoiceDetailQuery, InvoiceDetailDto?>,
        IRequestHandler<GetUnpaidInvoicesQuery, IEnumerable<InvoiceDto>>
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public InvoiceQueryHandler(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task<IEnumerable<InvoiceDto>> Handle(GetInvoicesByParentQuery request, CancellationToken cancellationToken)
        {
            return await _invoiceRepository.GetInvoicesByParentAsync(request.StudenId);
        }

        public async Task<InvoiceDetailDto?> Handle(GetInvoiceDetailQuery request, CancellationToken cancellationToken)
        {
            return await _invoiceRepository.GetInvoiceDetailAsync(request.InvoiceId, request.StudenId);
        }
        public async Task<IEnumerable<InvoiceDto>> Handle(GetUnpaidInvoicesQuery request, CancellationToken cancellationToken)
        {
            return await _invoiceRepository.GetUnpaidInvoicesAsync(request.StudentId);
        }
    }
}
