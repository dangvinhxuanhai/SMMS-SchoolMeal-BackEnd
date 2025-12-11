using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SMMS.Application.Features.Wardens.Interfaces;

namespace SMMS.Persistence.Service;

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
