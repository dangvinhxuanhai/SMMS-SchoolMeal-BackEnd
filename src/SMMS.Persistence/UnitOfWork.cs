using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Abstractions;
using SMMS.Persistence.Data;

namespace SMMS.Persistence;
public class UnitOfWork : IUnitOfWork
{
    private readonly EduMealContext _context;

    public UnitOfWork(EduMealContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
