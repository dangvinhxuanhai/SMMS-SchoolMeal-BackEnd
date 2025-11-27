using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Domain.Entities.rag;

namespace SMMS.Application.Features.foodmenu.Interfaces;
public interface IMenuRecommendSessionRepository
{
    Task<long> AddAsync(MenuRecommendSession session, CancellationToken ct = default);
}
