using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.Manager.Interfaces;
using SMMS.Application.Features.Manager.Queries;

namespace SMMS.WebAPI.Controllers.Modules.Manager;
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Manager")]
public class ManagerFinanceController : ControllerBase
{
    private readonly IMediator _mediator;

    public ManagerFinanceController(IMediator mediator)
    {
        _mediator = mediator;
    }
    // get school ID
    private Guid GetSchoolIdFromToken()
    {
        var schoolIdClaim = User.FindFirst("SchoolId")?.Value;

        if (string.IsNullOrEmpty(schoolIdClaim))
            throw new UnauthorizedAccessException("Không tìm thấy SchoolId trong token.");

        return Guid.Parse(schoolIdClaim);
    }
    // 🔍 Search invoices by keyword
    // GET: /api/ManagerFinance/invoices/search?schoolId=...&keyword=...
    [HttpGet("invoices/search")]
    public async Task<IActionResult> SearchInvoices( [FromQuery] string? keyword)
    {
        try
        {
            var schoolId = GetSchoolIdFromToken();
            var result = await _mediator.Send(new SearchInvoicesQuery(schoolId, keyword));
            return Ok(new { count = result.Count, data = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Lỗi khi tìm kiếm hóa đơn: {ex.Message}" });
        }
    }

    // 🎯 Filter invoices by payment status
    // GET: /api/ManagerFinance/invoices/filter?schoolId=...&status=...
    [HttpGet("invoices/filter")]
    public async Task<IActionResult> FilterInvoices([FromQuery] string status)
    {
        try
        {
            var schoolId = GetSchoolIdFromToken();
            var result = await _mediator.Send(new FilterInvoicesByStatusQuery(schoolId, status));
            return Ok(new { count = result.Count, data = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Lỗi khi lọc hóa đơn: {ex.Message}" });
        }
    }

    // 📊 Tổng quan tài chính
    // GET: /api/ManagerFinance/summary?schoolId=xxx&month=11&year=2025
    [HttpGet("summary")]
    public async Task<IActionResult> GetFinanceSummary([FromQuery] int year)
    {
        try
        {
            var schoolId = GetSchoolIdFromToken();
            var result = await _mediator.Send(new GetFinanceSummaryQuery(schoolId, year));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Lỗi khi lấy dữ liệu tài chính: {ex.Message}" });
        }
    }

    // 🟡 Danh sách hóa đơn của trường
    // GET: /api/ManagerFinance/invoices?schoolId=xxx
    [HttpGet("invoices")]
    public async Task<IActionResult> GetInvoices()
    {
        try
        {
            var schoolId = GetSchoolIdFromToken();
            var result = await _mediator.Send(new GetInvoicesQuery(schoolId));
            if (result == null || !result.Any())
                return NotFound(new { message = "Không có hóa đơn nào được tìm thấy." });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Lỗi khi lấy danh sách hóa đơn: {ex.Message}" });
        }
    }

    // 🟠 Chi tiết 1 hóa đơn
    // GET: /api/ManagerFinance/invoices/{invoiceId}
    [HttpGet("invoices/{invoiceId:long}")]
    public async Task<IActionResult> GetInvoiceDetail(long invoiceId)
    {
        try
        {
            var result = await _mediator.Send(new GetInvoiceDetailQuery(invoiceId));
            if (result == null)
                return NotFound(new { message = "Không tìm thấy hóa đơn này." });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Lỗi khi lấy chi tiết hóa đơn: {ex.Message}" });
        }
    }

    // 🔵 Danh sách đơn hàng mua sắm trong tháng
    // GET: /api/ManagerFinance/purchase-orders?schoolId=xxx&month=11&year=2025
    [HttpGet("purchase-orders")]
    public async Task<IActionResult> GetPurchaseOrders([FromQuery] int month, [FromQuery] int year)
    {
        try
        {
            var schoolId = GetSchoolIdFromToken();
            var result = await _mediator.Send(new GetPurchaseOrdersByMonthQuery(schoolId, month, year));
            if (result == null || !result.Any())
                return NotFound(new { message = "Không có đơn hàng nào trong tháng này." });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Lỗi khi lấy danh sách đơn hàng: {ex.Message}" });
        }
    }

    // 🔴 Chi tiết đơn hàng
    // GET: /api/ManagerFinance/purchase-orders/{orderId}
    [HttpGet("purchase-orders/{orderId:int}")]
    public async Task<IActionResult> GetPurchaseOrderDetail(int orderId)
    {
        try
        {
            var result = await _mediator.Send(new GetPurchaseOrderDetailQuery(orderId));
            if (result == null)
                return NotFound(new { message = "Không tìm thấy đơn hàng này." });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Lỗi khi lấy chi tiết đơn hàng: {ex.Message}" });
        }
    }

    // 🟡 Xuất báo cáo tài chính tháng/năm
    // GET: /api/ManagerFinance/export?schoolId=...&month=...&year=...&isYearly=true/false
    [HttpGet("export")]
    public async Task<IActionResult> ExportFinanceReport(
        [FromQuery] int month,
        [FromQuery] int year,
        [FromQuery] bool isYearly = false)
    {
        try
        {
            var schoolId = GetSchoolIdFromToken();
            var fileBytes = await _mediator.Send(
                new ExportFinanceReportQuery(schoolId, month, year, isYearly));

            string fileName = isYearly
                ? $"BaoCaoTaiChinh_Nam_{year}.xlsx"
                : $"BaoCaoTaiChinh_Thang_{month}_{year}.xlsx";

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Lỗi khi xuất báo cáo: {ex.Message}" });
        }
    }

    // 🛒 Xuất báo cáo chi phí đi chợ
    // GET: /api/ManagerFinance/export-purchase?schoolId=...&month=...&year=...&isYearly=true/false
    [HttpGet("export-purchase")]
    public async Task<IActionResult> ExportPurchase(
        [FromQuery] int month,
        [FromQuery] int year,
        [FromQuery] bool isYearly = false)
    {
        try
        {
            var schoolId = GetSchoolIdFromToken();
            var fileBytes = await _mediator.Send(
                new ExportPurchaseReportQuery(schoolId, month, year, isYearly));

            var fileName = $"BaoCaoChiPhiDiCho_{(isYearly ? $"Nam_{year}" : $"Thang_{month}_{year}")}.xlsx";

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Lỗi khi xuất báo cáo chi phí đi chợ: {ex.Message}" });
        }
    }
}
