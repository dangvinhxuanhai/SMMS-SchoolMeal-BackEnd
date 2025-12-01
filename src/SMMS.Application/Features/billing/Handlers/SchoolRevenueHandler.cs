using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.billing.Commands;
using SMMS.Application.Features.billing.Interfaces;
using SMMS.Application.Features.billing.Queries;
using SMMS.Domain.Entities.billing;

namespace SMMS.Application.Features.billing.Handlers
{
    public class SchoolRevenueHandler :
        IRequestHandler<CreateSchoolRevenueCommand, long>,
        IRequestHandler<UpdateSchoolRevenueCommand, Unit>,
        IRequestHandler<DeleteSchoolRevenueCommand, Unit>,
        IRequestHandler<GetRevenuesBySchoolQuery, IEnumerable<SchoolRevenue>>,
        IRequestHandler<GetRevenueByIdQuery, SchoolRevenue?>
    {
        private readonly ISchoolRevenueRepository _repo;

        public SchoolRevenueHandler(ISchoolRevenueRepository repo)
        {
            _repo = repo;
        }

        // Create
        public async Task<long> Handle(CreateSchoolRevenueCommand request, CancellationToken ct)
        {
            var dto = request.Dto;

            var revenue = new SchoolRevenue
            {
                SchoolId = dto.SchoolId,
                RevenueDate = dto.RevenueDate,
                RevenueAmount = dto.RevenueAmount,
                ContractCode = dto.ContractCode,
                ContractNote = dto.ContractNote,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.CreatedBy
            };

            // Upload contract file nếu có
            return await _repo.CreateAsync(revenue, dto.ContractFile);
        }

        // Update
        public async Task<Unit> Handle(UpdateSchoolRevenueCommand request, CancellationToken ct)
        {
            var existing = await _repo.GetByIdAsync(request.RevenueId);
            if (existing == null) throw new KeyNotFoundException("Revenue not found");

            var dto = request.Dto;

            existing.RevenueDate = dto.RevenueDate;
            existing.RevenueAmount = dto.RevenueAmount;
            existing.ContractCode = dto.ContractCode;
            existing.ContractNote = dto.ContractNote;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = request.UpdatedBy;

            await _repo.UpdateAsync(existing, dto.ContractFile);
            return Unit.Value;
        }

        // Delete
        public async Task<Unit> Handle(DeleteSchoolRevenueCommand request, CancellationToken ct)
        {
            await _repo.DeleteAsync(request.RevenueId);
            return Unit.Value;
        }

        // Get all revenues by SchoolId
        public async Task<IEnumerable<SchoolRevenue>> Handle(GetRevenuesBySchoolQuery request, CancellationToken ct)
        {
            // Lấy IQueryable từ repository và convert thành List async
            return await _repo.GetBySchool(request.SchoolId).ToListAsync(ct);
        }

        // Get revenue by Id
        public async Task<SchoolRevenue?> Handle(GetRevenueByIdQuery request, CancellationToken ct)
        {
            return await _repo.GetByIdAsync(request.RevenueId);
        }
    }
}
