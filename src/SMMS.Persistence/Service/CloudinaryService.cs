using System;
using System.IO;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using SMMS.Application.Features.Identity.Interfaces;
using SMMS.Application.Features.Wardens.Interfaces;

namespace SMMS.Persistence.Service
{
    public class CloudinaryService : IFileStorageService
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

        // FIX 1: Đổi tên hàm thành SaveFileAsync để khớp với Interface
        public async Task<string> SaveFileAsync(string fileName, byte[] fileData, string folderName, string newFileName)
        {
            if (fileData == null || fileData.Length == 0) return null;

            using var stream = new MemoryStream(fileData);

            var uploadParams = new ImageUploadParams()
            {
                // Dùng newFileName làm tên file trên cloud
                File = new FileDescription(newFileName, stream),
                Folder = folderName, // Ví dụ: "edu-meal/user-avatars"
                PublicId = Path.GetFileNameWithoutExtension(newFileName),
                Overwrite = true,
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            // Trả về link HTTPS (VD: https://res.cloudinary.com/...)
            return uploadResult.SecureUrl.ToString();
        }

        // FIX 2: Thêm hàm DeleteFileAsync để khớp với Interface
        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl)) return false;

            try
            {
                // Để xóa ảnh trên Cloudinary cần PublicId.
                // Logic tách PublicId từ URL khá phức tạp tùy vào cấu hình folder.
                // Tạm thời ta return true để code biên dịch được (không lỗi Interface).
                // Nếu sau này cần xóa thật, bạn cần lưu PublicId vào DB hoặc parse URL kỹ hơn.

                // Ví dụ cơ bản (có thể chưa chính xác 100% với mọi link):
                var uri = new Uri(fileUrl);
                var pathSegments = uri.Segments;
                var fileName = pathSegments[pathSegments.Length - 1];
                var publicId = Path.GetFileNameWithoutExtension(fileName);

                var deletionParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deletionParams);

                return result.Result == "ok";
            }
            catch
            {
                // Ghi log lỗi nếu cần
                return false;
            }
        }

        // methodc cũ, code trên là các method mới từ nhánh của a Hưng
        public async Task<string?> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0) return null;

            using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "school_contracts" // Tạo folder trên Cloudinary
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            return uploadResult.SecureUrl?.ToString();
        }
    }
}
