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
        public async Task<IActionResult> GetInvoices()
        {
            var parentId = GetCurrentUserId();
            var invoices = await _mediator.Send(new GetInvoicesByParentQuery(parentId));

            if (invoices == null || !invoices.Any())
                return NotFound(new { message = "Không có hóa đơn nào." });

            return Ok(invoices);
        }

        // ✅ API: Chi tiết hóa đơn
        [HttpGet("{invoiceId:long}")]
        public async Task<IActionResult> GetInvoiceDetail(long invoiceId)
        {
            var parentId = GetCurrentUserId();
            var invoice = await _mediator.Send(new GetInvoiceDetailQuery(invoiceId, parentId));

            if (invoice == null)
                return NotFound(new { message = "Không tìm thấy hóa đơn hoặc bạn không có quyền truy cập." });

            return Ok(invoice);
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
