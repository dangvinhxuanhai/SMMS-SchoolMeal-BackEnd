using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
