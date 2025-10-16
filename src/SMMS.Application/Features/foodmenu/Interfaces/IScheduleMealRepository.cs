using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.Skeleton.Interfaces;
using SMMS.Domain.Models.foodmenu;

namespace SMMS.Application.Features.foodmenu.Interfaces;
public interface IScheduleMealRepository : IRepository<ScheduleMeal>
{
}
