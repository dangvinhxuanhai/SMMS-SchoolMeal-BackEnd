using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Domain.Entities.billing;
using SMMS.Domain.Entities.purchasing;

namespace SMMS.Application.Features.Manager.Interfaces;
public interface IManagerFinanceRepository
{
    IQueryable<Invoice> Invoices { get; }
    IQueryable<Payment> Payments { get; }
    IQueryable<PurchaseOrder> PurchaseOrders { get; }
    IQueryable<PurchaseOrderLine> PurchaseOrderLines { get; }
    Task<List<Invoice>> GetInvoicesBySchoolAsync(Guid schoolId);

    // 🟡 Lấy chi tiết 1 hóa đơn (bao gồm Payment)
    Task<Invoice?> GetInvoiceDetailAsync(long invoiceId);

    // 🔵 Lấy các phiếu nhập hàng của trường trong tháng/năm
    Task<List<PurchaseOrder>> GetPurchaseOrdersByMonthAsync(Guid schoolId, int month, int year);

    // 🔴 Lấy chi tiết 1 phiếu nhập hàng
    Task<PurchaseOrder?> GetPurchaseOrderDetailAsync(int orderId);
}
