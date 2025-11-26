using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.foodmenu.Interfaces;
using SMMS.Domain.Entities.rag;
using SMMS.Persistence.Data;

namespace SMMS.Persistence.Repositories.foodmenu;
public class MenuRecommendSessionRepository : IMenuRecommendSessionRepository
{
    private readonly EduMealContext _ctx;

    public MenuRecommendSessionRepository(EduMealContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<long> AddAsync(MenuRecommendSession session, CancellationToken ct = default)
    {
        await _ctx.MenuRecommendSessions.AddAsync(session, ct);
        await _ctx.SaveChangesAsync(ct);
        // Sau SaveChanges, SessionId (IDENTITY) sẽ được set vào entity.
        return session.SessionId;
    }
}
