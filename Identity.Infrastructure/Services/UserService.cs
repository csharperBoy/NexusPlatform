using Core.Application.Abstractions;
using Core.Application.Abstractions.Authorization;
using Core.Application.Abstractions.HR;
using Core.Application.Abstractions.Identity;
using Core.Application.Context;
using Core.Application.Helper;
using Core.Shared.DTOs.Identity;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Identity.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Infrastructure.Services
{
    internal class UserService : IUserInternalService, IUserPublicService
    {
        private readonly IRepository<IdentityDbContext, ApplicationUser, Guid> _userRepository;

        private readonly IUnitOfWork<IdentityDbContext> _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UserService> _logger;

        private readonly IPositionPublicService _positionService;
        private readonly IRolePublicService _roleService;
        private readonly IPermissionPublicService _permissionService;

        public UserService(
            IRepository<IdentityDbContext, ApplicationUser, Guid> userRepository,
            IUnitOfWork<IdentityDbContext> unitOfWork,
            UserManager<ApplicationUser> userManager,
            IPositionPublicService positionService,
            IRolePublicService roleService,
            IPermissionPublicService permissionService,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _logger = logger;

            _positionService = positionService;
            _roleService = roleService;
            _permissionService = permissionService;
        }

        public async Task<DataScopeContext> GetInitializerUserContext()
        {
            var user = await _userManager.FindByNameAsync("intitializer");

            if (user == null)
            {
                throw new Exception($"user with name intitializer not found.");
            }
            var peronId = await GetPersonId(user.Id);


            Guid? PersonId = await GetPersonId(user.Id);
            List<Guid>? PositionId = await _positionService.GetUserPositionsId(user.Id);
            List<Guid> RoleIds = await _roleService.GetAllUserRolesId(user.Id);
            List<Guid>? OrgIds = await _positionService.GetUserOrganizeId(user.Id);
            var allPermission = await _permissionService.GetUserAllPermissionsAsync(user.Id, PersonId, PositionId, RoleIds);
           
            return new DataScopeContext
            {
                UserId = user.Id,
                PersonId = PersonId,
                PositionIds = PositionId?.ToHashSet(),
                OrganizationUnitIds = OrgIds?.ToHashSet(),
                RoleIds = RoleIds.ToHashSet(),
                Permissions = allPermission.ToHashSet(),
                //userPermissions = userPermissions.ToHashSet(),
                //rolePermissions = rolePermissions.ToHashSet(),
                //personPermissions = personPermissions.ToHashSet(),
                //positionPermissions = positionPermissions.ToHashSet(),
            };

        }

        public async Task<Guid?> GetPersonId(Guid userId)
        {
            if (!ModuleHelper.IsActive(Core.Domain.Enums.ModuleEnum.User))
                return null;

            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                throw new Exception($"user with name '{userId}' not found.");
            }

            return user.FkPersonId;
        }

        public async Task<Guid> GetUserId(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);

            if (user == null)
            {
                throw new Exception($"user with name '{userName}' not found.");
            }

            return user.Id;
        }

        public async Task<string?> GetUserName(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                throw new Exception($"user with name '{userId}' not found.");
            }

            return user.UserName;
        }
    }
}
