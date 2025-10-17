namespace SMMS.Application.Features.Identity.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(string fileName, byte[] fileData, string folder, string newFileName);
        Task<bool> DeleteFileAsync(string filePath);
    }
}
