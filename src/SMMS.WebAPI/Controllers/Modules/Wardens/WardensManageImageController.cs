using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.Wardens.Commands;
using SMMS.Application.Features.Wardens.DTOs;
using SMMS.Application.Features.Wardens.Interfaces;
using SMMS.Application.Features.Wardens.Queries;
using SMMS.Domain.Entities.school;
using SMMS.Persistence.Dbcontext;

namespace SMMS.WebAPI.Controllers.Modules.Wardens;
[Route("api/[controller]")]
[ApiController]
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
    [HttpPost("upload-student-image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadStudentImage([FromForm] UploadStudentImageRequest request)
    {
        if (request.File == null || request.File.Length == 0)
            return BadRequest(new { message = "Vui lòng chọn ảnh để upload." });

        try
        {
            // 🔹 Kiểm tra tồn tại học sinh & người upload
            var studentExists = await _context.Students
                .AnyAsync(s => s.StudentId == request.StudentId);
            var uploaderExists = await _context.Users
                .AnyAsync(u => u.UserId == request.UploaderId);

            if (!studentExists)
                return BadRequest(new { message = "Không tìm thấy học sinh trong hệ thống." });

            if (!uploaderExists)
                return BadRequest(new { message = "Người tải lên không tồn tại trong hệ thống." });

            // 🔹 Kiểm tra định dạng file
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var ext = Path.GetExtension(request.File.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
                return BadRequest(new { message = "Chỉ hỗ trợ các định dạng: .jpg, .jpeg, .png, .gif, .webp" });

            // 1️⃣ Gửi command upload ảnh (tự xử lý Cloudinary + folder)
            var uploadResult = await _mediator.Send(
                new UploadStudentImageCommand(request)
            );

            if (string.IsNullOrWhiteSpace(uploadResult.Url))
                return StatusCode(500, new { message = "Upload ảnh thất bại." });

            // 2️⃣ Lưu metadata vào DB
            var entity = new StudentImage
            {
                ImageId = Guid.NewGuid(),
                StudentId = request.StudentId,
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

    // 🟡 Lấy ảnh theo lớp (Cloudinary)
    [HttpGet("class/{classId:guid}/images")]
    public async Task<IActionResult> GetImagesByClass(Guid classId)
    {
        if (classId == Guid.Empty)
            return BadRequest(new { message = "Thiếu mã lớp (classId)." });

        try
        {
            var images = await _mediator.Send(new GetImagesByClassQuery(classId));

            if (images == null || !images.Any())
                return Ok(new
                {
                    message = "Không có ảnh nào trong lớp này.",
                    count = 0,
                    data = new List<object>()
                });

            return Ok(new
            {
                message = "Lấy danh sách ảnh thành công.",
                count = images.Count,
                data = images.Select(x => new
                {
                    url = x.Url,
                    publicId = x.PublicId,
                    createdAt = x.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                })
            });
        }
        catch (InvalidOperationException invEx)
        {
            return NotFound(new { message = invEx.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Lỗi khi lấy ảnh lớp: {ex.Message}" });
        }
    }

    // 🟣 Lấy ảnh của một học sinh (từ DB metadata)
    [HttpGet("student/{studentId:guid}")]
    public async Task<IActionResult> GetStudentImages(Guid studentId)
    {
        var exists = await _context.Students.AnyAsync(s => s.StudentId == studentId);
        if (!exists)
            return NotFound(new { message = "Không tìm thấy học sinh." });

        var images = await _context.StudentImages
            .Where(i => i.StudentId == studentId)
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new
            {
                i.ImageId,
                i.ImageUrl,
                i.Caption,
                i.CreatedAt
            })
            .ToListAsync();

        if (images.Count == 0)
            return NotFound(new { message = "Học sinh này chưa có ảnh nào được upload." });

        return Ok(new
        {
            message = $"Tìm thấy {images.Count} ảnh cho học sinh {studentId}.",
            data = images
        });
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
