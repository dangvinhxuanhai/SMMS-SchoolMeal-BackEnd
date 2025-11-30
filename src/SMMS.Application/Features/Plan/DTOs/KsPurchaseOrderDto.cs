using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Plan.DTOs;
public class KsPurchaseOrderDto
{
    public int OrderId { get; set; }
    public Guid SchoolId { get; set; }
    public DateTime OrderDate { get; set; }
    public string? PurchaseOrderStatus { get; set; }
    public string? SupplierName { get; set; }
    public string? Note { get; set; }
    public int? PlanId { get; set; }
    public Guid? StaffInCharged { get; set; }

    public List<KsPurchaseOrderLineDto> Lines { get; set; } = new();
}
