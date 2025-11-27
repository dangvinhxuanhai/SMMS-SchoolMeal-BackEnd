using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.foodmenu.DTOs;

public class KitchenDashboardDto
{
    public TodaySummaryDto TodaySummary { get; set; } = new();
    public List<AbsenceRequestShortDto> AbsenceRequests { get; set; } = new();
    public List<FeedbackShortDto> RecentFeedbacks { get; set; } = new();
    public List<InventoryAlertShortDto> InventoryAlerts { get; set; } = new();
}

public class TodaySummaryDto
{
    public int TotalDailyMealsToday { get; set; }      // Số DailyMeals hôm nay
    public int TotalDishesToday { get; set; }          // Tổng món trong các DailyMeals
    public int AbsenceCountToday { get; set; }         // Số HS nghỉ hôm nay
    public int FeedbackCountToday { get; set; }        // Số feedback hôm nay
    public int LowStockItemCount { get; set; }         // Số item LowStock
    public int NearExpiryItemCount { get; set; }       // Số item NearExpiry
    public int OpenPurchaseOrderCount { get; set; }    // Số đơn mua chưa hoàn tất
    public double AvgDishesPerMealToday { get; set; }  // TB số món / bữa
}

public class AbsenceRequestShortDto
{
    public int AttendanceId { get; set; }       // school.Attendance.AttendanceId
    public DateOnly AbsentDate { get; set; }    // Attendance.AbsentDate
    public string StudentName { get; set; } = default!;
    public string ClassName { get; set; } = default!;
    public string? ReasonShort { get; set; }
    public string? NotifiedByName { get; set; } // auth.Users.FullName
    public DateTime CreatedAt { get; set; }     // Attendance.CreatedAt
}

public class FeedbackShortDto
{
    public int FeedbackId { get; set; }         // foodmenu.Feedbacks.FeedbackId
    public DateTime CreatedAt { get; set; }     // Feedbacks.CreatedAt

    public DateOnly MealDate { get; set; }      // DailyMeals.MealDate
    public string MealType { get; set; } = default!;  // DailyMeals.MealType
    public string SenderName { get; set; } = default!; // auth.Users.FullName
    public string ContentPreview { get; set; } = default!;
}

public class InventoryAlertShortDto
{
    public int ItemId { get; set; }             // inventory.InventoryItems.ItemId
    public string IngredientName { get; set; } = default!; // nutrition.Ingredients.IngredientName
    public string ItemName { get; set; } = default!;       // InventoryItems.ItemName hoặc IngredientName
    public decimal QuantityGram { get; set; }   // InventoryItems.QuantityGram
    public DateOnly? ExpirationDate { get; set; } // InventoryItems.ExpirationDate
    public string AlertType { get; set; } = default!;     // "Expired" | "NearExpiry" | "LowStock"
}
