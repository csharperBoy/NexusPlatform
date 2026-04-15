using Core.Application.Abstractions;
using Core.Application.Abstractions.Authorization.PublicService;
using Core.Application.Abstractions.Caching.PublicService;
using Core.Application.Abstractions.HR;
using Core.Application.Abstractions.Identity.PublicService;
using Core.Application.Context;
using Core.Application.Helper;
using Core.Shared.DTOs.Identity;
using Identity.Application.Commands.User;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Application.Queries.User;
using Identity.Domain.Entities;
using Identity.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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

        private readonly ICachePublicService _cache;
        private readonly string baseCacheKey = "identity:user";

        public UserService(
            IRepository<IdentityDbContext, ApplicationUser, Guid> userRepository,
            IUnitOfWork<IdentityDbContext> unitOfWork,
            UserManager<ApplicationUser> userManager,
            IPositionPublicService positionService,
            IRolePublicService roleService,
            IPermissionPublicService permissionService,
            ICachePublicService cache,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _logger = logger;
            _cache = cache;
            _positionService = positionService;
            _roleService = roleService;
            _permissionService = permissionService;
        }
        public async Task<Guid> CreateUserAsync(CreateUserCommand command)
        {
            var user = new ApplicationUser(
                command.UserName,
                command.Email,
                command.firstName,
                command.lastName,
                command.phoneNumber,
                command.personId
            );
            await _userManager.CreateAsync(user,command.Password);            
            await InvalidateUserCachesAsync();
            return user.Id;
        }
        public async Task<ApplicationUser?> GetById(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user == null)
            {
                throw new Exception($"user with name '{id}' not found.");
            }

            return user;
        }

        public async Task<UserDataContext> GetInitializerUserContext()
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

            return new UserDataContext
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

        public async Task<IReadOnlyList<UserDto>> getUsers(GetUsersQuery request)
        {
            var cacheKey = $"{baseCacheKey}:full";
            var cached = await _cache.GetAsync<IReadOnlyList<UserDto>>(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for full resource tree");
                return cached;
            }
            var result = await _userManager.Users.Where(
                u => (request.UserName != null ? u.UserName.Contains(request.UserName) : true)
                && (request.FullName != null ? (u.FullName.FirstName.Contains(request.FullName) || u.FullName.LastName.Contains(request.FullName)) : true)
                && (request.phoneNumber != null ? u.PhoneNumber.Contains(request.phoneNumber) : true)
                ).Select(u => new UserDto
                {
                    FirstName = u.FullName.FirstName ,
                    LastName = u.FullName.LastName,
                    Email = u.Email,
                    Id = u.Id,
                    phoneNumber = u.PhoneNumber,
                    UserName = u.UserName
                }).AsNoTracking().ToListAsync();

            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30));
            return result;
        }

        public async Task UpdateUserAsync(UpdateUserCommand request)
        {
            var user = await _userRepository.GetByIdAsync(request.Id);
            if (user == null) throw new ArgumentException("User not found");
            bool hasChange = false;
            // آپدیت فیلدها
            if (request.UserName != null && request.UserName != user.UserName)
            {
                user.UserName =request.UserName;
                hasChange = true;
            }
            if ((request.FirstName != null && request.FirstName != user.FullName?.FirstName) || (request.LastName != null && request.LastName != user.FullName?.LastName))
            {
                user.SetFullName(request.FirstName, request.LastName);
                hasChange = true;
            }
            if (request.Email != null && request.Email.ToString() != user.Email)
            {
                user.Email = request.Email.ToString();
                hasChange = true;
            }
            if (request.phoneNumber != null && request.phoneNumber != user.PhoneNumber)
            {
                user.PhoneNumber = request.phoneNumber;
                hasChange = true;
            }
            if (request.Password != null )
            {
                user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, request.Password);
                hasChange = true;
            }

            if (hasChange)
            {
                await _userManager.UpdateAsync(user);
                await InvalidateUserCachesAsync();
            }
        }
        private async Task InvalidateUserCachesAsync()
        {
            await _cache.RemoveByPatternAsync($"{baseCacheKey}:*");
        }

    }
}
