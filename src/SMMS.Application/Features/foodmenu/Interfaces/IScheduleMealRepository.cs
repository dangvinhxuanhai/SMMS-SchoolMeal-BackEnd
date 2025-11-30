using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.Skeleton.Interfaces;
using SMMS.Domain.Entities.foodmenu;

namespace SMMS.Application.Features.foodmenu.Interfaces;
public interface IScheduleMealRepository
{
    Task<int> CountBySchoolAsync(Guid schoolId, CancellationToken ct = default);

    Task<IReadOnlyList<ScheduleMeal>> GetPagedBySchoolAsync(
        Guid schoolId,
        int pageIndex,
        int pageSize,
        CancellationToken ct = default);

    Task<ScheduleMeal?> GetForDateAsync(
        Guid schoolId,
        DateTime date,
        CancellationToken ct = default);

    Task<IReadOnlyList<DailyMeal>> GetDailyMealsForSchedulesAsync(
        IEnumerable<long> scheduleMealIds,
        CancellationToken ct = default);

    Task<IReadOnlyList<DailyMeal>> GetDailyMealsForScheduleAsync(
        long scheduleMealId,
        CancellationToken ct = default);
}
