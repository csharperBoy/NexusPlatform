using Authorization.Application.Commands.Permissions;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Validator.Permissions
{
    
    public class AssignPermissionCommandValidator : AbstractValidator<AssignPermissionCommand>
    {
        public AssignPermissionCommandValidator()
        {
            RuleFor(x => x.ResourceId)
                .NotEmpty().WithMessage("Resource ID is required");

            RuleFor(x => x.AssigneeType)
                .IsInEnum().WithMessage("Invalid assignee type");

            RuleFor(x => x.AssigneeId)
                .NotEmpty().WithMessage("Assignee ID is required");

            RuleFor(x => x.Action)
                .IsInEnum().WithMessage("Invalid permission action");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

            RuleFor(x => x.EffectiveFrom)
                .LessThan(x => x.ExpiresAt).When(x => x.EffectiveFrom.HasValue && x.ExpiresAt.HasValue)
                .WithMessage("Effective from date must be before expiration date");

            }
    }
}
