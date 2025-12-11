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
public sealed class SchoolPaymentGatewayRepository : ISchoolPaymentGatewayRepository
{
    private readonly EduMealContext _context; // đổi tên context cho đúng với project của anh

    public SchoolPaymentGatewayRepository(EduMealContext context)
    {
        _context = context;
    }

    public async Task<SchoolPaymentGateway?> GetPayOsGatewayAsync(
        Guid schoolId,
        CancellationToken cancellationToken)
    {
        return await _context.SchoolPaymentGateways
            .FirstOrDefaultAsync(
                x => x.SchoolId == schoolId
                     && x.TheProvider == "PayOS"
                     && x.IsActive,
                cancellationToken);
    }

    public async Task AddAsync(SchoolPaymentGateway gateway, CancellationToken cancellationToken)
    {
        await _context.SchoolPaymentGateways.AddAsync(gateway, cancellationToken);
    }

    public async Task<SchoolPaymentGateway?> GetPayOsGatewayByStudentIdAsync(
    Guid studentId,
    CancellationToken cancellationToken)
    {
        var query =
            from st in _context.Students
            join gw in _context.SchoolPaymentGateways
                on st.SchoolId equals gw.SchoolId
            where st.StudentId == studentId
                  && gw.TheProvider == "PayOS"
                  && gw.IsActive
            select gw;

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
}
