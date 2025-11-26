using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Manager.DTOs;
public class RecentPurchaseDto
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    //public string StaffName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string? Status { get; set; }
    public string? Note { get; set; }
}
