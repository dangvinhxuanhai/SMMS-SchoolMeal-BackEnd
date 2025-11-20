using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.billing.Interfaces;
using SMMS.Domain.Entities.billing;
using SMMS.Persistence.Data;
using SMMS.Persistence.Service;

public class SchoolRevenueRepository : ISchoolRevenueRepository
{
    private readonly EduMealContext _context;
    private readonly CloudinaryService _cloudinary;

    public SchoolRevenueRepository(EduMealContext ctx, CloudinaryService cloud)
    {
        _context = ctx;
        _cloudinary = cloud;
    }

    public async Task<long> CreateAsync(SchoolRevenue revenue, IFormFile? file)
    {
        if (file != null)
        {
            revenue.ContractFileUrl = await _cloudinary.UploadImageAsync(file);
        }

        revenue.CreatedAt = DateTime.UtcNow;

        _context.SchoolRevenues.Add(revenue);
        await _context.SaveChangesAsync();

        return revenue.SchoolRevenueId;
    }

    public async Task UpdateAsync(SchoolRevenue revenue, IFormFile? file)
    {
        if (file != null)
        {
            revenue.ContractFileUrl = await _cloudinary.UploadImageAsync(file);
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
            _context.SchoolRevenues.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<SchoolRevenue?> GetByIdAsync(long id)
        => await _context.SchoolRevenues.FirstOrDefaultAsync(x => x.SchoolRevenueId == id);

    public IQueryable<SchoolRevenue> GetBySchool(Guid schoolId)
        => _context.SchoolRevenues.Where(r => r.SchoolId == schoolId);
}
