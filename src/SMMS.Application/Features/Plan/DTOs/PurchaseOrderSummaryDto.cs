using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Plan.DTOs;
public class PurchaseOrderSummaryDto
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public string? PurchaseOrderStatus { get; set; }
    public string? SupplierName { get; set; }
    public int? PlanId { get; set; }

    // tổng quan cho list
    public int LinesCount { get; set; }
    public decimal TotalQuantityGram { get; set; }
}
