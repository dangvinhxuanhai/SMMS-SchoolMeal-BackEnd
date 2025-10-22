using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.billing.Interfaces;
using SMMS.Domain.Entities.billing;
using SMMS.Persistence.DbContextSite;
using SMMS.Persistence.Repositories.Skeleton;

namespace SMMS.Persistence.Repositories.billing;
public class NotificationService : Repository<Notification>, INotificationRepository
{
    public NotificationService(EduMealContext dbContext) : base(dbContext)
    {
    }
}
