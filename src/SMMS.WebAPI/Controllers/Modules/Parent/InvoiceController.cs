using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.billing.Interfaces;
using SMMS.Application.Features.billing.Queries;
using System.Security.Claims;

namespace SMMS.WebAPI.Controllers.Modules.Parent
{
    [Authorize(Roles = "Parent")]
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly IMediator _mediator;

        public InvoiceController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // ✅ API: Danh sách hóa đơn của phụ huynh
        [HttpGet("my-invoices")]
        public async Task<IActionResult> GetInvoices([FromQuery] Guid studentId)
        {
            if (studentId == Guid.Empty)
                return BadRequest(new { message = "Vui lòng nhập StudentId hợp lệ." });
            var invoices = await _mediator.Send(new GetInvoicesByParentQuery(studentId));
            if (invoices == null || !invoices.Any())
                return NotFound(new { message = "Không có hóa đơn nào cho học sinh này." });

            return Ok(invoices);
        }

        // ✅ API: Chi tiết hóa đơn
        [HttpGet("{invoiceId:long}")]
        public async Task<IActionResult> GetInvoiceDetail(long invoiceId, [FromQuery] Guid studentId)
        {
            if (studentId == Guid.Empty)
                return BadRequest(new { message = "Vui lòng nhập StudentId hợp lệ." });
            var invoice = await _mediator.Send(new GetInvoiceDetailQuery(invoiceId, studentId));

            if (invoice == null)
                return NotFound(new { message = "Không tìm thấy hóa đơn hoặc bạn không có quyền truy cập." });

            return Ok(invoice);
        }
        // ✅ API: Hóa đơn của phụ huynh chua thanh toan
        [HttpGet("invoices-unpaid")]
        public async Task<IActionResult> GetInvoicesUnpaid([FromQuery] Guid studentId)
        {
            if (studentId == Guid.Empty)
                return BadRequest(new { message = "Vui lòng nhập StudentId hợp lệ." });
            var invoices = await _mediator.Send(new GetUnpaidInvoicesQuery(studentId));

            if (invoices == null || !invoices.Any())
                return NotFound(new { message = "Không có hóa đơn nào chưa thanh toán." });

            return Ok(invoices);
        }

        // ✅ Helper: Lấy ParentId từ JWT token
        private Guid GetCurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(idClaim, out var parentId))
                throw new UnauthorizedAccessException("Token không hợp lệ.");
            return parentId;
        }
    }
}
