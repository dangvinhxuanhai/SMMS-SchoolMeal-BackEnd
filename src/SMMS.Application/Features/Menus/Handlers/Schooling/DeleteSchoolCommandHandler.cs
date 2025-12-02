using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Common.Interfaces;
using SMMS.Application.Features.Menus.Command.Schooling;
using SMMS.Domain.Entities.school;

namespace SMMS.Application.Features.Menus.Handlers.Schooling;
public sealed class DeleteSchoolCommandHandler : IRequestHandler<DeleteSchoolCommand, bool>
{
    private readonly IReadRepository<School, Guid> _readRepo;
    private readonly IWriteRepository<School, Guid> _writeRepo;

    public DeleteSchoolCommandHandler(
        IReadRepository<School, Guid> readRepo,
        IWriteRepository<School, Guid> writeRepo)
    {
        _readRepo = readRepo;
        _writeRepo = writeRepo;
    }

    public async Task<bool> Handle(DeleteSchoolCommand request, CancellationToken ct)
    {
        var exists = await _readRepo.GetByIdAsync(
            request.Id,
            keyName: nameof(School.SchoolId),
            ct
        );
        if (exists is null) return false;

        await _writeRepo.DeleteByIdAsync(request.Id, ct);
        await _writeRepo.SaveChangeAsync(ct);
        return true;
    }
}
