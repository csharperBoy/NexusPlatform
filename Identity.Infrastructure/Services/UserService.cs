using Core.Application.Abstractions;
using Core.Application.Abstractions.Identity;
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

        public UserService(
            IRepository<IdentityDbContext, ApplicationUser, Guid> userRepository,
            IUnitOfWork<IdentityDbContext> unitOfWork,
            UserManager<ApplicationUser> userManager,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _logger = logger;
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
    }
}
