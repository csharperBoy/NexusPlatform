using Identity.Shared.Events;
using Microsoft.Extensions.Logging;
using Moq;
using Notification.Application.EventHandlers;
using Notification.Application.Interfaces;
using Notification.Application.Models;
using Polly;
using Polly.Registry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Test
{
    public class WelcomeInAppNotificationHandlerTests
    {
        [Fact]
        public async Task Handle_Should_Send_InAppNotification_When_UserRegisteredEvent_Raised()
        {
            // Arrange
            var notifier = new Mock<IRealtimeNotifier>();
            var logger = new Mock<ILogger<WelcomeInAppNotificationHandler>>();

            var policy = Policy.NoOpAsync();
            var registry = new PolicyRegistry
        {
            { "DefaultRetry", policy }
        };

            var handler = new WelcomeInAppNotificationHandler(notifier.Object, logger.Object, registry);

            var userRegisteredEvent = new UserRegisteredEvent(
                Guid.NewGuid(),
                "ali",
                "ali@test.com"
            );

            // Act
            await handler.Handle(userRegisteredEvent, CancellationToken.None);

            // Assert
            notifier.Verify(n => n.SendToUserAsync(
                userRegisteredEvent.UserId.ToString(),
                It.Is<NotificationMessage>(msg =>
                    msg.Title == "Welcome!" &&
                    msg.Body.Contains("ali") &&
                    msg.Channel == NotificationChannel.InApp),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
