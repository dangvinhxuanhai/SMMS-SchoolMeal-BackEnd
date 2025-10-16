using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.Wardens.DTOs;
using SMMS.Application.Features.Wardens.Interfaces;
using SMMS.Domain.Entities.school;
using SMMS.Persistence.Dbcontext;

namespace SMMS.WebAPI.Controllers;
[Route("api/[controller]")]
[ApiController]
public class WardensManageImageController : ControllerBase
{
    private readonly EduMealContext _context;
    private readonly ICloudStorageService _cloudService;

    public WardensManageImageController(EduMealContext context, ICloudStorageService cloudService)
    {
        _context = context;
        _cloudService = cloudService;
    }
    [HttpPost("upload-student-image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadStudentImage([FromForm] UploadStudentImageRequest request)
    {
        if (request.File == null || request.File.Length == 0)
            return BadRequest(new { message = "Vui lòng chọn ảnh để upload." });

        try
        {
            // 🔹 Kiểm tra tồn tại học sinh & người upload
            var studentExists = await _context.Students.AnyAsync(s => s.StudentId == request.StudentId);
            var uploaderExists = await _context.Users.AnyAsync(u => u.UserId == request.UploaderId);

            if (!studentExists)
                return BadRequest(new { message = "Không tìm thấy học sinh trong hệ thống." });
            if (!uploaderExists)
                return BadRequest(new { message = "Người tải lên không tồn tại trong hệ thống." });

            // 🔹 Kiểm tra định dạng file
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var ext = Path.GetExtension(request.File.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
                return BadRequest(new { message = "Chỉ hỗ trợ các định dạng: .jpg, .jpeg, .png, .gif, .webp" });

            // 1️⃣ Upload ảnh lên Cloudinary
            var uploadResult = await _cloudService.UploadImageAsync(request.File, "student_images");
            if (string.IsNullOrWhiteSpace(uploadResult.Url))
                return BadRequest(new { message = "Upload ảnh thất bại." });

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
    // 🟡 API 2: Lấy tất cả ảnh từ Cloudinary (hoặc trong 1 folder)
    [HttpGet("images")]
    public async Task<IActionResult> GetAllImages([FromQuery] string? folder = "student_images", [FromQuery] int maxResults = 100)
    {
        try
        {
            var images = await _cloudService.GetAllImagesAsync(folder, maxResults);
            if (images == null || images.Count == 0)
                return NotFound(new { message = "Không tìm thấy ảnh nào trong Cloudinary." });

            return Ok(new
            {
                message = $"Tìm thấy {images.Count} ảnh.",
                data = images.Select(img => new
                {
                    img.Url,
                    img.PublicId,
                    img.CreatedAt
                })
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Lỗi khi lấy danh sách ảnh: {ex.Message}" });
        }
    }

    // 🟣 API 3: Lấy ảnh của một học sinh cụ thể
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
    // 🗑️ API 4: Xóa ảnh theo ImageId (xóa cả Cloudinary và DB)
    [HttpDelete("{imageId:guid}")]
    public async Task<IActionResult> DeleteImage(Guid imageId)
    {
        try
        {
            var image = await _context.StudentImages.FirstOrDefaultAsync(i => i.ImageId == imageId);
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

            var deleted = await _cloudService.DeleteImageAsync(publicId);
            if (!deleted)
                return StatusCode(500, new { message = $"Không thể xóa ảnh khỏi Cloudinary (publicId={publicId})." });

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
