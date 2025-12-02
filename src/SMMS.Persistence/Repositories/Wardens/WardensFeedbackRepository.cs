using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.Wardens.Interfaces;
using SMMS.Domain.Entities.auth;
using SMMS.Domain.Entities.foodmenu;
using SMMS.Domain.Entities.school;
using SMMS.Persistence.Data;

namespace SMMS.Persistence.Repositories.Wardens;
public class WardensFeedbackRepository : IWardensFeedbackRepository
{
    private readonly EduMealContext _context;

    public WardensFeedbackRepository(EduMealContext context)
    {
        _context = context;
    }

    public IQueryable<User> Users => _context.Users;
    public IQueryable<Class> Classes => _context.Classes;
    public IQueryable<Teacher> Teachers => _context.Teachers;
    public IQueryable<AcademicYear> AcademicYears => _context.AcademicYears;
    public IQueryable<DailyMeal> DailyMeals => _context.DailyMeals;
    public IQueryable<Feedback> Feedbacks => _context.Feedbacks;

    public async Task AddFeedbackAsync(Feedback feedback)
    {
        _context.Feedbacks.Add(feedback);
        await Task.CompletedTask;
    }
    // 🆕 Lấy 1 feedback theo Id
    public Task<Feedback?> GetFeedbackByIdAsync(int feedbackId)
        => _context.Feedbacks
            .FirstOrDefaultAsync(f => f.FeedbackId == feedbackId);

    // 🆕 Xoá feedback
    public Task DeleteFeedbackAsync(Feedback feedback)
    {
        _context.Feedbacks.Remove(feedback);
        return Task.CompletedTask;
    }
    public Task<int> SaveChangesAsync()
        => _context.SaveChangesAsync();
}
