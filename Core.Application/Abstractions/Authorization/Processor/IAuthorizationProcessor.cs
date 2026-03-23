using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Abstractions.Authorization.Processor
{
    public interface IAuthorizationProcessor
    {
        Task<bool> CheckAccessAsync( string resourceKey, string action);

    }
}
