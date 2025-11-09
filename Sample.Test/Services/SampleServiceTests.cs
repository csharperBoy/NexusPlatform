using Core.Application.Abstractions;
using Core.Application.Abstractions.Caching;
using Core.Application.Abstractions.Security;
using Microsoft.Extensions.Logging;
using Moq;
using Sample.Application.DTOs;
using Sample.Application.Interfaces;
using Sample.Domain.Entities;
using Sample.Domain.Specifications;
using Sample.Infrastructure.Data;
using Sample.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Test.Services
{
    public class SampleServiceTests
    {
        private readonly Mock<IRepository<SampleDbContext, SampleEntity, Guid>> _repositoryMock;
        private readonly Mock<IUnitOfWork<SampleDbContext>> _uowMock;
        private readonly Mock<ICurrentUserService> _currentUserMock;
        private readonly Mock<ICacheService> _cacheMock;
        private readonly Mock<ILogger<SampleService>> _loggerMock;

        private readonly ISampleService _service;

        public SampleServiceTests()
        {
            _repositoryMock = new Mock<IRepository<SampleDbContext, SampleEntity, Guid>>();
            _uowMock = new Mock<IUnitOfWork<SampleDbContext>>();
            _currentUserMock = new Mock<ICurrentUserService>();
            _cacheMock = new Mock<ICacheService>();
            _loggerMock = new Mock<ILogger<SampleService>>();

            _service = new SampleService(
                _repositoryMock.Object,
                _uowMock.Object,
                _loggerMock.Object,
                _currentUserMock.Object,
                _cacheMock.Object
            );
        }

        [Fact]
        public async Task SampleApiMethodAsync_Should_AddEntity_And_SaveChanges()
        {
            // Arrange
            var request = new SampleApiRequest("value1", "value2");

            // Act
            var result = await _service.SampleApiMethodAsync(request);

            // Assert
            Assert.True(result.Succeeded);
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<SampleEntity>()), Times.Once);
            _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task SampleApiMethodWithCacheAsync_Should_AddEntity_SaveChanges_And_InvalidateCache()
        {
            // Arrange
            var request = new SampleApiRequest("value1", "value2");

            // Act
            var result = await _service.SampleApiMethodWithCacheAsync(request);

            // Assert
            Assert.True(result.Succeeded);
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<SampleEntity>()), Times.Once);
            _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
            _cacheMock.Verify(c => c.RemoveByPatternAsync(It.Is<string>(s => s.Contains("sample:list:value1"))), Times.Once);
        }
    }
}
