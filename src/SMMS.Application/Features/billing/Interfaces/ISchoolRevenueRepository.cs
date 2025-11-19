using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SMMS.Domain.Entities.billing;

namespace SMMS.Application.Features.billing.Interfaces
{
    public interface ISchoolRevenueRepository
    {
        Task<long> CreateAsync(SchoolRevenue revenue, IFormFile? file);
        Task UpdateAsync(SchoolRevenue revenue, IFormFile? file);
        Task DeleteAsync(long id);

        Task<SchoolRevenue?> GetByIdAsync(long id);
        IQueryable<SchoolRevenue> GetBySchool(Guid schoolId);
    }

}
