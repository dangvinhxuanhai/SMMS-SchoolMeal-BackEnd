using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Domain.Entities.billing;

namespace SMMS.Application.Features.Manager.Interfaces;
public interface IManagerPaymentSettingRepository
{
    Task<List<SchoolPaymentSetting>> GetBySchoolAsync(
        Guid schoolId,
        CancellationToken cancellationToken = default);

    Task<SchoolPaymentSetting?> GetByIdAsync(
        int settingId,
        CancellationToken cancellationToken = default);

    Task<SchoolPaymentSetting> AddAsync(
        SchoolPaymentSetting entity,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        SchoolPaymentSetting entity,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        SchoolPaymentSetting entity,
        CancellationToken cancellationToken = default);
}
