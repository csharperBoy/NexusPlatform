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
    public class AssignDataScopeCommandValidator : AbstractValidator<AssignDataScopeCommand>
    {
        public AssignDataScopeCommandValidator()
        {
            RuleFor(x => x.ResourceId)
                .NotEmpty().WithMessage("Resource ID is required");

            RuleFor(x => x.AssigneeType)
                .IsInEnum().WithMessage("Invalid assignee type");

            RuleFor(x => x.AssigneeId)
                .NotEmpty().WithMessage("Assignee ID is required");

            RuleFor(x => x.Scope)
                .IsInEnum().WithMessage("Invalid scope type");

            RuleFor(x => x.SpecificPropertyId)
                .NotEmpty().When(x => x.Scope == ScopeType.SpecificProperty)
                .WithMessage("SpecificUnitId is required for SpecificUnit scope");

            RuleFor(x => x.SpecificPropertyId)
                .Null().When(x => x.Scope != ScopeType.SpecificProperty)
                .WithMessage("SpecificUnitId should only be set for SpecificUnit scope");

            RuleFor(x => x.CustomFilter)
                .MaximumLength(1000).WithMessage("Custom filter cannot exceed 1000 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

            RuleFor(x => x.EffectiveFrom)
                .LessThan(x => x.ExpiresAt).When(x => x.EffectiveFrom.HasValue && x.ExpiresAt.HasValue)
                .WithMessage("Effective from date must be before expiration date");
        }
    }
}
