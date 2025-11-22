using System.Threading.Tasks;
using System.Collections.Generic;

namespace SMMS.Persistence.Repositories.Wardens.Interfaces
{
    public interface IWardensHealthRepository
    {
        Task<object> GetHealthSummaryAsync(int wardenId);
        Task<IEnumerable<object>> GetStudentsHealthAsync(int classId);
        Task<bool> UpdateStudentHealthAsync(int studentId, object healthUpdateDto);
    }
}



