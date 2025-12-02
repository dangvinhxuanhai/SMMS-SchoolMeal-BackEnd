using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Domain.Entities.auth;
using SMMS.Domain.Entities.foodmenu;
using SMMS.Domain.Entities.school;

namespace SMMS.Application.Features.Wardens.Interfaces;
public interface IWardensFeedbackRepository
{
    IQueryable<User> Users { get; }
    IQueryable<Class> Classes { get; }
    IQueryable<Teacher> Teachers { get; }
    IQueryable<AcademicYear> AcademicYears { get; }
    IQueryable<DailyMeal> DailyMeals { get; }
    IQueryable<Feedback> Feedbacks { get; }

    Task AddFeedbackAsync(Feedback feedback);
    Task<Feedback?> GetFeedbackByIdAsync(int feedbackId);   // 🆕
    Task DeleteFeedbackAsync(Feedback feedback);
    Task<int> SaveChangesAsync();
}
