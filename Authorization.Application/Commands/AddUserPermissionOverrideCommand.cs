using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Commands
{
    //public class AddUserPermissionOverrideCommand
    //{
    //    public Guid UserId { get; set; }
    //    public Guid PermissionId { get; set; }
    //    public bool Granted { get; set; }
    //    public string? Scope { get; set; }
    //}
    public record AddUserPermissionOverrideCommand(Guid UserId, Guid PermissionId, bool Granted, string? Scope)
      : IRequest<Result<bool>>;
}
