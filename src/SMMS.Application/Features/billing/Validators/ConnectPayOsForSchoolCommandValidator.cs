using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using SMMS.Application.Features.billing.Commands;

namespace SMMS.Application.Features.billing.Validators;
public sealed class ConnectPayOsForSchoolCommandValidator
    : AbstractValidator<ConnectPayOsForSchoolCommand>
{
    public ConnectPayOsForSchoolCommandValidator()
    {
        RuleFor(x => x.SchoolId).NotEmpty();
        RuleFor(x => x.ClientId).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ApiKey).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ChecksumKey).NotEmpty().MaximumLength(200);
    }
}
