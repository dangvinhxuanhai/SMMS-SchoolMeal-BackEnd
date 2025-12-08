using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Manager.DTOs;
public class FinanceSummaryDto
{
    public Guid SchoolId { get; set; }
    public int Year { get; set; }

    public decimal TotalInvoices { get; set; }
    public decimal TotalPurchaseCost { get; set; }
    public List<SupplierExpenseDto> SupplierBreakdown { get; set; } = new();
}

public class SupplierExpenseDto
{
    public string Supplier { get; set; } = string.Empty;
    public decimal Total { get; set; }
}
public class InvoiceDto
{
    public long InvoiceId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public int MonthNo { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int AbsentDay { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class InvoiceDetailDto
{
    public long InvoiceId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public int MonthNo { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<PaymentDto> Payments { get; set; } = new();
}

public class PaymentDto
{
    public long PaymentId { get; set; }
    public decimal ExpectedAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
}
// 🛒 Đơn hàng (PurchaseOrder)
public class PurchaseOrderDto
{
    public int OrderId { get; set; }
    public Guid SchoolId { get; set; }
    public DateTime OrderDate { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string PurchaseOrderStatus { get; set; } = string.Empty;
    public string? Note { get; set; }
    public decimal TotalAmount { get; set; }
}

public class PurchaseOrderDetailDto
{
    public int OrderId { get; set; }
    public Guid SchoolId { get; set; }
    public DateTime OrderDate { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string PurchaseOrderStatus { get; set; } = string.Empty;
    public string? Note { get; set; }
    public decimal TotalAmount { get; set; } // ✅ Tổng tiền
    public List<PurchaseOrderLineDto> Lines { get; set; } = new();
}
