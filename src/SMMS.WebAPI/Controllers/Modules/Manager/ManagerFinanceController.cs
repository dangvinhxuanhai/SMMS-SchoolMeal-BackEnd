using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.Manager.Interfaces;

namespace SMMS.WebAPI.Controllers.Modules.Manager;
[Route("api/[controller]")]
[ApiController]
public class ManagerFinanceController : ControllerBase
{
    private readonly IManagerFinanceService _service;

    public ManagerFinanceController(IManagerFinanceService service)
    {
        _service = service;
    }
    // 🔍 Search invoices by keyword
    [HttpGet("invoices/search")]
    public async Task<IActionResult> SearchInvoices([FromQuery] Guid schoolId, [FromQuery] string? keyword)
    {
        try
        {
            var result = await _service.SearchInvoicesAsync(schoolId, keyword);
            return Ok(new { count = result.Count, data = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Lỗi khi tìm kiếm hóa đơn: {ex.Message}" });
        }
    }

    // 🎯 Filter invoices by payment status
    [HttpGet("invoices/filter")]
    public async Task<IActionResult> FilterInvoices([FromQuery] Guid schoolId, [FromQuery] string status)
    {
        try
        {
            var result = await _service.FilterInvoicesByStatusAsync(schoolId, status);
            return Ok(new { count = result.Count, data = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Lỗi khi lọc hóa đơn: {ex.Message}" });
        }
    }
    // GET: /api/ManagerFinance/summary?schoolId=xxx&month=11&year=2025
    [HttpGet("summary")]
    public async Task<IActionResult> GetFinanceSummary([FromQuery] Guid schoolId, [FromQuery] int month, [FromQuery] int year)
    {
        try
        {
            var result = await _service.GetFinanceSummaryAsync(schoolId, month, year);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Lỗi khi lấy dữ liệu tài chính: {ex.Message}" });
        }
    }
    // 🟡 2️⃣ Danh sách hóa đơn của trường
    // GET: /api/ManagerFinance/invoices?schoolId=xxx
    [HttpGet("invoices")]
    public async Task<IActionResult> GetInvoices([FromQuery] Guid schoolId)
    {
        try
        {
            var result = await _service.GetInvoicesAsync(schoolId);
            if (result == null || !result.Any())
                return NotFound(new { message = "Không có hóa đơn nào được tìm thấy." });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Lỗi khi lấy danh sách hóa đơn: {ex.Message}" });
        }
    }

    // 🟠 3️⃣ Chi tiết hóa đơn
    // GET: /api/ManagerFinance/invoices/{invoiceId}
    [HttpGet("invoices/{invoiceId:long}")]
    public async Task<IActionResult> GetInvoiceDetail(long invoiceId)
    {
        try
        {
            var result = await _service.GetInvoiceDetailAsync(invoiceId);
            if (result == null)
                return NotFound(new { message = "Không tìm thấy hóa đơn này." });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Lỗi khi lấy chi tiết hóa đơn: {ex.Message}" });
        }
    }

    // 🔵 4️⃣ Danh sách đơn hàng mua sắm trong tháng
    // GET: /api/ManagerFinance/purchase-orders?schoolId=xxx&month=11&year=2025
    [HttpGet("purchase-orders")]
    public async Task<IActionResult> GetPurchaseOrders([FromQuery] Guid schoolId, [FromQuery] int month, [FromQuery] int year)
    {
        try
        {
            var result = await _service.GetPurchaseOrdersByMonthAsync(schoolId, month, year);
            if (result == null || !result.Any())
                return NotFound(new { message = "Không có đơn hàng nào trong tháng này." });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Lỗi khi lấy danh sách đơn hàng: {ex.Message}" });
        }
    }

    // 🔴 5️⃣ Chi tiết đơn hàng
    // GET: /api/ManagerFinance/purchase-orders/{orderId}
    [HttpGet("purchase-orders/{orderId}")]
    public async Task<IActionResult> GetPurchaseOrderDetail(int orderId)
    {
        try
        {
            var result = await _service.GetPurchaseOrderDetailAsync(orderId);
            if (result == null)
                return NotFound(new { message = "Không tìm thấy đơn hàng này." });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Lỗi khi lấy chi tiết đơn hàng: {ex.Message}" });
        }
    }
}
