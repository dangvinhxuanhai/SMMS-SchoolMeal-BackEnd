using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using MediatR;
using Microsoft.Extensions.Options;
using SMMS.Application.Features.Wardens.Commands;
using SMMS.Application.Features.Wardens.DTOs;
using SMMS.Application.Features.Wardens.Interfaces;
using SMMS.Application.Features.Wardens.Queries;
using Microsoft.EntityFrameworkCore;
namespace SMMS.Application.Features.Wardens.Handlers;
public class CloudStorageHandler :
    IRequestHandler<GetAllImagesQuery, List<CloudImageDto>>,
    IRequestHandler<GetImagesByClassQuery, List<CloudImageDto>>,
    IRequestHandler<UploadStudentImageCommand, UploadImageResultDto>,
    IRequestHandler<DeleteImageCommand, bool>
{
    private readonly ICloudStorageRepository _repo;
    private readonly Cloudinary _cloudinary;
    private readonly CloudinarySettings _dbSettings;

    public CloudStorageHandler(
        ICloudStorageRepository repo,
        IOptions<CloudinarySettings> options)
    {
        _repo = repo;
        _dbSettings = options.Value;

        var account = new Account(
            _dbSettings.CloudName,
            _dbSettings.ApiKey,
            _dbSettings.ApiSecret
        );

        _cloudinary = new Cloudinary(account);
    }

    // 🟡 1. Lấy toàn bộ ảnh (option folder)
    public async Task<List<CloudImageDto>> Handle(
        GetAllImagesQuery request,
        CancellationToken cancellationToken)
    {
        var listParams = new ListResourcesParams
        {
            Type = "upload",
            ResourceType = ResourceType.Image,
            MaxResults = request.MaxResults
        };

        var result = await _cloudinary.ListResourcesAsync(listParams);

        if (result.StatusCode != HttpStatusCode.OK)
            throw new Exception($"Cloudinary list failed: {result.Error?.Message}");

        var resources = result.Resources.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(request.Folder))
        {
            var folderPrefix = request.Folder.TrimEnd('/') + "/";
            resources = resources.Where(r => r.PublicId.StartsWith(folderPrefix));
        }

        return resources
            .Select(r => new CloudImageDto
            {
                Url = r.SecureUrl?.ToString() ?? string.Empty,
                PublicId = r.PublicId,
                CreatedAt = DateTime.TryParse(r.CreatedAt, out var parsed)
                    ? parsed
                    : DateTime.MinValue
            })
            .ToList();
    }

    // 🟡 2. Lấy ảnh theo lớp
    public async Task<List<CloudImageDto>> Handle(
        GetImagesByClassQuery request,
        CancellationToken cancellationToken)
    {
        var classInfo = await (
            from c in _repo.Classes
            join y in _repo.AcademicYears on c.YearId equals y.YearId
            join sch in _repo.Schools on c.SchoolId equals sch.SchoolId
            where c.ClassId == request.ClassId
            select new
            {
                SchoolName = sch.SchoolName,
                YearName = y.YearName,
                ClassName = c.ClassName
            }
        ).FirstOrDefaultAsync(cancellationToken);

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

        // Dùng lại handler GetAllImagesQuery
        var result = await Handle(
            new GetAllImagesQuery(folderPath, request.MaxResults),
            cancellationToken);

        return result;
    }

    // 🟢 3. Upload ảnh học sinh
    public async Task<UploadImageResultDto> Handle(
        UploadStudentImageCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;
        string? baseFolder = command.BaseFolder ?? "student_images";

        var file = request.File;
        if (file == null || file.Length == 0)
            throw new InvalidOperationException("Không có tệp hợp lệ để upload.");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(fileExtension))
            throw new InvalidOperationException("Chỉ được phép upload các tệp hình ảnh (.jpg, .jpeg, .png, .gif, .webp)");

        // 🔹 Lấy thông tin học sinh, lớp, trường, năm
        var studentInfo = await (
            from s in _repo.Students
            join sc in _repo.StudentClasses on s.StudentId equals sc.StudentId
            join c in _repo.Classes on sc.ClassId equals c.ClassId
            join y in _repo.AcademicYears on c.YearId equals y.YearId
            join sch in _repo.Schools on c.SchoolId equals sch.SchoolId
            where s.StudentId == request.StudentId
            select new
            {
                SchoolName = sch.SchoolName,
                YearName = y.YearName,
                ClassName = c.ClassName
            }
        ).FirstOrDefaultAsync(cancellationToken);

        string school = studentInfo?.SchoolName ?? "Unknown_School";
        string year = studentInfo?.YearName ?? "Unknown_Year";
        string className = studentInfo?.ClassName ?? "Unknown_Class";

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

        school = Normalize(school);
        year = Normalize(year);
        className = Normalize(className);

        var folderPath = $"{baseFolder}/{school}/{year}/{className}";

        await using var stream = file.OpenReadStream();

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = folderPath,
            UseFilename = true,
            UniqueFilename = true,
            Overwrite = false
        };

        var result = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

        if (result.StatusCode != HttpStatusCode.OK)
            throw new Exception($"Cloudinary upload failed: {result.Error?.Message}");

        return new UploadImageResultDto
        {
            Url = result.SecureUrl.ToString(),
            PublicId = result.PublicId
        };
    }

    // 🧹 4. Xóa ảnh
    public async Task<bool> Handle(
        DeleteImageCommand request,
        CancellationToken cancellationToken)
    {
        var deletionParams = new DeletionParams(request.PublicId);
        var result = await _cloudinary.DestroyAsync(deletionParams);
        return result.Result == "ok";
    }
}
