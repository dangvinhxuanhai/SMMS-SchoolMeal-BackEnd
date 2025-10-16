using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.foodmenu.DTOs;

namespace SMMS.Application.Features.foodmenu.Interfaces;
public interface IWeeklyMenuRepository
{
    /// <summary>
    /// Trả về thực đơn tuần chứa ngày 'anyDateInWeek' dành cho học sinh.
    /// Nếu không có lịch tuần được Publish, trả về null.
    /// </summary>
    Task<WeekMenuDto?> GetWeekMenuAsync(Guid studentId, DateTime anyDateInWeek, CancellationToken ct = default);

    /// <summary>
    /// Danh sách các tuần đã được lập lịch cho học sinh (để fill dropdown).
    /// Optional filter theo khoảng ngày.
    /// </summary>
    Task<IReadOnlyList<WeekOptionDto>> GetAvailableWeeksAsync(Guid studentId, DateTime? from = null, DateTime? to = null, CancellationToken ct = default);
}
