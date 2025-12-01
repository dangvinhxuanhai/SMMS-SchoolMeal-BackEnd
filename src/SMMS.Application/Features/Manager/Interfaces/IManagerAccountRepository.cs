using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Domain.Entities.auth;
using SMMS.Domain.Entities.school;

namespace SMMS.Application.Features.Manager.Interfaces;
public interface IManagerAccountRepository
{
    IQueryable<User> Users { get; }
    IQueryable<Role> Roles { get; }
    IQueryable<Student> Students { get; }

    Task<User?> GetByIdAsync(Guid userId);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(User user);
    Task AddStudentAsync(Student student);
    Task AddStudentClassAsync(StudentClass studentClass);
    Task UpdateStudentAsync(Student student);
    Task DeleteStudentAsync(Student student);
    Task DeleteStudentClassAsync(StudentClass studentClass);
    Task AddTeacherAsync(Teacher teacher);
    Task DeleteNotificationRecipientsByUserIdAsync(Guid userId);
    Task MarkAllNotificationsAsReadAsync(Guid userId);
}
