using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Plan.DTOs;
public class CreatePurchasePlanResult
{
    public bool IsCreated { get; set; }
    public int? PurchasePlanId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public static class PurchasePlanResultReason
{
    public const string InventoryEnough = "InventoryEnough";
    public const string Created = "Created";
}
