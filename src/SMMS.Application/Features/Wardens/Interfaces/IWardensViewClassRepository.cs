using System.Threading.Tasks;
using System.Collections.Generic;

namespace SMMS.Application.Features.Wardens.Interfaces;
    public interface IWardensViewClassRepository
    {
        Task<IEnumerable<object>> GetClassesAsync(int wardenId);
        Task<IEnumerable<object>> GetStudentsInClassAsync(int classId);
        Task<object?> GetStudentDetailsAsync(int studentId);
    }



