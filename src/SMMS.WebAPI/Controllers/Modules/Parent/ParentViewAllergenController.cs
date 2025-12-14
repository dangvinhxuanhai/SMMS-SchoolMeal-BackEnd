using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    }
}
