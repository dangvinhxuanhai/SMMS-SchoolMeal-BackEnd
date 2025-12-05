using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.Plan.DTOs;
using SMMS.Application.Features.Plan.Interfaces;
using SMMS.Domain.Entities.purchasing;
using SMMS.Persistence.Data;

namespace SMMS.Persistence.Repositories.purchasing;
public class PurchasePlanRepository : IPurchasePlanRepository
{
    private readonly EduMealContext _context;

    public PurchasePlanRepository(EduMealContext context)
    {
        _context = context;
    }

    public async Task<int> CreateFromScheduleAsync(
    long scheduleMealId,
    Guid staffId,
    CancellationToken cancellationToken)
    {
        // 1. Check đã có plan cho ScheduleMeal này chưa
        var existed = await _context.PurchasePlans
            .AnyAsync(p => p.ScheduleMealId == scheduleMealId, cancellationToken);

        if (existed)
        {
            throw new InvalidOperationException("Purchase plan for this schedule already exists.");
        }

        // 2. Lấy thông tin Schedule để biết SchoolId + khoảng tuần
        var schedule = await _context.ScheduleMeals
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ScheduleMealId == scheduleMealId, cancellationToken);

        if (schedule == null)
        {
            throw new InvalidOperationException("Schedule meal not found.");
        }

        var schoolId = schedule.SchoolId;
        var weekStart = schedule.WeekStart; // DATE → thường map DateOnly
        var weekEnd = schedule.WeekEnd;

        // 3. Tổng số học sinh active của trường
        var totalActiveStudents = await _context.Students
            .Where(s => s.SchoolId == schoolId && s.IsActive)
            .CountAsync(cancellationToken);

        // 4. Lấy số HS vắng theo từng ngày trong tuần đó
        var absenceByDateList = await (
            from a in _context.Attendances
            join st in _context.Students
                on a.StudentId equals st.StudentId
            where st.SchoolId == schoolId
                  && a.AbsentDate >= weekStart
                  && a.AbsentDate <= weekEnd
            group a by a.AbsentDate into g
            select new
            {
                Date = g.Key,
                Count = g.Count()
            }
        ).ToListAsync(cancellationToken);

        var absenceByDate = absenceByDateList.ToDictionary(x => x.Date, x => x.Count);

        // 5. Lấy usage nguyên liệu theo từng bữa (chưa nhân số HS)
        //    Mỗi record: 1 ingredient dùng trong 1 DailyMeal, với QuantityGram cho 1 suất
        var ingredientPerMeal = await (
            from dm in _context.DailyMeals
            join mfi in _context.MenuFoodItems
                on dm.DailyMealId equals mfi.DailyMealId
            join fii in _context.FoodItemIngredients
                on mfi.FoodId equals fii.FoodId
            where dm.ScheduleMealId == scheduleMealId
            select new
            {
                dm.MealDate,          // ngày bữa ăn
                fii.IngredientId,     // nguyên liệu
                fii.QuantityGram      // gram cho 1 suất ăn
            }
        ).ToListAsync(cancellationToken); // từ đây trở xuống là LINQ to Objects

        // 6. Tính tổng gram cho từng ingredient, đã nhân số HS tham gia (đã trừ vắng)
        var ingredientRequirements = ingredientPerMeal
            .GroupBy(x => x.IngredientId)
            .Select(g =>
            {
                decimal totalGram = 0;

                foreach (var x in g)
                {
                    // Số HS vắng ngày đó
                    absenceByDate.TryGetValue(x.MealDate, out var absentCount);

                    // Số HS tham gia bữa ăn
                    var participants = totalActiveStudents - absentCount;
                    if (participants < 0) participants = 0;

                    // QuantityGram là gram cho 1 học sinh → nhân số HS tham gia
                    totalGram += x.QuantityGram * participants;
                }

                return new
                {
                    IngredientId = g.Key,
                    RqQuanityGram = totalGram
                };
            })
            .ToList();

        // 7. Tạo header PurchasePlan
        var plan = new PurchasePlan
        {
            GeneratedAt = DateTime.UtcNow,
            PlanStatus = "Draft",
            ScheduleMealId = scheduleMealId,
            StaffId = staffId,
            AskToDelete = false
        };

        _context.PurchasePlans.Add(plan);
        await _context.SaveChangesAsync(cancellationToken); // để có PlanId

        // 8. Tạo lines từ kết quả đã tính
        var lines = ingredientRequirements.Select(x => new PurchasePlanLine
        {
            PlanId = plan.PlanId,
            IngredientId = x.IngredientId,
            RqQuanityGram = x.RqQuanityGram
        }).ToList();

