using Azure.Core;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Authorization.PublicService;
using Core.Application.Abstractions.Caching.PublicService;
using Core.Application.Abstractions.HR;
using Core.Application.Abstractions.Identity.PublicService;
using Core.Application.Context;
using Core.Application.Helper;
using Core.Shared.DTOs;

//using Core.Shared.DTOs.Identity;
using Core.Shared.Results;
using Identity.Application.Commands.User;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Application.Queries.User;
using Identity.Domain.Entities;
using Identity.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Identity.Infrastructure.Services
{
    internal class UserService : IUserInternalService, IUserPublicService
    {
        private readonly IRepository<IdentityDbContext, ApplicationUser, Guid> _userRepository;

        private readonly IRepository<IdentityDbContext, IdentityUserRole<Guid>, Guid> _userRoleRepository;
        private readonly IUnitOfWork<IdentityDbContext> _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ILogger<UserService> _logger;

        private readonly IPositionPublicService _positionService;
        private readonly IRoleInternalService _roleService;
        private readonly IPermissionPublicService _permissionService;

        private readonly ICachePublicService _cache;
        private readonly string baseCacheKey = "identity:user";

        public UserService(
            IRepository<IdentityDbContext, ApplicationUser, Guid> userRepository,
            IRepository<IdentityDbContext, IdentityUserRole<Guid>, Guid> userRoleRepository,
            IUnitOfWork<IdentityDbContext> unitOfWork,
            UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager,
            IPositionPublicService positionService,
            IRoleInternalService roleService,
            IPermissionPublicService permissionService,
            ICachePublicService cache,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _cache = cache;
            _positionService = positionService;
            _roleService = roleService;
            _permissionService = permissionService;
        }

        public async Task DeleteUserAsync(Guid Id)
        {
            var user = await _userRepository.GetByIdAsync(Id);
            if (user == null) return;
            var Res = await _userManager.DeleteAsync(user);
            if (!Res.Succeeded)
                throw new Exception(Res.Errors.FirstOrDefault().Description);

            //await _userRepository.DeleteAsync(user);
            //await _unitOfWork.SaveChangesAsync();
            await InvalidateUserCachesAsync();
        }

                
        public async Task<Guid> CreateUserAsync(CreateUserCommand command)
        {
            var user = new ApplicationUser(
                command.UserName,
                command.Email,
                command.NickName,
                command.phoneNumber,
                command.personId
            );
            var createRes = await _userManager.CreateAsync(user, command.Password);
            if (!createRes.Succeeded)
                throw new Exception(createRes.Errors.FirstOrDefault().Description);

            var addToRolesRes = await _userManager.AddToRolesAsync(user, command.roles);
            if (!addToRolesRes.Succeeded)
                throw new Exception(addToRolesRes.Errors.FirstOrDefault().Description);



            //await _unitOfWork.SaveChangesAsync();
            await InvalidateUserCachesAsync();
            return user.Id;
        }
        public async Task<UserDto?> GetById(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user == null)
            {
                throw new Exception($"user with name '{id}' not found.");
            }
            UserDto result = new UserDto()
            {
                NickName = user.NickName,
                Email = user.Email,
                Id = user.Id,
                phoneNumber = user.PhoneNumber,
                UserName = user.UserName
            };
            result.roles = await _userManager.GetRolesAsync(user);
            return result;
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

        public async Task<IReadOnlyList<UserDto>> GetUsers(
        string? userName = null,
        List<Guid>? rolesId = null,
        string? nickName = null,
        string? phoneNumber = null)
        {
            var cacheKey = $"{baseCacheKey}:full";

            var cached = await _cache.GetAsync<IReadOnlyList<UserDto>>(cacheKey);
            //if (cached != null)
            //    return cached;

            // EF Projection
            // فرض کنید _userManager, _roleManager و _userRoleRepository همانند قبلی هستند

            var query = await (
                from user in _userManager.Users.AsNoTracking()
                    // 1. گروه‌کردن تمام نقش‌های کاربر (ممکن است خالی باشد)
                join userRole in await _userRoleRepository.AsNoTrackingQueryable()
                    on user.Id equals userRole.UserId into urGroup
                // 2. گسترش گروه به یک رکورد (یا null اگر خالی باشد)
                from ur in urGroup.DefaultIfEmpty()
                    // 3. اتصال نقش (اگر ur null بود این join هم null خواهد شد)
                join role in _roleManager.Roles.AsNoTracking()
                    on ur.RoleId equals role.Id into rGroup
                from r in rGroup.DefaultIfEmpty()
                    // 4. فیلترها (به‌دقت در شرایطی که null است)
                where
                    (userName == null || user.UserName.Contains(userName)) &&
                    (nickName == null || user.NickName.Contains(nickName)) &&
                    (phoneNumber == null || user.PhoneNumber.Contains(phoneNumber)) &&
                    (rolesId == null || rolesId.Contains(r != null ? r.Id : Guid.Empty))
                // 5. گروه‌سازی بر اساس کاربر (هر کاربر یک رکورد)
                group r by user into g
                // 6. ساخت DTO
                select new UserDto
                {
                    Id = g.Key.Id,
                    UserName = g.Key.UserName,
                    Email = g.Key.Email,
                    phoneNumber = g.Key.PhoneNumber,
                    NickName = g.Key.NickName,
                    // نقش‌های خالی را به لیست خالی تبدیل می‌کنیم
                    roles = g.Where(x => x != null).Select(x => x.Name).ToList()
                }).ToListAsync();


            var result = query;

            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30));

            return result;
        }

        public async Task UpdateUserAsync(UpdateUserCommand request)
        {
            var user = await _userRepository.GetByIdAsync(request.Id);
            if (user == null) throw new ArgumentException("User not found");
            if ( user.ApplyChange(request.UserName, request.NickName, request.Password, request.Email, request.phoneNumber,_userManager, request.personId))
            {
                await _userManager.UpdateAsync(user);
                await _userManager.RemoveFromRolesAsync(user, (await _userManager.GetRolesAsync(user)));
                await _userManager.AddToRolesAsync(user, request.roles);
                await InvalidateUserCachesAsync();
            }
        }
        private async Task InvalidateUserCachesAsync()
        {
            await _cache.RemoveByPatternAsync($"{baseCacheKey}:*");
        }

        
    }
}
