using Core.Application.Abstractions;
using Core.Application.Abstractions.Caching;
using Microsoft.Extensions.Logging;
using Moq;
using Sample.Application.Interfaces;
using Sample.Domain.Entities;
using Sample.Domain.Specifications;
using Sample.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Test.Services
{
    public class SampleQueryServiceTests
    {
        private readonly Mock<ISpecificationRepository<SampleEntity, Guid>> _specRepoMock;
        private readonly Mock<ICacheService> _cacheMock;
        private readonly Mock<ILogger<SampleService>> _loggerMock;

        private readonly ISampleQueryService _service;

        public SampleQueryServiceTests()
        {
            _specRepoMock = new Mock<ISpecificationRepository<SampleEntity, Guid>>();
            _cacheMock = new Mock<ICacheService>();
            _loggerMock = new Mock<ILogger<SampleService>>();

            _service = new SampleQueryService(
                _loggerMock.Object,
                _specRepoMock.Object,
                _cacheMock.Object
            );
        }

        [Fact]
        public async Task GetBySpecAsync_Should_Return_PagedResults()
        {
            // Arrange
            var list = new List<SampleEntity>
            {
                new SampleEntity { property1 = "a", CreatedAt = DateTime.UtcNow.AddMinutes(-2) },
                new SampleEntity { property1 = "a", CreatedAt = DateTime.UtcNow.AddMinutes(-1) },
                new SampleEntity { property1 = "a", CreatedAt = DateTime.UtcNow }
            };

            _specRepoMock.Setup(r => r.FindBySpecAsync(It.IsAny<SampleGetSpec>()))
                         .ReturnsAsync((list, list.Count));

            // Act
            var result = await _service.GetBySpecAsync("a", page: 1, pageSize: 2);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetCachedSamplesAsync_Should_Return_FromCache_WhenExists()
        {
            // Arrange
            var property1 = "cachedValue";
            var cachedList = new List<SampleEntity> { new SampleEntity { property1 = property1 } };
            _cacheMock.Setup(c => c.GetAsync<IReadOnlyList<SampleEntity>>(It.IsAny<string>()))
                      .ReturnsAsync(cachedList);

            // Act
            var result = await _service.GetCachedSamplesAsync(property1);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Single(result.Data);
            _cacheMock.Verify(c => c.GetAsync<IReadOnlyList<SampleEntity>>(It.IsAny<string>()), Times.Once);
            _specRepoMock.Verify(r => r.ListBySpecAsync(It.IsAny<SampleGetSpec>()), Times.Never);
        }

        [Fact]
        public async Task GetCachedSamplesAsync_Should_Query_And_SetCache_WhenNotCached()
        {
            // Arrange
            var property1 = "newValue";
            _cacheMock.Setup(c => c.GetAsync<IReadOnlyList<SampleEntity>>(It.IsAny<string>()))
                      .ReturnsAsync((IReadOnlyList<SampleEntity>)null);

            var list = new List<SampleEntity> { new SampleEntity { property1 = property1 } };
            _specRepoMock.Setup(r => r.ListBySpecAsync(It.IsAny<SampleGetSpec>()))
                         .ReturnsAsync(list);

            // Act
            var result = await _service.GetCachedSamplesAsync(property1);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Single(result.Data);
            _cacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>()), Times.Once);
        }
    }
}
