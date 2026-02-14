using Authorization.Application.Commands.DataScopes;
using Authorization.Domain.Enums;
using Core.Domain.Enums;
using Core.Shared.Enums;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Validator.DataScopes
{
    public class UpdateDataScopeCommandValidator : AbstractValidator<UpdateDataScopeCommand>
    {
        public UpdateDataScopeCommandValidator()
        {
            RuleFor(x => x.DataScopeId)
                .NotEmpty().WithMessage("DataScope ID is required");

            RuleFor(x => x.Scope)
                .IsInEnum().WithMessage("Invalid scope type");

            RuleFor(x => x.SpecificProperty)
                .NotEmpty().When(x => x.Scope == ScopeType.SpecificProperty)
                .WithMessage("SpecificUnitId is required for SpecificUnit scope");

            RuleFor(x => x.SpecificProperty)
                .Null().When(x => x.Scope != ScopeType.SpecificProperty)
                .WithMessage("SpecificUnitId should only be set for SpecificUnit scope");


            RuleFor(x => x.CustomFilter)
                .MaximumLength(1000).WithMessage("Custom filter cannot exceed 1000 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
        }
    }
}
