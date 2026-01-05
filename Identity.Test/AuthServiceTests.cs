using Core.Application.Abstractions;
using Core.Application.Abstractions.Auditing;
using Core.Application.Abstractions.Events;
using Core.Application.Abstractions.Security;
using Core.Domain.Common;
using FluentAssertions;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Identity.Infrastructure.Configuration;
using Identity.Infrastructure.Data;
using Identity.Infrastructure.Services;
using Identity.Shared.Events;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityResult = Microsoft.AspNetCore.Identity.IdentityResult;

namespace Identity.Test
{
    public class AuthServiceTests
    {
        [Fact]
        public async Task Register_Should_CreateUser_And_PublishEvent_And_GenerateToken_And_SaveRefreshToken()
        {
            // Arrange
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var userManager = MockUserManager(userStore.Object);
            var signInManager = MockSignInManager(userManager.Object);

            var tokenService = new Mock<IJwtTokenService>();
            tokenService
                .Setup(t => t.GenerateTokensAsync(
                    It.IsAny<ApplicationUser>(),
                    It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(("fake-jwt", "fake-refresh"));

            var outbox = new Mock<IOutboxService<IdentityDbContext>>();
            var roleResolver = new Mock<IRoleInternalService>();
            roleResolver.Setup(r => r.GetUserRolesAsync(It.IsAny<Guid>()))
                        .ReturnsAsync(new List<string> { "User" });

            var refreshRepo = new Mock<IRepository<IdentityDbContext, RefreshToken, Guid>>();
            var specRepo = new Mock<ISpecificationRepository<RefreshToken, Guid>>();
            var unitOfWork = new Mock<IUnitOfWork<IdentityDbContext>>();

            var logger = new Mock<ILogger<AuthService>>();
            var jwtOptions = Options.Create(new JwtOptions { AccessTokenExpiryMinutes = 60, RefreshTokenExpiryDays = 7 });

            var service = new AuthService(
                userManager.Object,
                signInManager.Object,
                tokenService.Object,
                jwtOptions,
                outbox.Object,
                roleResolver.Object,
                refreshRepo.Object,
                specRepo.Object,
                unitOfWork.Object,
                logger.Object
            );

            var request = new RegisterRequest("ali", "ali@test.com", "Pass123!", "Ali");

            // Act
            var result = await service.RegisterAsync(request);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.AccessToken.Should().Be("fake-jwt");
            result.Data!.RefreshToken.Should().Be("fake-refresh");

            // کاربر ساخته شده
            userManager.Verify(u => u.CreateAsync(It.IsAny<ApplicationUser>(), request.Password), Times.Once);

            // ایونت منتشر شده
            outbox.Verify(o => o.AddEventsAsync(
                It.Is<IEnumerable<IDomainEvent>>(evts => evts.Any(e => e is UserRegisteredEvent))),
                Times.Once);

            // توکن ساخته شده
            tokenService.Verify(t => t.GenerateTokensAsync(It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<string>>()), Times.Once);

            // RefreshToken ذخیره شده
            refreshRepo.Verify(r => r.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
            unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        // Helpers برای Mock کردن UserManager و SignInManager
        private static Mock<UserManager<ApplicationUser>> MockUserManager(IUserStore<ApplicationUser> store)
        {
            var mgr = new Mock<UserManager<ApplicationUser>>(store, null, null, null, null, null, null, null, null);
            mgr.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
               .ReturnsAsync(IdentityResult.Success);
            return mgr;
        }

        private static Mock<SignInManager<ApplicationUser>> MockSignInManager(UserManager<ApplicationUser> userManager)
        {
            var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            return new Mock<SignInManager<ApplicationUser>>(userManager, contextAccessor.Object, claimsFactory.Object, null, null, null, null);
        }
    }
}
