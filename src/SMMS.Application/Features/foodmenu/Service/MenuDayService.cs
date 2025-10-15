using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.foodmenu.Interfaces;
using SMMS.Application.Features.Skeleton.Service;
using SMMS.Domain.Models.foodmenu;
using SMMS.Persistence.DbContextSite;

namespace SMMS.Application.Features.foodmenu.Service;
public class MenuDayService : Service<MenuDay>, IMenuDayService
{
    public MenuDayService(EduMealContext dbContext) : base(dbContext)
    {
    }
}
