using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using MediatR;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.Wardens.Commands;
using SMMS.Application.Features.Wardens.DTOs;
using SMMS.Application.Features.Wardens.Interfaces;
using SMMS.Application.Features.Wardens.Queries;
using SMMS.Domain.Entities.foodmenu;

namespace SMMS.Application.Features.Wardens.Handlers;
public class WardensFeedbackHandler :
    IRequestHandler<GetWardenFeedbacksQuery, IEnumerable<FeedbackDto>>,
    IRequestHandler<CreateWardenFeedbackCommand, FeedbackDto>,
    IRequestHandler<UpdateWardenFeedbackCommand, FeedbackDto>,  // 🆕
    IRequestHandler<DeleteWardenFeedbackCommand, bool>
{
    private readonly IWardensFeedbackRepository _repo;

    public WardensFeedbackHandler(IWardensFeedbackRepository repo)
    {
        _repo = repo;
    }

    // 🟢 Lấy danh sách feedback của giám thị
    public async Task<IEnumerable<FeedbackDto>> Handle(
        GetWardenFeedbacksQuery request,
        CancellationToken cancellationToken)
    {
        var wardenId = request.WardenId;

        // Lấy tên giám thị
        var sender = await _repo.Users
            .Where(u => u.UserId == wardenId)
            .Select(u => u.FullName)
            .FirstOrDefaultAsync(cancellationToken);

        if (sender == null)
            throw new ArgumentException("Không tìm thấy giám thị trong hệ thống.");

        // Lớp hiện tại giám thị phụ trách (năm học mới nhất)
        var currentClass = await (
            from c in _repo.Classes
            join t in _repo.Teachers on c.TeacherId equals t.TeacherId
            join u in _repo.Users on t.TeacherId equals u.UserId
            join y in _repo.AcademicYears on c.YearId equals y.YearId
            where t.TeacherId == wardenId
            orderby y.BoardingEndDate descending
            select new
            {
                c.ClassName,
                TeacherName = u.FullName,
                y.BoardingStartDate,
                y.BoardingEndDate
            }
        ).FirstOrDefaultAsync(cancellationToken);

        string className = currentClass?.ClassName ?? "Không xác định";
        string teacherName = currentClass?.TeacherName ?? "N/A";

        // Feedbacks của giám thị
        var feedbacks = await _repo.Feedbacks
            .Where(f => f.SenderId == wardenId)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new FeedbackDto
            {
                FeedbackId = f.FeedbackId,
                Title = $"{className} - {teacherName} - {f.CreatedAt:dd/MM/yyyy}",
                SenderName = sender,
                Content = f.Content,
                TargetRef = f.TargetRef,
                TargetType = f.TargetType,
                CreatedAt = f.CreatedAt,
                DailyMealId = f.DailyMealId
            })
            .ToListAsync(cancellationToken);

        return feedbacks;
    }

    // 🟡 Tạo mới feedback
    public async Task<FeedbackDto> Handle(
        CreateWardenFeedbackCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;

        if (string.IsNullOrWhiteSpace(request.Content))
            throw new ArgumentException("Nội dung phản hồi không được để trống.");

        // Kiểm tra giám thị
        var sender = await _repo.Users
            .Where(u => u.UserId == request.SenderId)
            .Select(u => new { u.UserId, u.FullName })
            .FirstOrDefaultAsync(cancellationToken);

        if (sender == null)
            throw new ArgumentException("Giám thị không tồn tại trong hệ thống.");

        // Lớp mà giám thị đang phụ trách (năm học mới nhất)
        var currentClass = await (
            from c in _repo.Classes
            join t in _repo.Teachers on c.TeacherId equals t.TeacherId
            join u in _repo.Users on t.TeacherId equals u.UserId
            join y in _repo.AcademicYears on c.YearId equals y.YearId
            where t.TeacherId == request.SenderId
            orderby y.BoardingEndDate descending
            select new
            {
                c.ClassName,
                TeacherName = u.FullName,
                y.BoardingStartDate,
                y.BoardingEndDate
            }
        ).FirstOrDefaultAsync(cancellationToken);

        string className = currentClass?.ClassName ?? "Không xác định";
        string teacherName = currentClass?.TeacherName ?? sender.FullName;
        string dateNow = DateTime.UtcNow.ToString("dd/MM/yyyy");

        // Sinh tiêu đề
        string title = $"{className} - {teacherName} - {dateNow}";

        // Xác nhận daily meal (nếu có)
        if (request.DailyMealId.HasValue)
        {
            bool mealExists = await _repo.DailyMeals
                .AnyAsync(m => m.DailyMealId == request.DailyMealId, cancellationToken);

            if (!mealExists)
                throw new ArgumentException("Không tìm thấy bữa ăn để phản hồi.");
        }

        // Tạo feedback
        var feedback = new Feedback
        {
            SenderId = request.SenderId,
            TargetType = "KitchenStaff",   // theo code cũ: cố định KitchenStaff
            TargetRef = request.TargetRef,
            Content = request.Content.Trim(),
            DailyMealId = request.DailyMealId,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddFeedbackAsync(feedback);
        await _repo.SaveChangesAsync();

        return new FeedbackDto
        {
            FeedbackId = feedback.FeedbackId,
            Title = title,
            SenderName = sender.FullName,
            Content = feedback.Content,
            TargetRef = feedback.TargetRef,
            TargetType = feedback.TargetType,
            CreatedAt = feedback.CreatedAt,
            DailyMealId = feedback.DailyMealId
        };
    }

    // 🟠 Cập nhật feedback
    public async Task<FeedbackDto> Handle(
        UpdateWardenFeedbackCommand command,
        CancellationToken cancellationToken){
        var request = command.Request;

        if (string.IsNullOrWhiteSpace(request.Content))
            throw new ArgumentException("Nội dung phản hồi không được để trống.");

        // Kiểm tra giám thị
        var sender = await _repo.Users
            .Where(u => u.UserId == request.SenderId)
            .Select(u => new { u.UserId, u.FullName })
            .FirstOrDefaultAsync(cancellationToken);

        if (sender == null)
            throw new ArgumentException("Giám thị không tồn tại trong hệ thống.");

        // Lớp mà giám thị đang phụ trách
        var currentClass = await (
            from c in _repo.Classes
            join t in _repo.Teachers on c.TeacherId equals t.TeacherId
            join u in _repo.Users on t.TeacherId equals u.UserId
            join y in _repo.AcademicYears on c.YearId equals y.YearId
            where t.TeacherId == request.SenderId
            orderby y.BoardingEndDate descending
            select new
            {
                c.ClassName,
                TeacherName = u.FullName,
                y.BoardingStartDate,
                y.BoardingEndDate
            }
        ).FirstOrDefaultAsync(cancellationToken);

        string className = currentClass?.ClassName ?? "Không xác định";
        string teacherName = currentClass?.TeacherName ?? sender.FullName;
        string dateNow = DateTime.UtcNow.ToString("dd/MM/yyyy");

        string title = $"{className} - {teacherName} - {dateNow}";

        if (request.DailyMealId.HasValue)
        {
            bool mealExists = await _repo.DailyMeals
                .AnyAsync(m => m.DailyMealId == request.DailyMealId, cancellationToken);

            if (!mealExists)
                throw new ArgumentException("Không tìm thấy bữa ăn để phản hồi.");
        }

        // 🛠️ FIX: Map TargetType từ Frontend sang Role DB
        string dbTargetType;
        string reqType = request.TargetType?.ToLower()?.Trim() ?? "";

        switch (reqType)
        {
            case "food":
                dbTargetType = "KitchenStaff";
                break;
            case "facility":
                dbTargetType = "FacilityManager"; // Role quản lý CSVC
                break;
            case "health":
                dbTargetType = "MedicalStaff";    // Role y tế
                break;
            case "activity":
                dbTargetType = "ActivityManager"; // Role hoạt động (hoặc Admin)
                break;
            default:
                dbTargetType = "Admin";           // Mặc định gửi Admin nếu không khớp
                break;
        }

        var feedback = new Feedback
        {
            SenderId = request.SenderId,
            TargetType = dbTargetType, // ✅ Đã sửa: dùng biến đã map, không gán cứng
            TargetRef = request.TargetRef,
            Content = request.Content.Trim(),
            DailyMealId = request.DailyMealId,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddFeedbackAsync(feedback);
        await _repo.SaveChangesAsync();

        return new FeedbackDto
        {
            FeedbackId = feedback.FeedbackId,
            Title = title,
            SenderName = sender.FullName,
            Content = feedback.Content,
            TargetRef = feedback.TargetRef,
            TargetType = feedback.TargetType, // Trả về đúng loại đã lưu
            CreatedAt = feedback.CreatedAt,
            DailyMealId = feedback.DailyMealId
        };
    }


    // ❌ Xoá feedback
    public async Task<bool> Handle(
        DeleteWardenFeedbackCommand command,
        CancellationToken cancellationToken)
    {
        var feedback = await _repo.GetFeedbackByIdAsync(command.FeedbackId);
        if (feedback == null)
            return false;

        // Chỉ cho phép giám thị chủ feedback xoá
        if (feedback.SenderId != command.WardenId)
            throw new InvalidOperationException("Bạn không có quyền xoá phản hồi này.");

        await _repo.DeleteFeedbackAsync(feedback);
        await _repo.SaveChangesAsync();

        return true;
    }
}
