using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.school.Interfaces;
using SMMS.Persistence.Data;

namespace SMMS.Persistence.Repositories.schools;
public class StudentAllergenRepository : IStudentAllergenRepository
{
    private readonly EduMealContext _dbContext;

    public StudentAllergenRepository(EduMealContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<int>> GetAllergenIdsForSchoolAsync(
        Guid schoolId,
        CancellationToken ct = default)
    {
        // Lấy các allergen mà học sinh của trường này đang được ghi nhận
        // dùng nutrition.StudentAllergens + school.Students
        var query =
            from sa in _dbContext.StudentAllergens
            join s in _dbContext.Students
                on sa.StudentId equals s.StudentId
            where s.SchoolId == schoolId
            select sa.AllergenId;

        var ids = await query
            .Distinct()
            .ToListAsync(ct);

        return ids;
    }
}
