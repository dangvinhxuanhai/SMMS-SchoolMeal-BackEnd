using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.Manager.DTOs;

namespace SMMS.Application.Features.Manager.Interfaces;
public interface IManagerFinanceService
{
    Task<FinanceSummaryDto> GetFinanceSummaryAsync(Guid schoolId, int month, int year);
    // 🟡 2️⃣ Danh sách hóa đơn của trường
    Task<List<InvoiceDto>> GetInvoicesAsync(Guid schoolId);

    // 🟠 3️⃣ Chi tiết hóa đơn (gồm thông tin học sinh và thanh toán)
    Task<InvoiceDetailDto?> GetInvoiceDetailAsync(long invoiceId);

    // 🔵 4️⃣ Danh sách đơn hàng trong tháng
    Task<List<PurchaseOrderDto>> GetPurchaseOrdersByMonthAsync(Guid schoolId, int month, int year);

    // 🔴 5️⃣ Chi tiết đơn hàng (kèm nguyên liệu / PurchaseOrderLines)
    Task<PurchaseOrderDetailDto?> GetPurchaseOrderDetailAsync(int orderId);
    // 🟢 6️⃣ Tìm kiếm hóa đơn theo từ khóa (tên học sinh, mã hóa đơn)
    Task<List<InvoiceDto>> SearchInvoicesAsync(Guid schoolId, string? keyword);

    // 🟡 7️⃣ Lọc hóa đơn theo trạng thái thanh toán
    Task<List<InvoiceDto>> FilterInvoicesByStatusAsync(Guid schoolId, string status);
    Task<byte[]> ExportFinanceReportAsync(Guid schoolId, int month, int year, bool isYearly = false);
    Task<byte[]> ExportPurchaseReportAsync(Guid schoolId, int month, int year, bool isYearly = false);

}
