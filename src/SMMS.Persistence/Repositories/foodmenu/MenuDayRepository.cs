using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.foodmenu.Interfaces;
using SMMS.Domain.Entities.foodmenu;
using SMMS.Persistence.Data;
using SMMS.Persistence.Repositories.Skeleton;

namespace SMMS.Persistence.Repositories.foodmenu;
public class MenuDayRepository : Repository<MenuDay>, IMenuDayRepository
{
    public MenuDayRepository(EduMealContext dbContext) : base(dbContext)
    {
    }
}
