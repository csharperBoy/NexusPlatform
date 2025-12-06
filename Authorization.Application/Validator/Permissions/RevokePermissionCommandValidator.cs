using Authorization.Application.Commands.Permissions;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Validator.Permissions
{
    public class RevokePermissionCommandValidator : AbstractValidator<RevokePermissionCommand>
    {
        public RevokePermissionCommandValidator()
        {
            RuleFor(x => x.PermissionId)
                .NotEmpty().WithMessage("Permission ID is required");
        }
    }
}
