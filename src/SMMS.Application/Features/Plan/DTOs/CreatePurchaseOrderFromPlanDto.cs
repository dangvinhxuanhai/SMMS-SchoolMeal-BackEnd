using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MediatR;

namespace SMMS.Application.Features.Plan.DTOs;
public class CreatePurchaseOrderFromPlanDto
{
    public int PlanId { get; set; }
    public string SupplierName { get; set; } = default!;
    public string? Note { get; set; }
    public List<CreatePurchaseOrderLineFromPlanDto> Lines { get; set; } = new();
}
