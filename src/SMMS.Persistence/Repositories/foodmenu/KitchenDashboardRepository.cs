using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.foodmenu.DTOs;
using SMMS.Application.Features.foodmenu.Interfaces;
using SMMS.Persistence.Data;

namespace SMMS.Persistence.Repositories.foodmenu;
public class KitchenDashboardRepository : IKitchenDashboardRepository
{
    private readonly EduMealContext _context;

    public KitchenDashboardRepository(EduMealContext context)
    {
        _context = context;
    }

    // ------------------- 1. TodaySummary -------------------
    public async Task<TodaySummaryDto> GetTodaySummaryAsync(
        Guid schoolId,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        // Base query DailyMeals của trường + ngày
        var dailyMealsQuery =
            from dm in _context.DailyMeals
            join sm in _context.ScheduleMeals
                on dm.ScheduleMealId equals sm.ScheduleMealId
            where sm.SchoolId == schoolId
                  && dm.MealDate == date
            select dm;

        var totalDailyMealsToday = await dailyMealsQuery
            .CountAsync(cancellationToken);

        var totalDishesToday = await (
            from dm in dailyMealsQuery
            join mfi in _context.MenuFoodItems
                on dm.DailyMealId equals mfi.DailyMealId
            select mfi.FoodId
        ).CountAsync(cancellationToken);

        var absenceCountToday = await (
            from a in _context.Attendances
            join s in _context.Students
                on a.StudentId equals s.StudentId
            where s.SchoolId == schoolId
                  && a.AbsentDate == date
            select a.AttendanceId
        ).CountAsync(cancellationToken);

        var feedbackCountToday = await (
            from f in _context.Feedbacks
            join dm in _context.DailyMeals
                on f.DailyMealId equals dm.DailyMealId
            join sm in _context.ScheduleMeals
                on dm.ScheduleMealId equals sm.ScheduleMealId
            where sm.SchoolId == schoolId
                  && dm.MealDate == date
            select f.FeedbackId
        ).CountAsync(cancellationToken);

        // TODO: threshold LowStock nên đưa vào config hoặc column riêng
        const decimal LowStockThresholdGram = 1000m;

        var lowStockItemCount = await _context.InventoryItems
            .Where(ii => ii.SchoolId == schoolId
                         && ii.QuantityGram <= LowStockThresholdGram)
            .CountAsync(cancellationToken);

        var nearExpiryDate = date.AddDays(3);

        var nearExpiryItemCount = await _context.InventoryItems
            .Where(ii => ii.SchoolId == schoolId
                         && ii.ExpirationDate != null
                         && ii.ExpirationDate >= date
                         && ii.ExpirationDate <= nearExpiryDate)
            .CountAsync(cancellationToken);

        var openPurchaseOrderCount = await _context.PurchaseOrders
            .Where(po => po.SchoolId == schoolId
                         && po.PurchaseOrderStatus != "Completed")
            .CountAsync(cancellationToken);

        double avgDishesPerMealToday = totalDailyMealsToday > 0
            ? (double)totalDishesToday / totalDailyMealsToday
            : 0;

        return new TodaySummaryDto
        {
            TotalDailyMealsToday = totalDailyMealsToday,
            TotalDishesToday = totalDishesToday,
            AbsenceCountToday = absenceCountToday,
            FeedbackCountToday = feedbackCountToday,
            LowStockItemCount = lowStockItemCount,
            NearExpiryItemCount = nearExpiryItemCount,
            OpenPurchaseOrderCount = openPurchaseOrderCount,
            AvgDishesPerMealToday = avgDishesPerMealToday
        };
    }

    // ------------------- 2. AbsenceRequests (top 5) -------------------
    public async Task<List<AbsenceRequestShortDto>> GetAbsenceRequestsAsync(
        Guid schoolId,
        DateOnly date,
        int take,
        CancellationToken cancellationToken)
    {
        var query =
            from a in _context.Attendances
            join s in _context.Students
                on a.StudentId equals s.StudentId
            join sc in _context.StudentClasses
                on s.StudentId equals sc.StudentId
            join c in _context.Classes
                on sc.ClassId equals c.ClassId
            join u in _context.Users
                on a.NotifiedBy equals u.UserId into notifyJoin
            from notified in notifyJoin.DefaultIfEmpty()
            where s.SchoolId == schoolId
                  && a.AbsentDate >= date             // từ hôm nay trở đi
                  && (sc.LeftDate == null || sc.LeftDate >= a.AbsentDate)
                  && sc.RegistStatus == true          // đã duyệt
            orderby a.AbsentDate descending, a.CreatedAt descending
            select new AbsenceRequestShortDto
            {
                AttendanceId = a.AttendanceId,
                AbsentDate = a.AbsentDate,
                StudentName = s.FullName,
                ClassName = c.ClassName,
                ReasonShort = a.Reason != null && a.Reason.Length > 80
                    ? a.Reason.Substring(0, 80) + "..."
                    : a.Reason,
                NotifiedByName = notified != null ? notified.FullName : null,
                CreatedAt = a.CreatedAt
            };

        return await query
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    // ------------------- 3. RecentFeedbacks (top 5) -------------------
    public async Task<List<FeedbackShortDto>> GetRecentFeedbacksAsync(
        Guid schoolId,
        int take,
        CancellationToken cancellationToken)
    {
        var query =
            from f in _context.Feedbacks
            join u in _context.Users
                on f.SenderId equals u.UserId
            join dm in _context.DailyMeals
                on f.DailyMealId equals dm.DailyMealId
            join sm in _context.ScheduleMeals
                on dm.ScheduleMealId equals sm.ScheduleMealId
            where sm.SchoolId == schoolId
            orderby f.CreatedAt descending
            select new FeedbackShortDto
            {
                FeedbackId = f.FeedbackId,
                CreatedAt = f.CreatedAt,
                SenderName = u.FullName,
                MealDate = dm.MealDate,
                MealType = dm.MealType,
                ContentPreview = f.Content.Length > 80
                    ? f.Content.Substring(0, 80) + "..."
                    : f.Content
            };

        return await query
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    // ------------------- 4. InventoryAlerts (top 5) -------------------
    public async Task<List<InventoryAlertShortDto>> GetInventoryAlertsAsync(
        Guid schoolId,
        DateOnly today,
        int take,
        CancellationToken cancellationToken)
    {
        const decimal LowStockThresholdGram = 1000m;
        var nearExpiryDate = today.AddDays(3);

        var query =
            from ii in _context.InventoryItems
            join ing in _context.Ingredients
                on ii.IngredientId equals ing.IngredientId
            where ii.SchoolId == schoolId
            let isExpired = ii.ExpirationDate != null && ii.ExpirationDate < today
            let isNearExpiry = ii.ExpirationDate != null && ii.ExpirationDate >= today && ii.ExpirationDate <= nearExpiryDate
            let isLowStock = ii.QuantityGram <= LowStockThresholdGram
            where isExpired || isNearExpiry || isLowStock
            let priority = isExpired ? 1 : (isNearExpiry ? 2 : 3)
            orderby priority, ii.ExpirationDate, ii.QuantityGram
            select new InventoryAlertShortDto
            {
                ItemId = ii.ItemId,
                IngredientName = ing.IngredientName,
                ItemName = ii.ItemName ?? ing.IngredientName,
                QuantityGram = ii.QuantityGram,
                ExpirationDate = ii.ExpirationDate,
                AlertType = isExpired ? "Expired"
                               : (isNearExpiry ? "NearExpiry" : "LowStock")
            };

        return await query
            .Take(take)
            .ToListAsync(cancellationToken);
    }
}
