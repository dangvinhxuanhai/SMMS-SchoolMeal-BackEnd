using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using SMMS.Application.Features.foodmenu.Queries;

namespace SMMS.Application.Features.foodmenu.Validators;
public sealed class GetAvailableWeeksValidator : AbstractValidator<GetAvailableWeeksQuery>
{
    public GetAvailableWeeksValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("StudentId là bắt buộc.");

        // Nếu cả From và To đều có, kiểm tra From <= To
        When(x => x.From.HasValue && x.To.HasValue, () =>
        {
            RuleFor(x => x)
                .Must(x => x.From!.Value.Date <= x.To!.Value.Date)
                .WithMessage("Khoảng ngày không hợp lệ: From phải <= To.");
        });
    }
}
