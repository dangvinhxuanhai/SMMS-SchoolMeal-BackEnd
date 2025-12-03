using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.school.Queries;

namespace SMMS.WebAPI.Controllers.Modules.Parent
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Parent")]
    public class ParentViewStudentImagesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ParentViewStudentImagesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetImagesByStudent(Guid studentId)
        {
            var result = await _mediator.Send(new GetImagesByStudentQuery(studentId));

            if (result == null || !result.Any())
                return NotFound("Không có ảnh nào cho học sinh này.");

            return Ok(result);
        }
    }
}
