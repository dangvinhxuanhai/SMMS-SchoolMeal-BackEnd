using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.Manager.DTOs;

namespace SMMS.Application.Features.Manager.Interfaces;
public interface IManagerClassService
{
    Task<List<ClassDto>> GetAllAsync(Guid schoolId);
    Task<ClassDto> CreateAsync(CreateClassRequest request);
    Task<ClassDto?> UpdateAsync(Guid classId, UpdateClassRequest request);
    Task<bool> DeleteAsync(Guid classId);
    Task<object> GetTeacherAssignmentStatusAsync(Guid schoolId);

}
