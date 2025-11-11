using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Manager.DTOs;

namespace SMMS.Application.Features.Manager.Commands;
public record CreateAccountCommand(CreateAccountRequest Request)
    : IRequest<AccountDto>;

public record UpdateAccountCommand(Guid UserId, UpdateAccountRequest Request)
    : IRequest<AccountDto?>;

public record ChangeStatusCommand(Guid UserId, bool IsActive)
    : IRequest<bool>;

public record DeleteAccountCommand(Guid UserId)
    : IRequest<bool>;
