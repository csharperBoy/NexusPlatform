using Authorization.Application.Commands.DataScopes;
using Authorization.Domain.Enums;
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

            RuleFor(x => x.SpecificUnitId)
                .NotEmpty().When(x => x.Scope == ScopeType.SpecificUnit)
                .WithMessage("SpecificUnitId is required for SpecificUnit scope");

            RuleFor(x => x.SpecificUnitId)
                .Null().When(x => x.Scope != ScopeType.SpecificUnit)
                .WithMessage("SpecificUnitId should only be set for SpecificUnit scope");

            RuleFor(x => x.Depth)
                .InclusiveBetween(1, 10).When(x => x.Scope == ScopeType.Subtree)
                .WithMessage("Depth must be between 1 and 10 for Subtree scope");

            RuleFor(x => x.CustomFilter)
                .MaximumLength(1000).WithMessage("Custom filter cannot exceed 1000 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
        }
    }
}
