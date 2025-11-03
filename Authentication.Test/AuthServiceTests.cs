using Authentication.Application.DTOs;
using Authentication.Domain.Entities;
using Authentication.Infrastructure.Configuration;
using Authentication.Infrastructure.Data;
using Authentication.Infrastructure.Services;
using Core.Application.Abstractions.Auditing;
using Core.Application.Abstractions.Events;
using Core.Application.Abstractions.Security;
using Core.Domain.Common;
using Core.Domain.Events;
using FluentAssertions;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Notification.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityResult = Microsoft.AspNetCore.Identity.IdentityResult;

namespace Authentication.Test
{
    public class AuthServiceTests
    {
        [Fact]
        public async Task Register_Should_CreateUser_And_PublishEvent_And_GenerateToken()
        {
            // Arrange
            var userStore = new Mock<Microsoft.AspNetCore.Identity.IUserStore<ApplicationUser>>();
            var userManager = MockUserManager(userStore.Object);
            var signInManager = MockSignInManager(userManager.Object);

            var tokenService = new Mock<IJwtTokenService>();
            tokenService
                .Setup(t => t.GenerateTokensAsync(
                    It.IsAny<ApplicationUser>(),
                    It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(("fake-jwt", "fake-refresh"));

            var outbox = new Mock<IOutboxService<AuthDbContext>>();
            var roleResolver = new Mock<IRoleResolver>();
            roleResolver.Setup(r => r.GetUserRolesAsync(It.IsAny<Guid>()))
                        .ReturnsAsync(new List<string> { "User" });

            var logger = new Mock<ILogger<AuthService>>();
            var jwtOptions = Options.Create(new JwtOptions { AccessTokenExpiryMinutes = 60 });

            var service = new AuthService(
                userManager.Object,
                signInManager.Object,
                tokenService.Object,
                jwtOptions,
                outbox.Object,
                roleResolver.Object,
                logger.Object
            );

            var request = new RegisterRequest("ali", "ali@test.com", "Pass123!", "Ali");

            // Act
            var result = await service.RegisterAsync(request);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Token.Should().Be("fake-jwt");

            // کاربر ساخته شده
            userManager.Verify(u => u.CreateAsync(It.IsAny<ApplicationUser>(), request.Password), Times.Once);

            // ایونت منتشر شده
            outbox.Verify(o => o.AddEventsAsync(
                It.Is<IEnumerable<IDomainEvent>>(evts => evts.Any(e => e is Core.Domain.Events.UserRegisteredEvent))),
                Times.Once);

            // توکن ساخته شده
            tokenService.Verify(t => t.GenerateTokensAsync(It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<string>>()), Times.Once);
        }

        // Helpers برای Mock کردن UserManager و SignInManager
        private static Mock<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>> MockUserManager(Microsoft.AspNetCore.Identity.IUserStore<ApplicationUser> store)
        {
            var mgr = new Mock<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>>(store, null, null, null, null, null, null, null, null);
            mgr.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
               .ReturnsAsync(IdentityResult.Success);
            return mgr;
        }

        private static Mock<SignInManager<ApplicationUser>> MockSignInManager(Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager)
        {
            var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            return new Mock<SignInManager<ApplicationUser>>(userManager, contextAccessor.Object, claimsFactory.Object, null, null, null, null);
        }
    }
  }
