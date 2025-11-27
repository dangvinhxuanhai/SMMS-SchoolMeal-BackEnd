using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.school.Interfaces;
using SMMS.Persistence.Data;

namespace SMMS.Persistence.Repositories.schools;
public class ClassStudentRepository : IClassStudentRepository
{
    private readonly EduMealContext _ctx;

    public ClassStudentRepository(EduMealContext ctx)
    {
        _ctx = ctx;
    }

    public Task<int> CountActiveStudentsAsync(Guid classId, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        return _ctx.StudentClasses
            .Where(sc => sc.ClassId == classId
                         && sc.RegistStatus == true
                         && (sc.LeftDate == null || sc.LeftDate > today))
            .CountAsync(ct);
    }
}
