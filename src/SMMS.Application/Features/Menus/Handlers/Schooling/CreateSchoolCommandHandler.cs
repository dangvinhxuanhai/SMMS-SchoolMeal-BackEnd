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
public sealed class CreateSchoolCommandHandler : IRequestHandler<CreateSchoolCommand, Guid>
{
    private readonly IWriteRepository<School, Guid> _repo;

    public CreateSchoolCommandHandler(IWriteRepository<School, Guid> repo)
    {
        _repo = repo;
    }

    public async Task<Guid> Handle(CreateSchoolCommand request, CancellationToken ct)
    {
        var dto = request.Dto;

        var entity = new School
        {
            SchoolId = Guid.NewGuid(),
            SchoolName = dto.SchoolName.Trim(),
            ContactEmail = dto.ContactEmail,
            Hotline = dto.Hotline,
            //SchoolContract = dto.SchoolContract,
            SchoolAddress = dto.SchoolAddress,
            IsActive = dto.IsActive,
            CreatedBy = dto.CreatedBy,
        };

        await _repo.AddAsync(entity, ct);
        await _repo.SaveChangeAsync(ct);
        return entity.SchoolId;
    }
}
