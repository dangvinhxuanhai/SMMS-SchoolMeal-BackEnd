using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.Wardens.Interfaces;
using SMMS.Domain.Entities.auth;
using SMMS.Domain.Entities.billing;
using SMMS.Domain.Entities.nutrition;
using SMMS.Domain.Entities.school;
using SMMS.Persistence.Data;

namespace SMMS.Persistence.Repositories.Wardens;
public class WardensRepository : IWardensRepository
{
    private readonly EduMealContext _context;

    public WardensRepository(EduMealContext context)
    {
        _context = context;
    }

    public IQueryable<Class> Classes => _context.Classes;
    public IQueryable<Teacher> Teachers => _context.Teachers;
    public IQueryable<User> Users => _context.Users;
    public IQueryable<StudentClass> StudentClasses => _context.StudentClasses;
    public IQueryable<Student> Students => _context.Students;
    public IQueryable<Attendance> Attendances => _context.Attendances;
    public IQueryable<StudentAllergen> StudentAllergens => _context.StudentAllergens;
    public IQueryable<Allergen> Allergens => _context.Allergens;
    public IQueryable<StudentHealthRecord> StudentHealthRecords => _context.StudentHealthRecords;
    public IQueryable<NotificationRecipient> NotificationRecipients => _context.NotificationRecipients;
    public IQueryable<Notification> Notifications => _context.Notifications;
}
