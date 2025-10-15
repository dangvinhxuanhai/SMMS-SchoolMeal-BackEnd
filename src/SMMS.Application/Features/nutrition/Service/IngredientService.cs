using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.nutrition.Interfaces;
using SMMS.Application.Features.Skeleton.Service;
using SMMS.Domain.Models.nutrition;
using SMMS.Persistence.DbContextSite;

namespace SMMS.Application.Features.nutrition.Service;
public class IngredientService : Service<Ingredient>, IIngredientService
{
    public IngredientService(EduMealContext dbContext) : base(dbContext)
    {
    }
}
