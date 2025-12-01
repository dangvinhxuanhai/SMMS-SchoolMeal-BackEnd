using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Domain.Entities.foodmenu;

namespace SMMS.Application.Features.Meal.Interfaces;
public interface IMenuRepository
{
    Task<Menu> GetWithDetailsAsync(int menuId, CancellationToken ct = default);
    Task AddAsync(Menu menu, CancellationToken ct = default);
}
