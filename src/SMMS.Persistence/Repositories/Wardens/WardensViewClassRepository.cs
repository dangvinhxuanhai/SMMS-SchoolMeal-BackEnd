using System.Collections.Generic;
using System.Threading.Tasks;
using SMMS.Application.Features.Wardens.Interfaces;

namespace SMMS.Persistence.Repositories.Wardens
{
    public class WardensViewClassRepository : IWardensViewClassRepository
    {
        public Task<IEnumerable<object>> GetClassesAsync(int wardenId)
        {
            // TODO: Implement with actual DbContext
            return Task.FromResult<IEnumerable<object>>(new List<object>());
        }

        public Task<IEnumerable<object>> GetStudentsInClassAsync(int classId)
        {
            // TODO: Implement with actual DbContext
            return Task.FromResult<IEnumerable<object>>(new List<object>());
        }

        public Task<object?> GetStudentDetailsAsync(int studentId)
        {
            // TODO: Implement with actual DbContext
            return Task.FromResult<object?>(null);
        }
    }
}



