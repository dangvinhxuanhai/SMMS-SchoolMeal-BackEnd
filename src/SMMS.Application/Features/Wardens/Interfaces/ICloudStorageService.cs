using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SMMS.Application.Features.Wardens.Interfaces;

public interface ICloudStorageService
{
    Task<(string Url, string PublicId)> UploadImageAsync(IFormFile file, string? folder = null);
    Task<bool> DeleteImageAsync(string publicId);
    Task<List<(string Url, string PublicId, DateTime CreatedAt)>> GetAllImagesAsync(string? folder = null, int maxResults = 100);

}
