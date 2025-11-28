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
        var urls = resources
       .Select(r => r.SecureUrl?.ToString() ?? string.Empty)
       .Where(u => !string.IsNullOrEmpty(u))
       .ToList();

        var dbImages = await _repo.StudentImages
            .Where(si => urls.Contains(si.ImageUrl))
            .ToListAsync(cancellationToken);

        var dbMap = dbImages.ToDictionary(x => x.ImageUrl, x => x.ImageId);

        var resultDto = resources
            .Select(r =>
            {
                var url = r.SecureUrl?.ToString() ?? string.Empty;
                dbMap.TryGetValue(url, out var dbImageId);

                return new CloudImageDto
                {
                    Url = url,
                    PublicId = r.PublicId,
                    ImageId = dbImageId.ToString(), // 👈 ImageId từ DB
                    CreatedAt = DateTime.TryParse(r.CreatedAt, out var parsed)
                        ? parsed
                        : DateTime.MinValue
                };
            })
            .ToList();
        return resultDto;
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
    // 🟢 3. Upload ảnh học sinh (theo ClassId, tự chọn student đầu tiên)
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

        // 🔹 1. Lấy student đầu tiên của class (nếu cần dùng StudentId)
        Guid studentId;

        if (request.StudentId.HasValue && request.StudentId.Value != Guid.Empty)
        {
            studentId = request.StudentId.Value;
        }
        else
        {
            studentId = await _repo.StudentClasses
                .Where(sc => sc.ClassId == request.ClassId && sc.RegistStatus == true)
                .OrderBy(sc => sc.JoinedDate)
                .Select(sc => sc.StudentId)
                .FirstOrDefaultAsync(cancellationToken);

            if (studentId == Guid.Empty)
                throw new InvalidOperationException("Lớp này chưa có học sinh nào đăng ký.");
        }

        // 🔹 2. Lấy thông tin trường / năm học / lớp từ ClassId (không cần StudentId nữa)
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
            throw new InvalidOperationException("Không tìm thấy thông tin lớp học.");

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

        string school = Normalize(classInfo.SchoolName);
        string year = Normalize(classInfo.YearName);
        string className = Normalize(classInfo.ClassName);

        // Folder dạng: student_images/Truong_A/2025-2026/Lop_1A
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

        // 🔹 Nếu sau này bạn insert bản ghi StudentImages, bạn có sẵn studentId ở đây
        // tạo StudentImage entity và lưu bằng _repo.DbContext.SaveChangesAsync() chẳng hạn.

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
