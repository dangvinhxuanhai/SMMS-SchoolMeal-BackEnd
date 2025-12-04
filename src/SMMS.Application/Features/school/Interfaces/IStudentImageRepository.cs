using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.school.DTOs;
using SMMS.Domain.Entities.school;

namespace SMMS.Application.Features.school.Interfaces
{
    public interface IStudentImageRepository
    {
        Task<List<StudentImage>> GetImagesByStudentIdAsync(Guid studentId);
    }
}
