using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace SMMS.Application.Features.billing.Commands;
public sealed record ConnectPayOsForSchoolCommand(
    Guid SchoolId,
    Guid CreatedBy,
    string ClientId,
    string ApiKey,
    string ChecksumKey
) : IRequest;
