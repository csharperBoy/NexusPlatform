using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Security
{
    public class IdentityPermissionDefinitionProvider : IPermissionDefinitionProvider
    {
        public void Define(IPermissionDefinitionContext ctx)
        {
            var module = ctx.AddModule("Identity", "Identity & Users");

            var users = module.AddPage("Users", "User Management");
            users.AddAction("List", "List Users").Add("View", "View user details");
            users.AddAction("Create", "Create User");
            users.AddAction("Edit", "Edit User");
            users.AddAction("Delete", "Delete User");

            var roles = module.AddPage("Roles", "Role Management");
            roles.AddAction("List", "List Roles");
            roles.AddAction("Create", "Create Role");
            roles.AddAction("Edit", "Edit Role");
            roles.AddAction("Delete", "Delete Role");
        }
    }
}
