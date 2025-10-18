using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.Wardens.DTOs;
using SMMS.Application.Features.Wardens.Interfaces;
using SMMS.Domain.Entities.foodmenu;
using SMMS.Persistence.Dbcontext;
using Microsoft.EntityFrameworkCore;


namespace SMMS.Persistence.Repositories.Wardens;

public class WardensFeedbackService : IWardensFeedbackService
{
    private readonly EduMealContext _context;

    public WardensFeedbackService(EduMealContext context)
    {
        _context = context;
    }

    // 🟢 Lấy danh sách feedback của giám thị
    public async Task<IEnumerable<FeedbackDto>> GetFeedbacksByWardenAsync(Guid wardenId)
    {
        // Lấy thông tin giám thị (Sender)
        var sender = await _context.Users
            .Where(u => u.UserId == wardenId)
            .Select(u => u.FullName)
            .FirstOrDefaultAsync();

        if (sender == null)
            throw new ArgumentException("Không tìm thấy giám thị trong hệ thống.");

        // Lấy lớp hiện tại mà giám thị đang phụ trách
        var currentClass = await (
            from c in _context.Classes
            join t in _context.Teachers on c.TeacherId equals t.TeacherId
            join u in _context.Users on t.TeacherId equals u.UserId
            join y in _context.AcademicYears on c.YearId equals y.YearId
            where t.TeacherId == wardenId
            orderby y.BoardingEndDate descending
            select new
            {
                c.ClassName,
                TeacherName = u.FullName,
                y.BoardingStartDate,
                y.BoardingEndDate
            }
        ).FirstOrDefaultAsync();

        string className = currentClass?.ClassName ?? "Không xác định";
        string teacherName = currentClass?.TeacherName ?? "N/A";

        // Lấy danh sách feedback
        var feedbacks = await _context.Feedbacks
            .Where(f => f.SenderId == wardenId)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new FeedbackDto
            {
                FeedbackId = f.FeedbackId,
                // Ghép tiêu đề: [ClassName] + [TeacherName] + [Date]
                Title = $"{className} - {teacherName} - {f.CreatedAt:dd/MM/yyyy}",
                SenderName = sender,
                Content = f.Content,
                TargetRef = f.TargetRef,
                TargetType = f.TargetType,
                CreatedAt = f.CreatedAt,
                DailyMealId = f.DailyMealId
            })
            .ToListAsync();

        return feedbacks;
    }


    // 🟡 Tạo mới feedback gửi tới kitchen staff
    public async Task<FeedbackDto> CreateFeedbackAsync(CreateFeedbackRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            throw new ArgumentException("Nội dung phản hồi không được để trống.");

        // 🔹 Kiểm tra người gửi (giám thị)
        var sender = await _context.Users
            .Where(u => u.UserId == request.SenderId)
            .Select(u => new { u.UserId, u.FullName })
            .FirstOrDefaultAsync();

        if (sender == null)
            throw new ArgumentException("Giám thị không tồn tại trong hệ thống.");

        // 🔹 Xác định lớp mà giám thị đang phụ trách (theo năm học mới nhất)
        var currentClass = await (
            from c in _context.Classes
            join t in _context.Teachers on c.TeacherId equals t.TeacherId
            join u in _context.Users on t.TeacherId equals u.UserId
            join y in _context.AcademicYears on c.YearId equals y.YearId
            where t.TeacherId == request.SenderId
            orderby y.BoardingEndDate descending
            select new
            {
                c.ClassName,
                TeacherName = u.FullName,
                y.BoardingStartDate,
                y.BoardingEndDate
            }
            ).FirstOrDefaultAsync();

        string className = currentClass?.ClassName ?? "Không xác định";
        string teacherName = currentClass?.TeacherName ?? sender.FullName;
        string dateNow = DateTime.UtcNow.ToString("dd/MM/yyyy");

        // 🔹 Sinh tiêu đề tự động
        string title = $"{className} - {teacherName} - {dateNow}";

        // 🔹 Xác nhận bữa ăn nếu có
        if (request.DailyMealId.HasValue)
        {
            bool mealExists = await _context.DailyMeals
                .AnyAsync(m => m.DailyMealId == request.DailyMealId);
            if (!mealExists)
                throw new ArgumentException("Không tìm thấy bữa ăn để phản hồi.");
        }

        // 🟩 Tạo bản ghi feedback
        var feedback = new Feedback
        {
            SenderId = request.SenderId,
            TargetType = "Kitchen",                 // 🔹 Cố định, không còn kiểm tra
            TargetRef = request.TargetRef,          // Có thể null, hoặc ghi chú tên học sinh
            Content = request.Content.Trim(),
            DailyMealId = request.DailyMealId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Feedbacks.Add(feedback);
        await _context.SaveChangesAsync();

        // 🟢 Trả về DTO
        return new FeedbackDto
        {
            FeedbackId = feedback.FeedbackId,
            Title = title,
            SenderName = sender.FullName,
            Content = feedback.Content,
            TargetRef = feedback.TargetRef,
            CreatedAt = feedback.CreatedAt,
            DailyMealId = feedback.DailyMealId
        };
    }

}

