using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SMMS.Application.Features.Wardens.Interfaces;
using SMMS.Persistence.Configurations;


namespace SMMS.Persistence.Repositories.Wardens;
public class CloudStorageService : ICloudStorageService
{
    private readonly Cloudinary _cloudinary;
    private readonly CloudinarySettings _settings;

    public CloudStorageService(IOptions<CloudinarySettings> options)
    {
        _settings = options.Value;

        var account = new Account(
            _settings.CloudName,
            _settings.ApiKey,
            _settings.ApiSecret
        );

        _cloudinary = new Cloudinary(account);
    }
    // 🟡 Lấy danh sách toàn bộ ảnh (hoặc trong 1 folder)
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
    public async Task<(string Url, string PublicId)> UploadImageAsync(IFormFile file, string? folder = null)
    {
        if (file == null || file.Length == 0)
            return (string.Empty, string.Empty);

        // 🔹 Kiểm tra định dạng ảnh hợp lệ
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(fileExtension))
            throw new InvalidOperationException("Chỉ được phép upload các tệp hình ảnh (.jpg, .jpeg, .png, .gif, .webp)");

        // 🔹 Tiến hành upload lên Cloudinary
        await using var stream = file.OpenReadStream();

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = string.IsNullOrWhiteSpace(folder) ? _settings.DefaultFolder : folder,
            UseFilename = true,
            UniqueFilename = true,
            Overwrite = false
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.StatusCode != HttpStatusCode.OK)
            throw new Exception($"Cloudinary upload failed: {result.Error?.Message}");

        return (result.SecureUrl.ToString(), result.PublicId);
    }


    public async Task<bool> DeleteImageAsync(string publicId)
    {
        var deletionParams = new DeletionParams(publicId);
        var result = await _cloudinary.DestroyAsync(deletionParams);
        return result.Result == "ok";
    }
    
}
