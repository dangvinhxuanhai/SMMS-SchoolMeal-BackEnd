using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Domain.Entities.billing;

namespace SMMS.Application.Features.billing.Interfaces;
public interface ISchoolPaymentGatewayRepository
{
    Task<SchoolPaymentGateway?> GetPayOsGatewayAsync(
        Guid schoolId,
        CancellationToken cancellationToken);

    Task AddAsync(SchoolPaymentGateway gateway, CancellationToken cancellationToken);
    Task<SchoolPaymentGateway?> GetPayOsGatewayByStudentIdAsync(
    Guid studentId,
    CancellationToken cancellationToken);
}
