using Core.Domain.Events;
using Microsoft.Extensions.Logging;
using Moq;
using Notification.Application.EventHandlers;
using Notification.Application.Interfaces;
using Polly;
using Polly.Registry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Test
{
    public class WelcomeEmailEventHandlerTests
    {
        [Fact]
        public async Task Handle_Should_Send_WelcomeEmail_When_UserRegisteredEvent_Raised()
        {
            // Arrange
            var emailSender = new Mock<IEmailSender>();
            var logger = new Mock<ILogger<WelcomeEmailEventHandler>>();

            // یک Policy ساده برای تست
            var policy = Policy.NoOpAsync();
            var registry = new PolicyRegistry
        {
            { "DefaultRetry", policy }
        };

            var handler = new WelcomeEmailEventHandler(emailSender.Object, logger.Object, registry);

            var userRegisteredEvent = new UserRegisteredEvent(
                Guid.NewGuid(),
                "ali",
                "ali@test.com"
            );

            // Act
            await handler.Handle(userRegisteredEvent, CancellationToken.None);

            // Assert
            emailSender.Verify(e => e.SendEmailAsync(
                userRegisteredEvent.Email,
                "Welcome!",
                It.Is<string>(body => body.Contains("ali"))),
                Times.Once);
        }
    }
}
