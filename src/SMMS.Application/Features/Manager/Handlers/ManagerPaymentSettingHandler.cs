using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Common.Helpers;
using SMMS.Application.Features.Manager.Commands;
using SMMS.Application.Features.Manager.DTOs;
using SMMS.Application.Features.Manager.Interfaces;
using SMMS.Application.Features.Manager.Queries;
using SMMS.Domain.Entities.billing;

namespace SMMS.Application.Features.Manager.Handlers;
internal static class SchoolPaymentSettingMapping
{
    public static SchoolPaymentSettingDto ToDto(this SchoolPaymentSetting e)
    {
        return new SchoolPaymentSettingDto
        {
            SettingId = e.SettingId,
            SchoolId = e.SchoolId,
            FromMonth = e.FromMonth,
            ToMonth = e.ToMonth, // ✅ nullable
            TotalAmount = e.TotalAmount,
            MealPricePerDay = e.MealPricePerDay,
            Note = e.Note,
            IsActive = e.IsActive,
            CreatedAt = e.CreatedAt
        };
    }
}
public class ManagerPaymentSettingHandler
{
  

    // ===== QUERIES =====

    public class GetSchoolPaymentSettingsHandler
        : IRequestHandler<GetSchoolPaymentSettingsQuery, List<SchoolPaymentSettingDto>>
    {
        private readonly IManagerPaymentSettingRepository _repo;

        public GetSchoolPaymentSettingsHandler(IManagerPaymentSettingRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<SchoolPaymentSettingDto>> Handle(
            GetSchoolPaymentSettingsQuery request,
            CancellationToken cancellationToken)
        {
            var list = await _repo.GetBySchoolAsync(request.SchoolId, cancellationToken);
            return list.Select(x => x.ToDto()).ToList();
        }
    }

    public class GetSchoolPaymentSettingByIdHandler
        : IRequestHandler<GetSchoolPaymentSettingByIdQuery, SchoolPaymentSettingDto?>
    {
        private readonly IManagerPaymentSettingRepository _repo;

        public GetSchoolPaymentSettingByIdHandler(IManagerPaymentSettingRepository repo)
        {
            _repo = repo;
        }

        public async Task<SchoolPaymentSettingDto?> Handle(
            GetSchoolPaymentSettingByIdQuery request,
            CancellationToken cancellationToken)
        {
            var entity = await _repo.GetByIdAsync(request.SettingId, cancellationToken);
            return entity?.ToDto();
        }
    }

    // ===== COMMANDS =====

    public class CreateSchoolPaymentSettingHandler
        : IRequestHandler<CreateSchoolPaymentSettingCommand, SchoolPaymentSettingDto>
    {
        private readonly IManagerPaymentSettingRepository _repo;
        public CreateSchoolPaymentSettingHandler(IManagerPaymentSettingRepository repo) => _repo = repo;

        public async Task<SchoolPaymentSettingDto> Handle(CreateSchoolPaymentSettingCommand command, CancellationToken ct)
        {
            var r = command.Request;

            if (r.FromMonth < 1 || r.FromMonth > 12)
                throw new ArgumentException("Tháng phải nằm trong khoảng 1..12.");
            if (r.MealPricePerDay < 0)
                throw new ArgumentException("MealPricePerDay không được âm.");

            var existed = await _repo.ExistsMonthAsync(r.SchoolId, r.FromMonth, null, ct);
            if (existed) throw new InvalidOperationException("Tháng này đã có cấu hình payment setting.");

            var year = DateTime.UtcNow.Year;
            var (fromD, toD) = DateOnlyUtils.GetMonthRange(year, r.FromMonth);

            var total = r.MealPricePerDay * DateOnlyUtils.CountWeekdays(fromD, toD);

            var entity = new SchoolPaymentSetting
            {
                SchoolId = r.SchoolId,
                FromMonth = r.FromMonth,
                ToMonth = null,
                TotalAmount = total,
                MealPricePerDay = r.MealPricePerDay,
                Note = r.Note,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            entity = await _repo.AddAsync(entity, ct);
            return entity.ToDto();
        }
    }

    public class UpdateSchoolPaymentSettingHandler
        : IRequestHandler<UpdateSchoolPaymentSettingCommand, SchoolPaymentSettingDto?>
    {
        private readonly IManagerPaymentSettingRepository _repo;
        public UpdateSchoolPaymentSettingHandler(IManagerPaymentSettingRepository repo) => _repo = repo;

        public async Task<SchoolPaymentSettingDto?> Handle(UpdateSchoolPaymentSettingCommand command, CancellationToken ct)
        {
            var entity = await _repo.GetByIdAsync(command.SettingId, ct);
            if (entity == null) return null;

            var r = command.Request;

            if (r.FromMonth < 1 || r.FromMonth > 12)
                throw new ArgumentException("Tháng phải nằm trong khoảng 1..12.");
            if (r.MealPricePerDay < 0)
                throw new ArgumentException("MealPricePerDay không được âm.");

            var existed = await _repo.ExistsMonthAsync(entity.SchoolId, r.FromMonth, entity.SettingId, ct);
            if (existed) throw new InvalidOperationException("Tháng này đã có cấu hình payment setting.");

            var year = DateTime.UtcNow.Year;
            var (fromD, toD) = DateOnlyUtils.GetMonthRange(year, r.FromMonth);

            var total = r.MealPricePerDay * DateOnlyUtils.CountWeekdays(fromD, toD);

            entity.FromMonth = r.FromMonth;
            entity.ToMonth = null;
            entity.TotalAmount = total;
            entity.MealPricePerDay = r.MealPricePerDay;
            entity.Note = r.Note;
            entity.IsActive = r.IsActive;

            await _repo.UpdateAsync(entity, ct);
            return entity.ToDto();
        }
    }

    public class DeleteSchoolPaymentSettingHandler
        : IRequestHandler<DeleteSchoolPaymentSettingCommand, bool>
    {
        private readonly IManagerPaymentSettingRepository _repo;
        public DeleteSchoolPaymentSettingHandler(IManagerPaymentSettingRepository repo) => _repo = repo;

        public async Task<bool> Handle(DeleteSchoolPaymentSettingCommand command, CancellationToken ct)
        {
            var entity = await _repo.GetByIdAsync(command.SettingId, ct);
            if (entity == null) return false;

            await _repo.DeleteAsync(entity, ct);
            return true;
        }
    }
}
