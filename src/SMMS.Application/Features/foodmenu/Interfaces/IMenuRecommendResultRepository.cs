using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Domain.Entities.rag;

namespace SMMS.Application.Features.foodmenu.Interfaces;
/// <summary>
/// Repo thao tác với bảng rag.MenuRecommendResults (update IsChosen, ChosenAt).
/// </summary>
public interface IMenuRecommendResultRepository
{
    Task MarkChosenAsync(
        Guid userId,
        long sessionId,
        IEnumerable<(int foodId, bool isMain)> selected,
        CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<MenuRecommendResult> results, CancellationToken ct = default);
}