        if (lines.Count > 0)
        {
            _context.PurchasePlanLines.AddRange(lines);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return plan.PlanId;
    }

    // ------------------- UpdatePlanWithLinesAsync -------------------
    public async Task UpdatePlanWithLinesAsync(
        int planId,
        string planStatus,
        Guid? confirmedBy,
        List<UpdatePurchasePlanLineDto> lines,
        CancellationToken cancellationToken)
    {
        var plan = await _context.PurchasePlans
            .FirstOrDefaultAsync(p => p.PlanId == planId, cancellationToken);

        if (plan == null)
            throw new InvalidOperationException("Purchase plan not found.");

        // update header
        plan.PlanStatus = planStatus;
        if (confirmedBy.HasValue && planStatus == "Confirmed")
        {
            plan.ConfirmedBy = confirmedBy;
            plan.ConfirmedAt = DateTime.UtcNow;
        }

        // lấy lines hiện tại
        var existingLines = await _context.PurchasePlanLines
            .Where(l => l.PlanId == planId)
            .ToListAsync(cancellationToken);

        // map theo IngredientId
        var existingDict = existingLines.ToDictionary(x => x.IngredientId);

        // update + add
        foreach (var dto in lines)
        {
            if (existingDict.TryGetValue(dto.IngredientId, out var line))
            {
                // update quantity
                line.RqQuanityGram = dto.RqQuanityGram;
            }
            else
            {
                // add mới
                var newLine = new PurchasePlanLine
                {
                    PlanId = planId,
                    IngredientId = dto.IngredientId,
                    RqQuanityGram = dto.RqQuanityGram
                };
                _context.PurchasePlanLines.Add(newLine);
            }
        }

        // remove những line không còn trong danh sách mới
        var newIngredientIds = lines.Select(x => x.IngredientId).ToHashSet();
        var toRemove = existingLines
            .Where(x => !newIngredientIds.Contains(x.IngredientId))
            .ToList();

        if (toRemove.Count > 0)
            _context.PurchasePlanLines.RemoveRange(toRemove);

        await _context.SaveChangesAsync(cancellationToken);
    }

    // ------------------- DeletePlanAsync -------------------
    public async Task DeletePlanAsync(
        int planId,
        CancellationToken cancellationToken)
    {
        // check dependency: có PurchaseOrders dùng plan này không?
        var hasOrders = await _context.PurchaseOrders
            .AnyAsync(o => o.PlanId == planId, cancellationToken);

        if (hasOrders)
        {
            throw new InvalidOperationException("Cannot delete plan because there are purchase orders referencing it.");
        }

        var plan = await _context.PurchasePlans
            .FirstOrDefaultAsync(p => p.PlanId == planId, cancellationToken);

        if (plan == null)
            return;

        var lines = await _context.PurchasePlanLines
            .Where(l => l.PlanId == planId)
            .ToListAsync(cancellationToken);

        if (lines.Count > 0)
        {
            _context.PurchasePlanLines.RemoveRange(lines);
        }

        _context.PurchasePlans.Remove(plan);
        await _context.SaveChangesAsync(cancellationToken);
    }
    

    // ------------------- NEW: GetPlansForSchoolAsync -------------------
    public async Task<List<PurchasePlanListItemDto>> GetPlansForSchoolAsync(
        Guid schoolId,
        bool includeDeleted,
        CancellationToken cancellationToken)
    {
        var query =
            from p in _context.PurchasePlans
            join sm in _context.ScheduleMeals
                on p.ScheduleMealId equals sm.ScheduleMealId
            join u in _context.Users
                on p.StaffId equals u.UserId
            where sm.SchoolId == schoolId
            select new { Plan = p, Schedule = sm, Staff = u };

        if (!includeDeleted)
        {
            query = query.Where(x => !x.Plan.AskToDelete);
        }

        var result = await query
            .OrderByDescending(x => x.Schedule.YearNo)
            .ThenByDescending(x => x.Schedule.WeekNo)
            .ThenByDescending(x => x.Plan.GeneratedAt)
            .Select(x => new PurchasePlanListItemDto
            {
                PlanId = x.Plan.PlanId,
                ScheduleMealId = (long)x.Plan.ScheduleMealId,
                GeneratedAt = x.Plan.GeneratedAt,
                PlanStatus = x.Plan.PlanStatus,
                AskToDelete = x.Plan.AskToDelete,

                WeekStart = x.Schedule.WeekStart,
                WeekEnd = x.Schedule.WeekEnd,
                WeekNo = x.Schedule.WeekNo,
                YearNo = x.Schedule.YearNo,

                StaffId = x.Plan.StaffId,
                StaffName = x.Staff.FullName
            })
            .ToListAsync(cancellationToken);

        return result;
    }

