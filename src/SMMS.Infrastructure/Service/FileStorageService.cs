using SMMS.Application.Features.Identity.Interfaces;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SMMS.Infrastructure.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;

        public FileStorageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> SaveFileAsync(string fileName, byte[] fileData, string folder, string newFileName)
        {
            if (fileData == null || fileData.Length == 0)
                throw new ArgumentException("Dữ liệu file không hợp lệ.");

            // ✅ Nếu WebRootPath null thì gán thủ công để tránh lỗi
            var webRootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            // ✅ Tạo đường dẫn lưu file
            var uploadsFolder = Path.Combine(webRootPath, "uploads", folder);

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, newFileName);

            // ✅ Ghi file
            await File.WriteAllBytesAsync(filePath, fileData);

            // ✅ Trả về URL tương đối (client có thể load qua /uploads)
            return $"/uploads/{folder}/{newFileName}";
        }

        public Task<bool> DeleteFileAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return Task.FromResult(false);

            var webRootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var fullPath = Path.Combine(webRootPath, filePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }
}
