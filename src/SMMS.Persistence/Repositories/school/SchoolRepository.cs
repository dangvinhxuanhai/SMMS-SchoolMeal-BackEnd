using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.school.Interfaces;
using SMMS.Domain.Entities.school;
using SMMS.Persistence.DbContextSite;

namespace SMMS.Persistence.Repositories.school
{
    public class SchoolRepository : ISchoolRepository
    {
        private readonly EduMealContext _context;

        public SchoolRepository(EduMealContext context)
        {
            _context = context;
        }
        public IQueryable<School> GetAllSchools()
        {
            return _context.Schools
                .Include(s => s.Students)
                .AsNoTracking();
        }
        public async Task<List<School>> GetAllAsync()
        {
            return await _context.Schools
                .Include(s => s.Students)
                .Select(s => new School
                {
                    SchoolId = s.SchoolId,
                    SchoolName = s.SchoolName,
                    ContactEmail = s.ContactEmail,
                    Hotline = s.Hotline,
                    SchoolAddress = s.SchoolAddress,
                    IsActive = s.IsActive,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    Students = s.Students
                })
                .ToListAsync();
        }

        public async Task<School?> GetByIdAsync(Guid id)
        {
            return await _context.Schools
                .Include(s => s.Students)
                .FirstOrDefaultAsync(s => s.SchoolId == id);
        }

        public async Task AddAsync(School school)
        {
            _context.Schools.Add(school);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(School school)
        {
            _context.Schools.Update(school);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.Schools.FindAsync(id);
            if (entity != null)
            {
                _context.Schools.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
