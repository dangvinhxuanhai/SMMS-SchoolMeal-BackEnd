using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SMMS.Application.Features.Manager.DTOs;

namespace SMMS.Application.Features.Manager.Interfaces;
public interface IManagerParentService
{
    Task<List<ParentAccountDto>> GetAllAsync(Guid schoolId, Guid? classId = null);
    Task<AccountDto> CreateAsync(CreateParentRequest request);
    Task<AccountDto?> UpdateAsync(Guid userId, UpdateParentRequest request);
    Task<bool> ChangeStatusAsync(Guid userId, bool isActive);
    Task<bool> DeleteAsync(Guid userId);
    Task<List<ParentAccountDto>> SearchAsync(Guid schoolId, string keyword);
    Task<List<AccountDto>> ImportFromExcelAsync(Guid schoolId, IFormFile file, string createdBy);
    Task<byte[]> GetExcelTemplateAsync();
}
