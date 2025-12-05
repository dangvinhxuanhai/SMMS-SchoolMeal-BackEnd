using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SMMS.Application.Features.Wardens.Interfaces;

namespace SMMS.Persistence
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IOptions<CloudinarySettings> config)
        {
            var acc = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );
            _cloudinary = new Cloudinary(acc);
        }

        // 1. Upload từ File (IFormFile) - Dùng cho Avatar, Ảnh món ăn từ máy tính
        public async Task<string?> UploadImageAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0) return null;

            using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folderName
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            return uploadResult.SecureUrl?.ToString();
        }

        // 2. (Tùy chọn) Upload từ URL (string) - Nếu input là đường dẫn ảnh online
        public async Task<string?> UploadImageFromUrlAsync(string url, string folderName)
        {
            if (string.IsNullOrEmpty(url)) return null;

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(url),
                Folder = folderName
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.SecureUrl?.ToString();
        }

        // 3. Hàm xóa ảnh (Cần thiết khi update ảnh mới thì xóa ảnh cũ)
        public async Task DeleteImageAsync(string publicId)
        {
            var deletionParams = new DeletionParams(publicId);
            await _cloudinary.DestroyAsync(deletionParams);
        }
    }
}
