using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.Wardens.Commands;
using SMMS.Application.Features.Wardens.DTOs;
using SMMS.Application.Features.Wardens.Interfaces;
using SMMS.Application.Features.Wardens.Queries;
using SMMS.Domain.Entities.school;
using SMMS.Persistence.Data;

namespace SMMS.WebAPI.Controllers.Modules.Wardens;
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Teacher")]
public class WardensManageImageController : ControllerBase
{
    private readonly EduMealContext _context;
    private readonly IMediator _mediator;

    public WardensManageImageController(EduMealContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    // 🟢 Upload ảnh học sinh
    // 🟢 Upload ảnh học sinh cho 1 lớp (tự chọn student đầu tiên trong lớp)
    [HttpPost("upload-student-image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadStudentImage([FromForm] UploadStudentImageRequest request)
    {
        if (request.File == null || request.File.Length == 0)
            return BadRequest(new { message = "Vui lòng chọn ảnh để upload." });

        if (request.ClassId == Guid.Empty)
            return BadRequest(new { message = "ClassId không hợp lệ." });

        try
        {
            var userIdString = User.FindFirst("UserId")?.Value
                               ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var currentUserId))
            {
                return Unauthorized(new { message = "Không xác định được người dùng." });
            }

            request.UploaderId = currentUserId;

            // 🔹 Kiểm tra định dạng file (OPTIONAL, trùng với handler nhưng giúp báo lỗi sớm)
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var ext = Path.GetExtension(request.File.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
                return BadRequest(new { message = "Chỉ hỗ trợ các định dạng: .jpg, .jpeg, .png, .gif, .webp" });

            // 🔹 Lấy học sinh đầu tiên của lớp (đã đăng ký)
            var studentId = await _context.StudentClasses
                .Where(sc => sc.ClassId == request.ClassId && sc.RegistStatus == true)
                .OrderBy(sc => sc.JoinedDate)
                .Select(sc => sc.StudentId)
                .FirstOrDefaultAsync();

            if (studentId == Guid.Empty)
                return BadRequest(new { message = "Lớp này chưa có học sinh nào đăng ký." });

            // 1️⃣ Gửi command upload ảnh (handler tự dùng ClassId để build folder Cloudinary)
            var uploadResult = await _mediator.Send(
                new UploadStudentImageCommand(request) // BaseFolder dùng default "student_images"
            );

            if (string.IsNullOrWhiteSpace(uploadResult.Url))
                return StatusCode(500, new { message = "Upload ảnh thất bại." });

            // 2️⃣ Lưu metadata vào DB (gắn với student đầu tiên của lớp)
            var entity = new StudentImage
            {
                ImageId = Guid.NewGuid(),
                StudentId = studentId,
                UploadedBy = request.UploaderId,
                ImageUrl = uploadResult.Url,
                Caption = request.Caption ?? Path.GetFileNameWithoutExtension(request.File.FileName),
                TakenAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.StudentImages.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Upload ảnh thành công!",
                data = new
                {
                    entity.ImageId,
                    entity.StudentId,
                    entity.ImageUrl,
                    entity.Caption,
                    entity.CreatedAt
                }
            });
        }
        catch (DbUpdateException dbEx)
        {
            var inner = dbEx.InnerException?.Message ?? dbEx.Message;
            return StatusCode(500, new { message = $"Lỗi khi ghi vào DB: {inner}" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Lỗi khi upload ảnh: {ex.Message}" });
        }
    }

    // 🟡 Lấy tất cả ảnh theo lớp (Cloudinary folder)
    [HttpGet("class/{classId:guid}/images")]
    public async Task<IActionResult> GetImagesByClass(Guid classId, [FromQuery] int maxResults = 100)
    {
        if (classId == Guid.Empty)
            return BadRequest(new { message = "ClassId không hợp lệ." });

        try
        {
            // kiểm tra lớp có tồn tại không
            var exists = await _context.Classes.AnyAsync(c => c.ClassId == classId);
            if (!exists)
                return NotFound(new { message = "Không tìm thấy lớp học." });

            // Gửi query → Handler tự lấy SchoolName / YearName / ClassName và scan folder Cloudinary
            var images = await _mediator.Send(
                new GetImagesByClassQuery(classId, maxResults)
            );

            return Ok(new
            {
                message = $"Tìm thấy {images.Count} ảnh cho lớp {classId}.",
                data = images
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = $"Lỗi khi lấy ảnh theo lớp: {ex.Message}"
            });
        }
    }

    // 🗑️ Xóa ảnh theo ImageId (xóa Cloudinary + DB)
    [HttpDelete("{imageId:guid}")]
    public async Task<IActionResult> DeleteImage(Guid imageId)
    {
        try
        {
            var image = await _context.StudentImages
                .FirstOrDefaultAsync(i => i.ImageId == imageId);

            if (image == null)
                return NotFound(new { message = "Không tìm thấy ảnh trong hệ thống." });

            string? publicId = null;

            try
            {
                var uri = new Uri(image.ImageUrl);
                var parts = uri.AbsolutePath.Split('/');
                var uploadIndex = Array.IndexOf(parts, "upload");

                if (uploadIndex >= 0 && uploadIndex + 2 < parts.Length)
                {
                    publicId = string.Join('/', parts.Skip(uploadIndex + 2))
                        .Replace(Path.GetExtension(image.ImageUrl), "");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Không thể phân tích URL ảnh: {ex.Message}" });
            }

            if (string.IsNullOrEmpty(publicId))
                return BadRequest(new { message = "Không thể xác định publicId từ URL Cloudinary." });

            // 🔻 Gửi command xóa ảnh trên Cloudinary
            var deleted = await _mediator.Send(new DeleteImageCommand(publicId));
            if (!deleted)
                return StatusCode(500, new { message = $"Không thể xóa ảnh khỏi Cloudinary (publicId={publicId})." });

            // 🔻 Xóa metadata trong DB
            _context.StudentImages.Remove(image);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa ảnh thành công!", image.ImageUrl, image.Caption });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Lỗi khi xóa ảnh: {ex.Message}" });
        }
    }
}
