using Authorization.Application.Commands;
using Core.Shared.Results;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Validator.Resource
{
    public class CreateResourceValidator : AbstractValidator<CreateResourceCommand>
    {
        public CreateResourceValidator()
        {
            RuleFor(x => x.Key)
                .NotEmpty().WithMessage("Resource key is required")
                .MaximumLength(100).WithMessage("Resource key cannot exceed 100 characters")
                .Matches("^[a-z0-9.-]+$").WithMessage("Resource key can only contain lowercase letters, numbers, dots and hyphens");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Resource name is required")
                .MaximumLength(200).WithMessage("Resource name cannot exceed 200 characters");

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Invalid resource type");

            RuleFor(x => x.Category)
                .IsInEnum().WithMessage("Invalid resource category");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order must be a positive number");

            RuleFor(x => x.Icon)
                .MaximumLength(50).WithMessage("Icon cannot exceed 50 characters");

            RuleFor(x => x.Route)
                .MaximumLength(200).WithMessage("Route cannot exceed 200 characters")
                .Matches("^[a-zA-Z0-9/_-]+$").When(x => !string.IsNullOrEmpty(x.Route))
                .WithMessage("Route can only contain letters, numbers, slashes, hyphens and underscores");
        }
    }
}
