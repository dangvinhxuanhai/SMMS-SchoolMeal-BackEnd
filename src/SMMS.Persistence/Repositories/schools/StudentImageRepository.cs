using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.school.DTOs;
using SMMS.Application.Features.school.Interfaces;
using SMMS.Domain.Entities.school;
using SMMS.Persistence.Data;

namespace SMMS.Persistence.Repositories.schools
{
    public class StudentImageRepository : IStudentImageRepository
    {
        private readonly EduMealContext _db;

        public StudentImageRepository(EduMealContext db)
        {
            _db = db;
        }

        public async Task<List<StudentImage>> GetImagesByStudentIdAsync(Guid studentId)
        {
            var classId = await _db.StudentClasses
            .Where(s => s.StudentId == studentId)
            .Select(s => s.ClassId)
            .FirstOrDefaultAsync();
            // 2. Lấy toàn bộ StudentId thuộc class đó
            var studentIds = await _db.StudentClasses
                .Where(s => s.ClassId == classId)
                .Select(s => s.StudentId)
                .ToListAsync();

            // 3. Lấy ảnh của tất cả student trong class đó
            return await _db.StudentImages
                .Where(img => studentIds.Contains(img.StudentId))
                .OrderByDescending(img => img.TakenAt)
                .ToListAsync();
        }
    }
}
