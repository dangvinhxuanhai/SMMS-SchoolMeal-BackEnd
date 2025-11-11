using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.billing.Interfaces;
using SMMS.Domain.Entities.billing;
using SMMS.Persistence.Data;
using SMMS.Persistence.Repositories.Skeleton;

namespace SMMS.Persistence.Repositories.billing;
public class NotificationRecipientRepository : Repository<NotificationRecipient>, INotificationRecipientRepository
{
    public NotificationRecipientRepository(EduMealContext dbContext) : base(dbContext)
    {
    }
}
