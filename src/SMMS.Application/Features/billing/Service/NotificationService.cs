using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.billing.Interfaces;
using SMMS.Application.Features.Skeleton.Service;
using SMMS.Domain.Models.billing;
using SMMS.Persistence.DbContextSite;

namespace SMMS.Application.Features.billing.Service;
public class NotificationService : Service<Notification>, INotificationService
{
    public NotificationService(EduMealContext dbContext) : base(dbContext)
    {
    }
}
