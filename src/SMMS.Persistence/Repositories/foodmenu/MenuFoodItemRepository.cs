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
public class MenuFoodItemRepository : Repository<MenuFoodItem>, IMenuFoodItemRepository
{
    public MenuFoodItemRepository(EduMealContext dbContext) : base(dbContext)
    {
    }
}
