using Authorization.Application.Commands.Permissions;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Validator.Permissions
{
    public class RevokePermissionCommandValidator : AbstractValidator<DeletePermissionCommand>
    {
        public RevokePermissionCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Permission ID is required");
        }
    }
}
