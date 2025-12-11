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

    // ✅ Thêm hàm này để check trùng khoảng tháng
    Task<bool> HasOverlappedRangeAsync(
           Guid schoolId,
           byte fromMonth,
           byte toMonth,
           int? excludeSettingId = null,
           CancellationToken cancellationToken = default);
    /// Lấy cấu hình thu phí trùng chính xác khoảng tháng [fromMonth, toMonth]
    Task<SchoolPaymentSetting?> GetExactRangeAsync(
        Guid schoolId,
        short fromMonth,
        short toMonth,
        CancellationToken ct);
}
