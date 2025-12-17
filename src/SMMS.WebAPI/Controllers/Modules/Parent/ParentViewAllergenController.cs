using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.nutrition.Commands;
using SMMS.Application.Features.nutrition.DTOs;
using SMMS.Application.Features.nutrition.Queries;

namespace SMMS.WebAPI.Controllers.Modules.Parent
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Parent")]
    public class ParentViewAllergenController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ParentViewAllergenController(IMediator mediator)
        {
            _mediator = mediator;
        }
        /// <summary>
        /// Lấy danh sách dị ứng theo học sinh
        /// </summary>
        [HttpGet("by-student/{studentId}")]
        public async Task<IActionResult> GetByStudent(Guid studentId)
        {
            var query = new GetAllAllergensByStudentQuery(studentId);
            var result = await _mediator.Send(query);

            return Ok(result);
        }
        [HttpGet("top")]
        public async Task<IActionResult> GetTopAllergens([FromQuery] Guid studentId, [FromQuery] int top = 5)
        {
            if (studentId == Guid.Empty)
                return BadRequest("StudentId is required.");

            try
            {
                var query = new GetTopAllergensQuery(studentId, top);
                var result = await _mediator.Send(query);

                if (result == null || result.Count == 0)
                    return NotFound("No allergens found for this student.");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("by-student/{studentId}")]
        public async Task<IActionResult> Create(
        Guid studentId,
        [FromBody] AddStudentAllergyDTO request)
        {
            // 1️⃣ Lấy userId từ token
            var userId = GetCurrentUserId();

            // 2️⃣ Map sang Command
            var command = new AddStudentAllergyCommand
            {
                StudentId = studentId,
                UserId = userId,
                AllergenId = request.AllergenId,
                AllergenName = request.AllergenName,
                AllergenInfo = request.AllergenInfo
            };

            // 3️⃣ Send MediatR
            await _mediator.Send(command);

            return Ok(new
            {
                message = "Add allergy successfully"
            });
        }
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                throw new UnauthorizedAccessException("Không tìm thấy ID người dùng trong token.");
            return Guid.Parse(userIdClaim.Value);
        }
    }
}
