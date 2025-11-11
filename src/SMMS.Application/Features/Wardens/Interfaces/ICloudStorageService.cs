using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SMMS.Application.Features.Wardens.Interfaces;

public interface ICloudStorageService
{
    Task<(string Url, string PublicId)> UploadImageAsync(
          IFormFile file,
          Guid studentId,
          string? baseFolder = "student_images");

    Task<bool> DeleteImageAsync(string publicId);
    Task<List<(string Url, string PublicId, DateTime CreatedAt)>> GetAllImagesAsync(string? folder = null, int maxResults = 100);
    Task<List<(string Url, string PublicId, DateTime CreatedAt)>> GetImagesByClassAsync(
            Guid classId,
            int maxResults = 100);

}
