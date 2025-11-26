using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Domain.Entities.auth;
using SMMS.Domain.Entities.billing;
using SMMS.Domain.Entities.nutrition;
using SMMS.Domain.Entities.school;

namespace SMMS.Application.Features.Wardens.Interfaces;
public interface IWardensRepository
{
    IQueryable<Class> Classes { get; }
    IQueryable<Teacher> Teachers { get; }
    IQueryable<User> Users { get; }
    IQueryable<StudentClass> StudentClasses { get; }
    IQueryable<Student> Students { get; }
    IQueryable<Attendance> Attendances { get; }
    IQueryable<StudentAllergen> StudentAllergens { get; }
    IQueryable<Allergen> Allergens { get; }
    IQueryable<StudentHealthRecord> StudentHealthRecords { get; }
    IQueryable<NotificationRecipient> NotificationRecipients { get; }
    IQueryable<Notification> Notifications { get; }
}
