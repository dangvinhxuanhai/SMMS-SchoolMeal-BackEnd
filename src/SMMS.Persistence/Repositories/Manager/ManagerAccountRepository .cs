using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.Manager.Interfaces;
using SMMS.Domain.Entities.auth;
using SMMS.Domain.Entities.school;
using SMMS.Persistence.Dbcontext;

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
        // 1. Tìm teacher gắn với user này
        var teacher = await _context.Teachers
            .Include(t => t.TeacherNavigation)
            .FirstOrDefaultAsync(t => t.TeacherNavigation.UserId == user.UserId);
        // 👆 chỗ này bạn sửa lại cho đúng:
        //   t.TeacherNavigation.Id == user.Id
        // hoặc t.TeacherNavigation.UserId == user.UserId
        // tuỳ theo model của bạn

        if (teacher != null)
        {
            // 2. Lấy tất cả Class đang dùng Teacher này
            var classesOfTeacher = await _context.Classes
                .Where(c => c.TeacherId == teacher.TeacherId)
                .ToListAsync();

            // 3. Gỡ teacher khỏi các lớp (không xoá lớp)
            foreach (var cls in classesOfTeacher)
            {
                cls.TeacherId = null;   // 👈 giữ lớp, chỉ bỏ giáo viên
            }

            // 4. Cập nhật lại các Class
            if (classesOfTeacher.Count > 0)
            {
                _context.Classes.UpdateRange(classesOfTeacher);
            }

            // 5. Xoá Teacher
            _context.Teachers.Remove(teacher);
        }

        // 6. Cuối cùng xoá User
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
    public async Task DeleteStudentAsync(Student student)
    {
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
}
