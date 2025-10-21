using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.Skeleton.Interfaces;
using SMMS.Domain.Entities.billing;

namespace SMMS.Application.Features.billing.Interfaces;
public interface INotificationRecipientRepository : IRepository<NotificationRecipient>
{
}
