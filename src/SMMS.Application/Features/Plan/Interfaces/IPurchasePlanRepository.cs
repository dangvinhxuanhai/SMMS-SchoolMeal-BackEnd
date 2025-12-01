using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.Plan.DTOs;
using SMMS.Domain.Entities.purchasing;

namespace SMMS.Application.Features.Plan.Interfaces;
public interface IPurchasePlanRepository
{
    Task<int> CreateFromScheduleAsync(
        long scheduleMealId,
        Guid staffId,
        CancellationToken cancellationToken);

    Task<PurchasePlanDto?> GetPlanDetailAsync(
        int planId,
        CancellationToken cancellationToken);

    Task UpdatePlanWithLinesAsync(
        int planId,
        string planStatus,
        Guid? confirmedBy,
        List<UpdatePurchasePlanLineDto> lines,
        CancellationToken cancellationToken);

    Task DeletePlanAsync(
        int planId,
        CancellationToken cancellationToken);

    // ====== NEW: GET ALL ======

    // Lấy tất cả plan của 1 trường (mặc định chỉ lấy AskToDelete = 0)
    Task<List<PurchasePlanListItemDto>> GetPlansForSchoolAsync(
        Guid schoolId,
        bool includeDeleted,
        CancellationToken cancellationToken);

    // Lấy plan theo ngày: tìm ScheduleMeal của tuần chứa date đó
    Task<PurchasePlanDto?> GetPlanByDateAsync(
        Guid schoolId,
        DateOnly date,
        CancellationToken cancellationToken);

    Task SoftDeletePlanAsync(
            int planId,
            CancellationToken cancellationToken);

    Task<PurchasePlan?> GetByIdAsync(int planId, CancellationToken ct = default);

    Task<IReadOnlyList<PurchasePlanLine>> GetLinesAsync(
        int planId,
        CancellationToken ct = default);

    // Lấy SchoolId của plan bằng cách join ScheduleMeal
    Task<Guid> GetSchoolIdAsync(int planId, CancellationToken ct = default);
}
