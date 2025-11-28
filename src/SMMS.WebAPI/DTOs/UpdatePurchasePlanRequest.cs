using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.Plan.DTOs;

namespace SMMS.WebAPI.DTOs;
public class UpdatePurchasePlanRequest
{
    public int PlanId { get; set; }
    public string PlanStatus { get; set; } = "Draft";
    public System.Collections.Generic.List<UpdatePurchasePlanLineDto> Lines { get; set; } = new();
}
