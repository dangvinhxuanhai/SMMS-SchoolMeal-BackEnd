using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.school.Commands;
using SMMS.Application.Features.school.DTOs;
using SMMS.Application.Features.school.Interfaces;
using SMMS.Application.Features.school.Queries;
using SMMS.Domain.Entities.school;

namespace SMMS.Application.Features.school.Handlers
{
    public class SchoolCommandHandler :
       IRequestHandler<CreateSchoolCommand, Guid>,
       IRequestHandler<UpdateSchoolCommand, Unit>,
       IRequestHandler<DeleteSchoolCommand, Unit>,
       IRequestHandler<UpdateManagerStatusCommand, bool>
    {
        private readonly ISchoolRepository _repo;

        public SchoolCommandHandler(ISchoolRepository repo)
        {
            _repo = repo;
        }

        public async Task<Guid> Handle(CreateSchoolCommand request, CancellationToken cancellationToken)
        {
            var dto = request.SchoolDto;

            var school = new School
            {
                SchoolId = Guid.NewGuid(),
                SchoolName = dto.SchoolName,
                ContactEmail = dto.ContactEmail,
                Hotline = dto.Hotline,
                SchoolAddress = dto.SchoolAddress,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.CreatedBy
            };
            await _repo.AddAsync(school);
            return school.SchoolId;
        }

        public async Task<Unit> Handle(UpdateSchoolCommand request, CancellationToken cancellationToken)
        {
            var existing = await _repo.GetByIdAsync(request.SchoolId);
            if (existing == null) throw new KeyNotFoundException("School not found");

            var dto = request.SchoolDto;

            existing.SchoolName = dto.SchoolName;
            existing.ContactEmail = dto.ContactEmail;
            existing.Hotline = dto.Hotline;
            existing.SchoolAddress = dto.SchoolAddress;
            existing.IsActive = dto.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = request.UpdatedBy;
            await _repo.UpdateAsync(existing, dto.ManagerIsActive);
            return Unit.Value;
        }

        public async Task<Unit> Handle(DeleteSchoolCommand request, CancellationToken cancellationToken)
        {
            var school = await _repo.GetByIdAsync(request.SchoolId);
            if (school == null) throw new KeyNotFoundException("School not found");

            school.IsActive = false;
            school.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(school, false);
            return Unit.Value;
        }
        public async Task<bool> Handle(UpdateManagerStatusCommand request, CancellationToken cancellationToken)
        {
            return await _repo.UpdateManagerStatusAsync(request.SchoolId, request.IsActive);
        }
    }
    public class SchoolQueryHandler :
        IRequestHandler<GetAllSchoolsQuery, IEnumerable<SchoolDTO>>,
        IRequestHandler<GetSchoolByIdQuery, SchoolDTO?>,
        IRequestHandler<GetManagerStatusQuery, bool?>
    {
        private readonly ISchoolRepository _repo;

        public SchoolQueryHandler(ISchoolRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<SchoolDTO>> Handle(GetAllSchoolsQuery request, CancellationToken cancellationToken)
        {
            var schools = _repo.GetAllSchools()
                .Select(s => new SchoolDTO
                {
                    SchoolId = s.SchoolId,
                    SchoolName = s.SchoolName,
                    ContactEmail = s.ContactEmail,
                    Hotline = s.Hotline,
                    SchoolAddress = s.SchoolAddress,
                    IsActive = s.IsActive,
                    CreatedAt = s.CreatedAt,
                    StudentCount = s.Students.Count(),
                    ManagerIsActive = s.Users
                    .Where(u => u.RoleId == 2)
                     .Select(u => (bool)u.IsActive)
                     .FirstOrDefault()
                });

            return schools;
        }

        public async Task<SchoolDTO?> Handle(GetSchoolByIdQuery request, CancellationToken cancellationToken)
        {
            var s = await _repo.GetByIdAsync(request.SchoolId);
            if (s == null) return null;
             var manager = s.Users?.FirstOrDefault(u => u.RoleId == 2);
            return new SchoolDTO
            {
                SchoolId = s.SchoolId,
                SchoolName = s.SchoolName,
                ContactEmail = s.ContactEmail,
                Hotline = s.Hotline,
                SchoolAddress = s.SchoolAddress,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt,
                StudentCount = s.Students?.Count() ?? 0,
                ManagerIsActive = manager.IsActive
            };
        }
        public async Task<bool?> Handle(GetManagerStatusQuery request, CancellationToken cancellationToken)
        {
            return await _repo.GetManagerStatusAsync(request.SchoolId);
        }
    }
}
