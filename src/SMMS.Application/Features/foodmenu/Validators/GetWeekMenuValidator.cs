using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using SMMS.Application.Features.foodmenu.Queries;

namespace SMMS.Application.Features.foodmenu.Validators;
public sealed class GetWeekMenuValidator : AbstractValidator<GetWeekMenuQuery>
{
    public GetWeekMenuValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("StudentId là bắt buộc.");

        RuleFor(x => x.AnyDateInWeek)
            .NotEqual(default(DateTime))
            .WithMessage("AnyDateInWeek không hợp lệ.");
    }
}
