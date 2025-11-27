using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.billing.Interfaces;
using SMMS.Application.Features.Identity.Interfaces; // Namespace chứa IFileStorageService
using SMMS.Domain.Entities.billing;
using SMMS.Persistence.Data;
using SMMS.Persistence;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SMMS.Persistence.Repositories.billing;

public class SchoolRevenueRepository : ISchoolRevenueRepository
{
    private readonly EduMealContext _context;
    private readonly IFileStorageService _fileStorageService; // Sửa: Dùng Interface

    // Sửa: Inject IFileStorageService thay vì CloudinaryService
    public SchoolRevenueRepository(EduMealContext ctx, IFileStorageService fileStorageService)
    {
        _context = ctx;
        _fileStorageService = fileStorageService;
    }

    public async Task<long> CreateAsync(SchoolRevenue revenue, IFormFile? file)
    {
        if (file != null && file.Length > 0)
        {
            // 1. Đọc file thành byte[]
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var fileData = ms.ToArray();

            // 2. Tạo tên file mới (để tránh trùng)
            var fileExtension = Path.GetExtension(file.FileName);
            var newFileName = $"contract_{revenue.SchoolId}_{DateTime.UtcNow:yyyyMMddHHmmss}{fileExtension}";

            // 3. Gọi hàm SaveFileAsync (đã khớp với Interface)
            revenue.ContractFileUrl = await _fileStorageService.SaveFileAsync(
                file.FileName,
                fileData,
                "edu-meal/school-contracts", // Đặt tên folder riêng cho hợp đồng
                newFileName
            );
        }

        revenue.CreatedAt = DateTime.UtcNow;

        _context.SchoolRevenues.Add(revenue);
        await _context.SaveChangesAsync();

        return revenue.SchoolRevenueId;
    }

    public async Task UpdateAsync(SchoolRevenue revenue, IFormFile? file)
    {
        if (file != null && file.Length > 0)
        {
            // Logic tương tự như CreateAsync
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var fileData = ms.ToArray();

            var fileExtension = Path.GetExtension(file.FileName);
            var newFileName = $"contract_{revenue.SchoolId}_{DateTime.UtcNow:yyyyMMddHHmmss}{fileExtension}";

            revenue.ContractFileUrl = await _fileStorageService.SaveFileAsync(
                file.FileName,
                fileData,
                "edu-meal/school-contracts",
                newFileName
            );
        }

        revenue.UpdatedAt = DateTime.UtcNow;

        _context.SchoolRevenues.Update(revenue);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _context.SchoolRevenues.FindAsync(id);
        if (entity != null)
        {
            // (Tuỳ chọn) Nếu muốn xóa file trên Cloudinary khi xóa record DB
            // if (!string.IsNullOrEmpty(entity.ContractFileUrl)) {
            //     await _fileStorageService.DeleteFileAsync(entity.ContractFileUrl);
            // }

            _context.SchoolRevenues.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<SchoolRevenue?> GetByIdAsync(long id)
        => await _context.SchoolRevenues.FirstOrDefaultAsync(x => x.SchoolRevenueId == id);

    public IQueryable<SchoolRevenue> GetBySchool(Guid schoolId)
        => _context.SchoolRevenues.Where(r => r.SchoolId == schoolId);
}
