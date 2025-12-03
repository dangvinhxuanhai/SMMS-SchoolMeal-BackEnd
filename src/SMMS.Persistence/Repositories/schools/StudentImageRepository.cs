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
            return await _db.StudentImages
                .Where(img => img.StudentId == studentId)
                .OrderByDescending(img => img.TakenAt)
                .ToListAsync();
        }
    }
}
