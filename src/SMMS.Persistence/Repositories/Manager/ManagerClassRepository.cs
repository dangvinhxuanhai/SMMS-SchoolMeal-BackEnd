using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.Manager.Interfaces;
using SMMS.Domain.Entities.school;
using SMMS.Persistence.Data;
using Microsoft.EntityFrameworkCore;
namespace SMMS.Persistence.Repositories.Manager;
public class ManagerClassRepository : IManagerClassRepository
{
    private readonly EduMealContext _context;

    public ManagerClassRepository(EduMealContext context)
    {
        _context = context;
    }

    // 🧱 Truy cập trực tiếp bảng Classes
    public IQueryable<AcademicYear> AcademicYears => _context.AcademicYears;
    public IQueryable<Class> Classes => _context.Classes.AsQueryable();
    public IQueryable<Teacher> Teachers => _context.Teachers.AsQueryable();
    // 🔹 Lấy thông tin lớp theo ID
    public async Task<Class?> GetByIdAsync(Guid classId)

    {
        return await _context.Classes
            .Include(c => c.School)
            .Include(c => c.Teacher)
            .Include(c => c.Year)
            .FirstOrDefaultAsync(c => c.ClassId == classId);
    }

    // 🔹 Tạo mới
    public async Task AddAsync(Class entity)
    {
        await _context.Classes.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    // 🔹 Cập nhật
    public async Task UpdateAsync(Class entity)
    {
        _context.Classes.Update(entity);
        await _context.SaveChangesAsync();
    }

    // 🔹 Xóa
    public async Task DeleteAsync(Class entity)
    {
        _context.Classes.Remove(entity);
        await _context.SaveChangesAsync();
    }
}
