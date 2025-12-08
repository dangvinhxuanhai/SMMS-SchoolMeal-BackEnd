using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Manager.DTOs;

namespace SMMS.Application.Features.Manager.Queries;
// 1️⃣ Search invoice theo keyword
public record SearchInvoicesQuery(Guid SchoolId, string? Keyword)
    : IRequest<List<InvoiceDto>>;

// 2️⃣ Filter invoice theo status
public record FilterInvoicesByStatusQuery(Guid SchoolId, string Status)
    : IRequest<List<InvoiceDto>>;

// 3️⃣ Tổng hợp tài chính (invoice + purchase)
public record GetFinanceSummaryQuery(Guid SchoolId, int Year)
    : IRequest<FinanceSummaryDto>;

// 4️⃣ Danh sách hóa đơn của trường
public record GetInvoicesQuery(Guid SchoolId)
    : IRequest<List<InvoiceDto>>;

// 5️⃣ Chi tiết hóa đơn
public record GetInvoiceDetailQuery(long InvoiceId)
    : IRequest<InvoiceDetailDto?>;

// 6️⃣ Danh sách đơn hàng trong tháng
public record GetPurchaseOrdersByMonthQuery(Guid SchoolId, int Month, int Year)
    : IRequest<List<PurchaseOrderDto>>;

// 7️⃣ Chi tiết đơn hàng
public record GetPurchaseOrderDetailQuery(int OrderId)
    : IRequest<PurchaseOrderDetailDto?>;

// 8️⃣ Export báo cáo tài chính (Excel)
public record ExportFinanceReportQuery(Guid SchoolId, int Month, int Year, bool IsYearly = false)
    : IRequest<byte[]>;

// 9️⃣ Export báo cáo chi phí đi chợ (Excel)
public record ExportPurchaseReportQuery(Guid SchoolId, int Month, int Year, bool IsYearly = false)
    : IRequest<byte[]>;
