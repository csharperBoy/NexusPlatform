using Audit.Domain.Entities;
using Audit.Infrastructure.Data;
using Audit.Infrastructure.Services;
using Core.Application.Abstractions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audit.Test
{
    
    public class AuditServiceTests
    {
        [Fact]
        public async Task LogAsync_Should_Add_AuditLog_And_SaveChanges()
        {
            // Arrange
            var repo = new Mock<IRepository<AuditDbContext, AuditLog, Guid>>();
            var uow = new Mock<IUnitOfWork<AuditDbContext>>();
            var logger = new Mock<ILogger<AuditService>>();

            var service = new AuditService(repo.Object, uow.Object, logger.Object);

            // Act
            await service.LogAsync(
                action: "UserRegistered",
                entityName: "ApplicationUser",
                entityId: Guid.NewGuid().ToString(),
                userId: new Guid("123"),
                changes: new { Username = "ali", Email = "ali@test.com" });

            // Assert
            repo.Verify(r => r.AddAsync(It.Is<AuditLog>(log =>
                log.Action == "UserRegistered" &&
                log.EntityName == "ApplicationUser" &&
                log.UserId == new Guid("123"))), Times.Once);

            uow.Verify(u => u.SaveChangesAsync(default), Times.Once);
        }
    }
}
