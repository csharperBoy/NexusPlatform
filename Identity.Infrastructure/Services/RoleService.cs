using Core.Application.Abstractions;
using Core.Application.Abstractions.Identity;
using Core.Application.Abstractions.Security;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Identity.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Infrastructure.Services
{
    public class RoleService : IRoleInternalService, IRolePublicService
    {
        private readonly IAuthorizationService _authorizationService;

        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<IdentityDbContext, ApplicationRole, Guid> _roleRepository;
        private readonly ISpecificationRepository<ApplicationRole, Guid> _roleSpecRepository;
        public RoleService(
            IAuthorizationService authorizationService, 
            IRepository<IdentityDbContext, ApplicationRole, Guid> roleRepository,
            ISpecificationRepository<ApplicationRole, Guid> roleSpecRepository,
            RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager
            )
        {
            _authorizationService = authorizationService;
            _roleRepository = roleRepository;
            _roleManager = roleManager;
            _userManager = userManager;
            _roleSpecRepository = roleSpecRepository;
        }


        public async Task<Guid> GetAdminRoleIdAsync(CancellationToken cancellationToken = default)
        {
            return await _authorizationService.GetRoleId("Admin");
        }

        public async Task<List<Guid>> GetAllUserRolesId(Guid userId)
        {
            List<Guid> RolesId = new List<Guid>();
            ApplicationUser? user = await _userManager.FindByIdAsync(userId.ToString());
            var rolesName = await _userManager.GetRolesAsync(user);
            foreach (var roleName in rolesName)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                RolesId.Add(role.Id);
            }
            return RolesId;
        }

        public async Task<IList<string>> GetUserRolesAsync(Guid userId)
        {
            return await _authorizationService.GetUserRolesAsync(userId);
        }
    }

}