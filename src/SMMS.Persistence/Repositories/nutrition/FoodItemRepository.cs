using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.nutrition.Interfaces;
using SMMS.Domain.Entities.nutrition;
using SMMS.Persistence.Data;
using SMMS.Persistence.Repositories.Skeleton;

namespace SMMS.Persistence.Repositories.nutrition;
public class FoodItemRepository : Repository<FoodItem>, IFoodItemRepository
{
    public FoodItemRepository(EduMealContext dbContext) : base(dbContext)
    {
    }
}
