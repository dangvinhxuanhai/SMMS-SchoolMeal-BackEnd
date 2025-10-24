using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.Manager.DTOs;

namespace SMMS.Application.Features.Manager.Interfaces;
public interface IManagerAccountService
{
    Task<List<AccountDto>> GetAllAsync(Guid schoolId);

    Task<AccountDto> CreateAsync(CreateAccountRequest request);

    Task<AccountDto?> UpdateAsync(Guid userId, UpdateAccountRequest request);
    Task<bool> ChangeStatusAsync(Guid userId, bool isActive);

    Task<bool> DeleteAsync(Guid userId);
    Task<List<AccountDto>> SearchAccountsAsync(Guid schoolId, string keyword);
    Task<List<AccountDto>> FilterByRoleAsync(Guid schoolId, string role);


}
