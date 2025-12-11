using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.Manager.Interfaces;
using SMMS.Domain.Entities.auth;
using SMMS.Domain.Entities.school;
using SMMS.Persistence.Data;

namespace SMMS.Persistence.Repositories.Manager;

public class ManagerAccountRepository : IManagerAccountRepository
{
    private readonly EduMealContext _context;

    public ManagerAccountRepository(EduMealContext context)
    {
        _context = context;
    }

    public IQueryable<User> Users => _context.Users.AsNoTracking();
    public IQueryable<Role> Roles => _context.Roles.AsNoTracking();
    public IQueryable<Student> Students => _context.Students;

    public async Task<User?> GetByIdAsync(Guid userId)
    {
        return await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(User user)
    {
        var userId = user.UserId;
        // 1. Xoá refresh tokens của user
        var refreshTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == user.UserId)
            .ToListAsync();

        if (refreshTokens.Count > 0)
        {
            _context.RefreshTokens.RemoveRange(refreshTokens);
        }

        // 2. Gỡ user khỏi Menus.ConfirmedBy
        var menusConfirmedByUser = await _context.Menus
            .Where(m => m.ConfirmedBy == userId)
            .ToListAsync();

        foreach (var menu in menusConfirmedByUser)
        {
            // chọn 1 trong 2:
            menu.ConfirmedBy = null; // nếu cho phép null
            // hoặc menu.ConfirmedBy = someOtherUserId;  // nếu bạn muốn gán cho user khác
        }

        _context.Menus.UpdateRange(menusConfirmedByUser);

        // 3. Gỡ CreatedBy ở ScheduleMeal
        var schedulesCreatedByUser = await _context.ScheduleMeals
            .Where(sm => sm.CreatedBy == userId)
            .ToListAsync();

        foreach (var sm in schedulesCreatedByUser)
        {
            sm.CreatedBy = null; // hoặc gán user khác
        }

        _context.ScheduleMeals.UpdateRange(schedulesCreatedByUser);

        // 2. Xoá teacher nếu có
        var teacher = await _context.Teachers
            .Include(t => t.TeacherNavigation)
            .FirstOrDefaultAsync(t => t.TeacherNavigation.UserId == user.UserId);

        if (teacher != null)
        {
            var classesOfTeacher = await _context.Classes
                .Where(c => c.TeacherId == teacher.TeacherId)
                .ToListAsync();

            foreach (var cls in classesOfTeacher)
            {
                cls.TeacherId = null;
            }

            _context.Classes.UpdateRange(classesOfTeacher);
            _context.Teachers.Remove(teacher);
        }

        // 3. Xoá user cuối cùng
        _context.Users.Remove(user);

        await _context.SaveChangesAsync();
    }

    public async Task AddStudentAsync(Student student)
    {
        _context.Students.Add(student);
        await _context.SaveChangesAsync();
    }

    public async Task AddStudentClassAsync(StudentClass studentClass)
    {
        _context.StudentClasses.Add(studentClass);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateStudentAsync(Student student)
    {
        _context.Students.Update(student);
        await _context.SaveChangesAsync();
    }

    public async Task MarkAllNotificationsAsReadAsync(Guid userId)
    {
        await _context.NotificationRecipients
            .Where(nr => nr.UserId == userId && !nr.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(
                    nr => nr.IsRead, true)
                .SetProperty(nr => nr.ReadAt, DateTime.UtcNow));
    }

    public async Task DeleteStudentAsync(Student student)
    {
        var studentId = student.StudentId;
        // 1. Xoá health records của student
        var healthRecords = await _context.StudentHealthRecords
            .Where(x => x.StudentId == student.StudentId)
            .ToListAsync();

        if (healthRecords.Count > 0)
        {
            _context.StudentHealthRecords.RemoveRange(healthRecords);
        }

        // 2. Xoá allergens của student
        var allergenRecords = await _context.StudentAllergens
            .Where(a => a.StudentId == student.StudentId)
            .ToListAsync();

        if (allergenRecords.Count > 0)
        {
            _context.StudentAllergens.RemoveRange(allergenRecords);
        }

        // 3. Xoá images
        var imageRecords = await _context.StudentImages
            .Where(i => i.StudentId == studentId)
            .ToListAsync();

        if (imageRecords.Count > 0)
        {
            _context.StudentImages.RemoveRange(imageRecords);
        }

        // 4. Xử lý billing
        // 4.1. Lấy danh sách invoiceId của student này
        var invoiceIds = await _context.Invoices
            .Where(inv => inv.StudentId == studentId)
            .Select(inv => inv.InvoiceId) // đúng tên key của bạn
            .ToListAsync();

        if (invoiceIds.Any())
        {
            // 4.2. Xoá payments trước
            var payments = await _context.Payments
                .Where(p => invoiceIds.Contains(p.InvoiceId))
                .ToListAsync();
            _context.Payments.RemoveRange(payments);

            // 4.3. Rồi xoá invoices
            var invoices = await _context.Invoices
                .Where(inv => invoiceIds.Contains(inv.InvoiceId))
                .ToListAsync();
            _context.Invoices.RemoveRange(invoices);
        }

        // 5. Xoá attendance
        var attendances = await _context.Attendances // hoặc Attendances, tuỳ DbSet
            .Where(a => a.StudentId == studentId)
            .ToListAsync();
        _context.Attendances.RemoveRange(attendances);

        // 6. Xoá StudentClasses (cái này đang gây lỗi)
        var studentClasses = await _context.StudentClasses
            .Where(sc => sc.StudentId == studentId)
            .ToListAsync();
        if (studentClasses.Count > 0)
        {
            _context.StudentClasses.RemoveRange(studentClasses);
        }
        // 3. Xoá student
        _context.Students.Remove(student);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteStudentClassAsync(StudentClass studentClass)
    {
        _context.StudentClasses.Remove(studentClass);
        await _context.SaveChangesAsync();
    }

    public async Task AddTeacherAsync(Teacher teacher)
    {
        _context.Teachers.Add(teacher);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteNotificationRecipientsByUserIdAsync(Guid userId)
    {
        var recipients = _context.NotificationRecipients.Where(n => n.UserId == userId);
        _context.NotificationRecipients.RemoveRange(recipients);
        await _context.SaveChangesAsync();
    }
}
