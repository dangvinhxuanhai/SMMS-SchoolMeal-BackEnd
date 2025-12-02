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
public sealed class UpdateSchoolCommandHandler : IRequestHandler<UpdateSchoolCommand, bool>
{
    private readonly IReadRepository<School, Guid> _readRepo;
    private readonly IWriteRepository<School, Guid> _writeRepo;

    public UpdateSchoolCommandHandler(
        IReadRepository<School, Guid> readRepo,
        IWriteRepository<School, Guid> writeRepo)
    {
        _readRepo = readRepo;
        _writeRepo = writeRepo;
    }

    public async Task<bool> Handle(UpdateSchoolCommand request, CancellationToken ct)
    {
        var entity = await _readRepo.GetByIdAsync(
            request.Id,
            keyName: nameof(School.SchoolId),
            ct
        );
        if (entity is null) return false;

        var dto = request.Dto;
        entity.SchoolName = dto.SchoolName.Trim();
        entity.ContactEmail = dto.ContactEmail;
        entity.Hotline = dto.Hotline;
        //entity.SchoolContract = dto.SchoolContract;
        entity.SchoolAddress = dto.SchoolAddress;
        entity.IsActive = dto.IsActive;
        entity.UpdatedBy = dto.UpdatedBy;
        entity.UpdatedAt = DateTime.UtcNow;

        await _writeRepo.UpdateAsync(entity, ct);
        await _writeRepo.SaveChangeAsync(ct);
        return true;
    }
}
