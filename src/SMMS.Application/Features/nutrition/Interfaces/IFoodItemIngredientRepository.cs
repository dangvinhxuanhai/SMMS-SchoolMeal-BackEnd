using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.Skeleton.Interfaces;
using SMMS.Domain.Models.nutrition;

namespace SMMS.Application.Features.nutrition.Interfaces;
public interface IFoodItemIngredientRepository : IRepository<FoodItemIngredient>
{
}
