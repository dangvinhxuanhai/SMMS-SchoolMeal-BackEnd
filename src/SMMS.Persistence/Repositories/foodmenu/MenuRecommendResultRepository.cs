using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.foodmenu.Interfaces;
using SMMS.Domain.Entities.rag;
using SMMS.Persistence.Data;

namespace SMMS.Persistence.Repositories.foodmenu;
public class MenuRecommendResultRepository : IMenuRecommendResultRepository
{
    private readonly EduMealContext _db;

    public MenuRecommendResultRepository(EduMealContext db)
    {
        _db = db;
    }

    public async Task MarkChosenAsync(
        Guid userId,
        long sessionId,
        IEnumerable<(int foodId, bool isMain)> selected,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var pSessionId = new SqlParameter("@SessionId", sessionId);
        var pNow = new SqlParameter("@Now", now);

        foreach (var (foodId, isMain) in selected)
        {
            var pFoodId = new SqlParameter("@FoodId", foodId);
            var pIsMain = new SqlParameter("@IsMain", isMain);

            var sql = @"
            UPDATE rag.MenuRecommendResults
            SET IsChosen = 1,
                ChosenAt = @Now
            WHERE SessionId = @SessionId
              AND FoodId = @FoodId
              AND IsMain = @IsMain;";

            await _db.Database.ExecuteSqlRawAsync(
                sql,
                new object[] { pSessionId, pFoodId, pIsMain, pNow },
                cancellationToken);
        }
    }

    public async Task AddRangeAsync(IEnumerable<MenuRecommendResult> results, CancellationToken ct = default)
    {
        await _db.MenuRecommendResults.AddRangeAsync(results, ct);
        await _db.SaveChangesAsync(ct);
    }
}
