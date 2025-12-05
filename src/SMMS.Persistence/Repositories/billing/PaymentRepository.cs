using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.billing.Interfaces;
using SMMS.Domain.Entities.billing;
using SMMS.Persistence.Data;

namespace SMMS.Persistence.Repositories.billing;
public sealed class PaymentRepository : IPaymentRepository
{
    private readonly EduMealContext _context;

    public PaymentRepository(EduMealContext context)
    {
        _context = context;
    }

    public Task<Payment?> GetByIdAsync(long paymentId, CancellationToken ct)
    {
        return _context.Payments
            .FirstOrDefaultAsync(p => p.PaymentId == paymentId, ct);
    }

    public async Task AddAsync(Payment payment, CancellationToken ct)
    {
        await _context.Payments.AddAsync(payment, ct);
        // SaveChanges sẽ được gọi ở IUnitOfWork bên ngoài (handler)
    }
}
