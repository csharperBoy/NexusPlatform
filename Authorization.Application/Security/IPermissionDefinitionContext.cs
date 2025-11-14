using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Security
{
    public interface IPermissionDefinitionContext
    {
        IModuleBuilder AddModule(string moduleCode, string name);
    }

}
