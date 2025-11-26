using Microsoft.AspNetCore.Http;
using SMMS.Application.Features.school.DTOs;
using SMMS.Domain.Entities.school;

namespace SMMS.Application.Features.school.Interfaces
{
    public interface ISchoolRepository
    {
        IQueryable<School> GetAllSchools();
        Task<List<School>> GetAllAsync();
        Task<School?> GetByIdAsync(Guid id);
        Task AddAsync(School school);
        Task UpdateAsync(School school);
        Task DeleteAsync(Guid id);
    }
}
