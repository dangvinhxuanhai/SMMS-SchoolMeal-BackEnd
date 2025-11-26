using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Manager.DTOs;

namespace SMMS.Application.Features.Manager.Queries;
public record SearchAccountsQuery(Guid SchoolId, string Keyword)
    : IRequest<List<AccountDto>>;

public record FilterByRoleQuery(Guid SchoolId, string Role)
    : IRequest<List<AccountDto>>;

public record GetAllStaffQuery(Guid SchoolId)
    : IRequest<List<AccountDto>>;
