using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.school.Interfaces;
public interface IClassStudentRepository
{
    /// <summary>
    /// Đếm số học sinh đang học trong 1 lớp tại ngày hiện tại.
    /// Dựa vào StudentClasses (LeftDate null hoặc > today, RegistStatus = true).
    /// </summary>
    Task<int> CountActiveStudentsAsync(Guid classId, CancellationToken ct = default);
}
