using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SMMS.Application.Features.Wardens.Interfaces;
using SMMS.Persistence.Configurations;
using SMMS.Persistence.Dbcontext; // ✅ thêm using cho DbContext

namespace SMMS.Persistence.Repositories.Wardens
{
    public class CloudStorageService : ICloudStorageService
    {
        private readonly Cloudinary _cloudinary;
        private readonly CloudinarySettings _settings;
        private readonly EduMealContext _context; // ✅ thêm context

        public CloudStorageService(
            IOptions<CloudinarySettings> options,
            EduMealContext context) // ✅ inject DbContext
        {
            _settings = options.Value;
            _context = context;

            var account = new Account(
                _settings.CloudName,
                _settings.ApiKey,
                _settings.ApiSecret
            );

            _cloudinary = new Cloudinary(account);
        }

        // 🟡 Lấy danh sách toàn bộ ảnh
        public async Task<List<(string Url, string PublicId, DateTime CreatedAt)>> GetAllImagesAsync(
            string? folder = null, int maxResults = 100)
        {
            var listParams = new ListResourcesParams
            {
                Type = "upload",
                ResourceType = ResourceType.Image,
                MaxResults = maxResults
            };

            var result = await _cloudinary.ListResourcesAsync(listParams);

            if (result.StatusCode != HttpStatusCode.OK)
                throw new Exception($"Cloudinary list failed: {result.Error?.Message}");

            var resources = result.Resources.AsEnumerable();

            // 🔹 Lọc theo folder nếu có
            if (!string.IsNullOrWhiteSpace(folder))
                resources = resources.Where(r => r.PublicId.StartsWith(folder.TrimEnd('/') + "/"));

            return resources
                .Select(r => (
                    Url: r.SecureUrl?.ToString() ?? string.Empty,
                    PublicId: r.PublicId,
                    CreatedAt: DateTime.TryParse(r.CreatedAt, out var parsed)
                        ? parsed
                        : DateTime.MinValue
                ))
                .ToList();
        }
        public async Task<List<(string Url, string PublicId, DateTime CreatedAt)>> GetImagesByClassAsync(Guid classId, int maxResults = 100)
        {
            // 🔹 Lấy thông tin lớp
            var classInfo = await (
                from c in _context.Classes
                join y in _context.AcademicYears on c.YearId equals y.YearId
                join sch in _context.Schools on c.SchoolId equals sch.SchoolId
                where c.ClassId == classId
                select new
                {
                    SchoolName = sch.SchoolName,
                    YearName = y.YearName,
                    ClassName = c.ClassName
                }
            ).FirstOrDefaultAsync();

            if (classInfo == null)
                throw new InvalidOperationException("Không tìm thấy lớp học.");

            string Normalize(string text)
            {
                if (string.IsNullOrWhiteSpace(text)) return "Unknown";
                text = text.Normalize(System.Text.NormalizationForm.FormD);
                var chars = text.Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
                                            != System.Globalization.UnicodeCategory.NonSpacingMark);
                return new string(chars.ToArray())
                    .Replace(" ", "_")
                    .Replace("/", "-")
                    .Replace("\\", "-")
                    .Replace(".", "")
                    .Trim();
            }

            var school = Normalize(classInfo.SchoolName);
            var year = Normalize(classInfo.YearName);
            var className = Normalize(classInfo.ClassName);

            var folderPath = $"student_images/{school}/{year}/{className}";

            // 🔹 Dùng lại hàm cũ để lấy ảnh trong folder (và giới hạn maxResults)
            return await GetAllImagesAsync(folderPath, maxResults);
        }

        // 🟢 Upload ảnh theo từng lớp/trường/năm
        public async Task<(string Url, string PublicId)> UploadImageAsync(
            IFormFile file,
            Guid studentId,
            string? baseFolder = "student_images")
        {
            if (file == null || file.Length == 0)
                throw new InvalidOperationException("Không có tệp hợp lệ để upload.");

            // 🔹 Kiểm tra định dạng ảnh hợp lệ
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
                throw new InvalidOperationException("Chỉ được phép upload các tệp hình ảnh (.jpg, .jpeg, .png, .gif, .webp)");

            // 🔹 Lấy thông tin học sinh, lớp, trường, năm học
            var studentInfo = await (
                from s in _context.Students
                join sc in _context.StudentClasses on s.StudentId equals sc.StudentId
                join c in _context.Classes on sc.ClassId equals c.ClassId
                join y in _context.AcademicYears on c.YearId equals y.YearId
                join sch in _context.Schools on c.SchoolId equals sch.SchoolId
                where s.StudentId == studentId
                select new
                {
                    SchoolName = sch.SchoolName,
                    YearName = y.YearName,
                    ClassName = c.ClassName
                }
            ).FirstOrDefaultAsync();

            // 🔹 Xử lý tên folder
            string school = studentInfo?.SchoolName ?? "Unknown_School";
            string year = studentInfo?.YearName ?? "Unknown_Year";
            string className = studentInfo?.ClassName ?? "Unknown_Class";

            string Normalize(string text)
            {
                if (string.IsNullOrWhiteSpace(text)) return "Unknown";
                text = text.Normalize(System.Text.NormalizationForm.FormD);
                var chars = text.Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark);
                return new string(chars.ToArray())
                    .Replace(" ", "_")
                    .Replace("/", "-")
                    .Replace("\\", "-")
                    .Replace(".", "")
                    .Trim();
            }

            school = Normalize(school);
            year = Normalize(year);
            className = Normalize(className);

            // 🔹 Folder final: ví dụ student_images/TruongA/Nam2025/Lop1A
            var folderPath = $"{baseFolder}/{school}/{year}/{className}";

            // 🔹 Upload ảnh lên Cloudinary
            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folderPath,
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.StatusCode != HttpStatusCode.OK)
                throw new Exception($"Cloudinary upload failed: {result.Error?.Message}");

            return (result.SecureUrl.ToString(), result.PublicId);
        }

        // 🧹 Xóa ảnh
        public async Task<bool> DeleteImageAsync(string publicId)
        {
            var deletionParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deletionParams);
            return result.Result == "ok";
        }
    }
}
