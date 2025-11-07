using FluentValidation;
using Sample.Application.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Application.Validators
{
    public class SampleApiCommandValidator : AbstractValidator<SampleApiCommand>
    {
        public SampleApiCommandValidator()
        {
            RuleFor(x => x.property1)
                .NotEmpty().WithMessage("property1 نباید خالی باشد.")
                .MaximumLength(200);

            RuleFor(x => x.property2)
                .NotEmpty().WithMessage("property2 نباید خالی باشد.")
                .MaximumLength(200);
        }
    }
}
