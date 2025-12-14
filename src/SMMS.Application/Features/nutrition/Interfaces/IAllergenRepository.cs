using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.nutrition.DTOs;
using SMMS.Application.Features.Skeleton.Interfaces;
using SMMS.Domain.Entities.nutrition;

namespace SMMS.Application.Features.nutrition.Interfaces;
public interface IAllergenRepository
{
    Task<List<AllergenDTO>> GetAllAsync(Guid studentId);
}
