    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using SMMS.Application.Features.Manager.DTOs;
    using SMMS.Application.Features.Manager.Queries;

    namespace SMMS.WebAPI.Controllers.Modules.Manager;
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Manager")]
    public class ManagerInvoiceController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ManagerInvoiceController(IMediator mediator)
        {
            _mediator = mediator;
        }

        private Guid GetSchoolIdFromToken()
        {
            var schoolIdClaim = User.FindFirst("SchoolId")?.Value;
            if (string.IsNullOrEmpty(schoolIdClaim))
                throw new UnauthorizedAccessException("Không tìm thấy SchoolId trong token.");

            return Guid.Parse(schoolIdClaim);
        }

        // 🔍 Lấy danh sách invoice theo trường (filter tháng/năm/trạng thái)
        // GET api/ManagerInvoice?monthNo=1&year=2026&status=Unpaid
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] short? monthNo,
            [FromQuery] int? year,
            [FromQuery] string? status,
            CancellationToken ct)
        {
            var schoolId = GetSchoolIdFromToken();

            var result = await _mediator.Send(
                new GetSchoolInvoicesQuery(schoolId, monthNo, year, status),
                ct);

            return Ok(new
            {
                count = result.Count,
                data = result
            });
        }

        // 🔎 Lấy chi tiết 1 invoice (scope theo trường)
        // GET api/ManagerInvoice/123
        [HttpGet("{invoiceId:long}")]
        public async Task<IActionResult> GetById(long invoiceId, CancellationToken ct)
        {
            var schoolId = GetSchoolIdFromToken();

            var invoice = await _mediator.Send(
                new GetSchoolInvoiceByIdQuery(schoolId, invoiceId),
                ct);

            if (invoice == null)
                return NotFound(new { message = "Không tìm thấy hóa đơn." });

            return Ok(invoice);
        }

        // 🟡 Generate invoice cho TOÀN BỘ học sinh của 1 trường trong khoảng ngày
        // POST api/ManagerInvoice/generate
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateForSchool(
            [FromBody] GenerateSchoolInvoicesRequest request,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var schoolId = GetSchoolIdFromToken();

                var invoices = await _mediator.Send(
                    new GenerateSchoolInvoicesCommand(schoolId, request),
                    ct);

                if (invoices.Count == 0)
                {
                    return Ok(new
                    {
                        message = "Không có hóa đơn nào được tạo (có thể tất cả đã tồn tại hoặc không có học sinh active trong khoảng ngày này).",
                        data = invoices
                    });
                }

                // MonthNo đang được handler set theo tháng của DateFrom
                var monthNo = request.DateFrom.Month;

                return Ok(new
                {
                    message = $"Đã tạo {invoices.Count} hóa đơn cho toàn trường trong khoảng ngày {request.DateFrom:yyyy-MM-dd} - {request.DateTo:yyyy-MM-dd} (tháng {monthNo}).",
                    data = invoices
                });
            }
            catch (ArgumentException ex)          // validate lỗi (ngày, tháng, năm…)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)   // lỗi nghiệp vụ (trùng khoảng…)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }


        // 🟠 Cập nhật 1 invoice
        // PUT api/ManagerInvoice/{invoiceId}
        [HttpPut("{invoiceId:long}")]
        public async Task<IActionResult> Update(
            long invoiceId,
            [FromBody] UpdateInvoiceRequest request,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var schoolId = GetSchoolIdFromToken();

                var updated = await _mediator.Send(
                    new UpdateInvoiceCommand(schoolId, invoiceId, request),
                    ct);

                if (updated == null)
                {
                    return NotFound(new
                    {
                        message = "Không tìm thấy hóa đơn hoặc hóa đơn không thuộc trường này."
                    });
                }

                return Ok(new
                {
                    message = $"Cập nhật hóa đơn thành công! (khoảng {updated.DateFrom:yyyy-MM-dd} - {updated.DateTo:yyyy-MM-dd}, tháng {updated.MonthNo}).",
                    data = updated
                });
            }
            catch (ArgumentException ex)          // lỗi validate ngày / tháng
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)  // lỗi overlap, nghiệp vụ
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }


        // 🔴 Xóa 1 invoice (scope theo trường)
        // DELETE api/ManagerInvoice/123
        [HttpDelete("{invoiceId:long}")]
        public async Task<IActionResult> Delete(long invoiceId, CancellationToken ct)
        {
            var schoolId = GetSchoolIdFromToken();

            var success = await _mediator.Send(
                new DeleteInvoiceCommand(schoolId, invoiceId),
                ct);

            if (!success)
                return NotFound(new { message = "Không tìm thấy hóa đơn hoặc không thuộc trường này." });

            return Ok(new { message = "Xóa hóa đơn thành công!" });
        }
    }
