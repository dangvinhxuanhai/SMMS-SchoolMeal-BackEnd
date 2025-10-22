using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.billing.Interfaces;
using System.Security.Claims;

namespace SMMS.WebAPI.Controllers.Modules.Parent
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public InvoiceController(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        // ✅ API xem danh sách hóa đơn của các con
        [HttpGet("my-invoices")]
        public async Task<IActionResult> GetInvoices()
        {
            var parentId = GetCurrentUserId();
            var invoices = await _invoiceRepository.GetInvoicesByParentAsync(parentId);

            if (!invoices.Any())
                return NotFound("Không có hóa đơn nào.");

            return Ok(invoices);
        }

        // ✅ API xem chi tiết hóa đơn (1 con)
        [HttpGet("{invoiceId:long}")]
        public async Task<IActionResult> GetInvoiceDetail(long invoiceId)
        {
            var parentId = GetCurrentUserId();
            var invoice = await _invoiceRepository.GetInvoiceDetailAsync(invoiceId, parentId);

            if (invoice == null)
                return NotFound("Không tìm thấy hóa đơn hoặc bạn không có quyền truy cập.");

            return Ok(invoice);
        }

        // ✅ Lấy ParentId từ JWT token
        private Guid GetCurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(idClaim, out var parentId))
                throw new UnauthorizedAccessException("Token không hợp lệ.");
            return parentId;
        }
    }
}
