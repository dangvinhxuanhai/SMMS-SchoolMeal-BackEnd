using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.Manager.DTOs;

namespace SMMS.Application.Features.Manager.Interfaces;
public interface IManagerService
{
    Task<ManagerOverviewDto> GetOverviewAsync(Guid schoolId);
    Task<List<RecentPurchaseDto>> GetRecentPurchasesAsync(Guid schoolId, int take = 8);
    Task<RevenueSeriesDto> GetRevenueAsync(Guid schoolId, DateTime from, DateTime to, string granularity = "daily");
    Task<List<PurchaseOrderLineDto>> GetPurchaseOrderDetailsAsync(int orderId);
}