    // ------------------- NEW: GetPlanByDateAsync -------------------
    public async Task<PurchasePlanDto?> GetPlanByDateAsync(
    Guid schoolId,
    DateOnly nextWeekDate,
    CancellationToken cancellationToken)
    {
        // Lấy ngày bất kỳ của "tuần tới" so với date
        //var nextWeekDate = date.AddDays(7);

        // Tìm PlanId của tuần chứa "nextWeekDate"
        var planId = await (
            from p in _context.PurchasePlans
            join sm in _context.ScheduleMeals
                on p.ScheduleMealId equals sm.ScheduleMealId
            where sm.SchoolId == schoolId
                  && sm.WeekStart <= nextWeekDate
                  && sm.WeekEnd >= nextWeekDate
                  && !p.AskToDelete
            orderby sm.YearNo descending,
                    sm.WeekNo descending,
                    p.GeneratedAt descending
            select p.PlanId
        ).FirstOrDefaultAsync(cancellationToken);

        if (planId == 0)
        {
            return null;
        }

        // Tái sử dụng GetPlanDetailAsync để lấy đầy đủ header + lines
        return await GetPlanDetailAsync(planId, cancellationToken);
    }

    // ------------------- GetPlanDetailAsync -------------------
    public async Task<PurchasePlanDto?> GetPlanDetailAsync(
        int planId,
        CancellationToken cancellationToken)
    {
        // Header + Staff name
        var header = await (
            from p in _context.PurchasePlans
            join u in _context.Users
                on p.StaffId equals u.UserId
            where p.PlanId == planId
            select new
            {
                p.PlanId,
                p.ScheduleMealId,
                p.PlanStatus,
                p.GeneratedAt,
                p.StaffId,
                StaffName = u.FullName
            }
        ).FirstOrDefaultAsync(cancellationToken);

        if (header == null)
            return null;

        // Lines + IngredientName
        var lines = await (
            from l in _context.PurchasePlanLines
            join ing in _context.Ingredients
                on l.IngredientId equals ing.IngredientId
            where l.PlanId == planId
            orderby ing.IngredientName
            select new PurchasePlanLineDto
            {
                IngredientId = l.IngredientId,
                IngredientName = ing.IngredientName,
                RqQuanityGram = l.RqQuanityGram
            }
        ).ToListAsync(cancellationToken);

        return new PurchasePlanDto
        {
            PlanId = header.PlanId,
            ScheduleMealId = (long)header.ScheduleMealId,
            PlanStatus = header.PlanStatus,
            GeneratedAt = header.GeneratedAt,
            StaffId = header.StaffId,
            StaffName = header.StaffName,
            Lines = lines
        };
    }
    // ------------------- SoftDeletePlanAsync -------------------
    public async Task SoftDeletePlanAsync(
    int planId,
    CancellationToken cancellationToken)
    {
        var plan = await _context.PurchasePlans
            .FirstOrDefaultAsync(p => p.PlanId == planId, cancellationToken);

        if (plan == null)
        {
            // Không tìm thấy thì thôi, không cần throw
            return;
        }

        // Nếu đã được đánh dấu trước đó thì thôi
        if (plan.AskToDelete)
            return;

        plan.AskToDelete = true;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task<PurchasePlan?> GetByIdAsync(int planId, CancellationToken ct = default)
    {
        return _context.PurchasePlans
            .FirstOrDefaultAsync(p => p.PlanId == planId, ct);
    }

    public async Task<IReadOnlyList<PurchasePlanLine>> GetLinesAsync(
        int planId,
        CancellationToken ct = default)
    {
        return await _context.PurchasePlanLines
            .Where(l => l.PlanId == planId)
            .ToListAsync(ct);
    }

    public async Task<Guid> GetSchoolIdAsync(int planId, CancellationToken ct = default)
    {
        // Plan -> ScheduleMeal -> SchoolId
        var query =
            from p in _context.PurchasePlans
            join s in _context.ScheduleMeals
                on p.ScheduleMealId equals s.ScheduleMealId
            where p.PlanId == planId
            select s.SchoolId;

        var schoolId = await query.SingleOrDefaultAsync(ct);

        if (schoolId == Guid.Empty)
            throw new InvalidOperationException($"Cannot resolve SchoolId for Plan {planId}");

        return schoolId;
    }
}
