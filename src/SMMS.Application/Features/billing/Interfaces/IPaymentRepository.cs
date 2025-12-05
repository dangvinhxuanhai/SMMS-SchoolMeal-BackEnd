using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Domain.Entities.billing;

namespace SMMS.Application.Features.billing.Interfaces;
public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(long paymentId, CancellationToken ct);
    Task AddAsync(Payment payment, CancellationToken ct);
}
