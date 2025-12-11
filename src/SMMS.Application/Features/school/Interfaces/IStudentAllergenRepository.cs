using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.school.Interfaces;
public interface IStudentAllergenRepository
{
    /// <summary>
    /// Lấy danh sách AllergenId mà HỌC SINH trong trường đang được ghi nhận.
    /// Mỗi AllergenId xuất hiện tối đa 1 lần.
    /// </summary>
    Task<IReadOnlyList<int>> GetAllergenIdsForSchoolAsync(
        Guid schoolId,
        CancellationToken ct = default);
}
