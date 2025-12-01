using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Meal.DTOs;

namespace SMMS.Application.Features.Meal.Command;
public class CreateScheduleMealCommand : IRequest<long>
{
     [JsonIgnore]
    public Guid SchoolId { get; set; }

     [JsonIgnore]
    public Guid CreatedByUserId { get; set; }

    public DateTime WeekStart { get; set; }  // ngày đầu tuần (date only)
    public DateTime WeekEnd { get; set; }  // ngày cuối tuần
    public short WeekNo { get; set; }  // số tuần (1..53)
    public short YearNo { get; set; }  // ví dụ: 2025

    /// <summary>
    /// AcademicYearId dùng để lưu vào Menus.YearId (FK school.AcademicYears)
    /// </summary>
    public int? AcademicYearId { get; set; }

    /// <summary>
    /// Nếu chọn 1 menu template cũ → copy ra ScheduleMeal mới.
    /// Nếu null → tạo mới template từ data gửi lên.
    /// </summary>
    public int? BaseMenuId { get; set; }

    /// <summary>
    /// Danh sách bữa ăn trong tuần (theo ngày thực tế).
    /// Nếu BaseMenuId có giá trị và DailyMeals rỗng → handler auto copy từ template.
    /// </summary>
    public List<DailyMealRequestDto> DailyMeals { get; set; } = new();
}
