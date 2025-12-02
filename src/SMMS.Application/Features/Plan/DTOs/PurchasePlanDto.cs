using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Plan.DTOs;
public class PurchasePlanDto
{
    public int PlanId { get; set; }
    public long ScheduleMealId { get; set; }
    public string PlanStatus { get; set; } = default!;
    public DateTime GeneratedAt { get; set; }

    public Guid StaffId { get; set; }
    public string StaffName { get; set; } = default!;

    public List<PurchasePlanLineDto> Lines { get; set; } = new();
}

public class PurchasePlanLineDto
{
    public int IngredientId { get; set; }
    public string IngredientName { get; set; } = default!;
    public decimal RqQuanityGram { get; set; }   // đúng tên cột trong DB
}

// Dùng cho update
public class UpdatePurchasePlanLineDto
{
    public int IngredientId { get; set; }
    public decimal RqQuanityGram { get; set; }
}

// Dùng cho màn "View all purchase plans"
public class PurchasePlanListItemDto
{
    public int PlanId { get; set; }
    public long ScheduleMealId { get; set; }

    public DateTime GeneratedAt { get; set; }
    public string PlanStatus { get; set; } = default!;
    public bool AskToDelete { get; set; }

    // Thông tin tuần từ ScheduleMeal
    public DateOnly WeekStart { get; set; }
    public DateOnly WeekEnd { get; set; }
    public short WeekNo { get; set; }
    public short YearNo { get; set; }

    // Nhân sự tạo kế hoạch
    public Guid StaffId { get; set; }
    public string StaffName { get; set; } = default!;
}
