using System.Threading.Tasks;
using System.Collections.Generic;


namespace SMMS.Application.Features.Wardens.Interfaces;
    public interface IWardensHomeRepository
    {
        Task<object> GetDashboardAsync(int wardenId);
        Task<IEnumerable<object>> GetNotificationsAsync(int wardenId);
        Task<bool> AcknowledgeNotificationAsync(int wardenId, int notificationId);
    }



