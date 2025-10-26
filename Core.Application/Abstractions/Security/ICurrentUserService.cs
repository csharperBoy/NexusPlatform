using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Abstractions.Security
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        string? UserName { get; }
        bool IsAuthenticated { get; }
        IEnumerable<string> Roles { get; }
    }
}