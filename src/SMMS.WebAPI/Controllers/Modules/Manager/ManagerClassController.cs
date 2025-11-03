using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.Manager.DTOs;
using SMMS.Application.Features.Manager.Interfaces;

namespace SMMS.WebAPI.Controllers.Modules.Manager;
[Route("api/[controller]")]
[ApiController]
public class ManagerClassController : ControllerBase
{
    private readonly IManagerClassService _service;

    public ManagerClassController(IManagerClassService service)
    {
        _service = service;
    }

    // 🟢 GET: /api/ManagerClass?schoolId={id}
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid schoolId)
    {
        var result = await _service.GetAllAsync(schoolId);
        return Ok(new { count = result.Count, data = result });
    }

    // 🟡 POST: /api/ManagerClass
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClassRequest request)
    {
        var result = await _service.CreateAsync(request);
        return Ok(result);
    }

    // 🟠 PUT: /api/ManagerClass/{id}
    [HttpPut("{classId}")]
    public async Task<IActionResult> Update(Guid classId, [FromBody] UpdateClassRequest request)
    {
        var result = await _service.UpdateAsync(classId, request);
        if (result == null) return NotFound();
        return Ok(result);
    }

    // 🔴 DELETE: /api/ManagerClass/{id}
    [HttpDelete("{classId}")]
    public async Task<IActionResult> Delete(Guid classId)
    {
        var success = await _service.DeleteAsync(classId);
        if (!success) return NotFound();
        return Ok(new { message = "Đã xóa lớp học thành công." });
    }
}

