using MediatR;
using Microsoft.AspNetCore.Authorization; // Thêm cái này để check user
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.nutrition.Commands;
using SMMS.Application.Features.nutrition.DTOs;
using SMMS.Application.Features.nutrition.Queries;
using System.Security.Claims; // Để lấy Claim

namespace SMMS.WebAPI.Controllers.Modules.KitchenStaff;

[ApiController]
[Route("api/nutrition/[controller]")]
[Authorize] // Bắt buộc phải đăng nhập mới dùng được các API này
public class FoodItemsController : ControllerBase
{
    private readonly IMediator _mediator;

    public FoodItemsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ... (Các hàm GET giữ nguyên như code của bạn) ...
    // GET api/nutrition/fooditems?schoolId=...&keyword=...
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FoodItemDto>>> GetList(
        [FromQuery] Guid schoolId,
        [FromQuery] string? keyword)
    {
        var result = await _mediator.Send(new GetFoodItemsQuery(schoolId, keyword));
        return Ok(result);
    }

    // GET api/nutrition/fooditems/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<FoodItemDto>> GetById(int id)
    {
        var result = await _mediator.Send(new GetFoodItemByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    // ==========================================
    // XỬ LÝ CREATE (POST)
    // ==========================================
    [HttpPost]
    public async Task<ActionResult<FoodItemDto>> Create([FromBody] CreateFoodItemCommand command)
    {
        // 1. Lấy User ID từ Token (Claim Types.NameIdentifier hoặc "sub")
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdString, out var userId))
        {
            command.CreatedBy = userId;
        }

        // 2. Lấy School ID từ Token (giả sử bạn lưu schoolId trong claim custom tên "school_id")
        // Nếu API này dành cho Admin trường thì SchoolId phải lấy từ token để bảo mật,
        // không cho phép họ truyền bừa SchoolId của trường khác.
        var schoolIdString = User.FindFirst("school_id")?.Value;
        if (Guid.TryParse(schoolIdString, out var schoolId))
        {
            command.SchoolId = schoolId;
        }

        // Nếu không lấy từ token mà tin tưởng client truyền lên thì giữ nguyên dòng dưới:
        // (Nếu client truyền command.SchoolId = null hoặc Guid.Empty thì cần validate)

        var created = await _mediator.Send(command);

        // Trả về 201 Created kèm Header Location trỏ về API GetById
        return CreatedAtAction(nameof(GetById), new { id = created.FoodId }, created);
    }

    // ==========================================
    // XỬ LÝ UPDATE (PUT)
    // ==========================================
    [HttpPut("{id:int}")]
    public async Task<ActionResult<FoodItemDto>> Update(
        int id,
        [FromBody] UpdateFoodItemCommand command)
    {
        if (id != command.FoodId)
        {
            // Gán lại cho chắc chắn hoặc báo lỗi
            return BadRequest("ID trong URL và Body không khớp");
        }

        try
        {
            var updated = await _mediator.Send(command);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Không tìm thấy món ăn với ID {id}");
        }
    }

    // ==========================================
    // XỬ LÝ DELETE (DELETE)
    // ==========================================
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        [FromQuery] bool hardDeleteIfNoRelation = false)
    {
        await _mediator.Send(new DeleteFoodItemCommand
        {
            FoodId = id,
            HardDeleteIfNoRelation = hardDeleteIfNoRelation
        });

        // Trả về 204 No Content (Chuẩn RESTful cho Delete)
        return NoContent();
    }
}
