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
       IRequestHandler<DeleteSchoolCommand, Unit>
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
            await _repo.UpdateAsync(existing);
            return Unit.Value;
        }

        public async Task<Unit> Handle(DeleteSchoolCommand request, CancellationToken cancellationToken)
        {
            var school = await _repo.GetByIdAsync(request.SchoolId);
            if (school == null) throw new KeyNotFoundException("School not found");

            school.IsActive = false;
            school.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(school);
            return Unit.Value;
        }
    }
    public class SchoolQueryHandler :
        IRequestHandler<GetAllSchoolsQuery, IEnumerable<SchoolDTO>>,
        IRequestHandler<GetSchoolByIdQuery, SchoolDTO?>
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
                    StudentCount = s.Students.Count()
                });

            return schools;
        }

        public async Task<SchoolDTO?> Handle(GetSchoolByIdQuery request, CancellationToken cancellationToken)
        {
            var s = await _repo.GetByIdAsync(request.SchoolId);
            if (s == null) return null;

            return new SchoolDTO
            {
                SchoolId = s.SchoolId,
                SchoolName = s.SchoolName,
                ContactEmail = s.ContactEmail,
                Hotline = s.Hotline,
                SchoolAddress = s.SchoolAddress,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt,
                StudentCount = s.Students.Count()
            };
        }
    }
}
