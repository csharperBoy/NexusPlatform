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
    /*
     📌 SampleQueryServiceTests
     --------------------------
     این کلاس تست واحد (Unit Test) برای SampleQueryService است.
     هدف آن اطمینان از صحت رفتار سرویس Query در سناریوهای مختلف است.

     ✅ نکات کلیدی:
     - از Moq برای شبیه‌سازی وابستگی‌ها استفاده می‌کنیم:
       1. ISpecificationRepository → شبیه‌سازی دسترسی به دیتابیس.
       2. ICacheService → شبیه‌سازی کش.
       3. ILogger → شبیه‌سازی لاگ.
     - سرویس اصلی (ISampleQueryService) با این Mockها ساخته می‌شود.
     - سه تست اصلی وجود دارد:
       1. بررسی صفحه‌بندی (Paging).
       2. بررسی بازگشت داده از Cache.
       3. بررسی درج داده در Cache وقتی داده‌ای وجود ندارد.

     📌 نتیجه:
     این تست‌ها تضمین می‌کنند که SampleQueryService هم از نظر منطق صفحه‌بندی
     و هم از نظر استفاده بهینه از Cache درست کار می‌کند.
    */

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

            // 📌 ساخت سرویس اصلی با Mockها
            _service = new SampleQueryService(
                _loggerMock.Object,
                _specRepoMock.Object,
                _cacheMock.Object
            );
        }

        [Fact]
        public async Task GetBySpecAsync_Should_Return_PagedResults()
        {
            /*
             📌 هدف تست:
             بررسی اینکه متد GetBySpecAsync داده‌ها را صفحه‌بندی می‌کند.
             */

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
            Assert.Equal(2, result.Data.Count); // فقط ۲ آیتم باید برگردد
        }

        [Fact]
        public async Task GetCachedSamplesAsync_Should_Return_FromCache_WhenExists()
        {
            /*
             📌 هدف تست:
             بررسی اینکه اگر داده در Cache موجود باشد،
             سرویس بدون Query به دیتابیس همان داده را برمی‌گرداند.
             */

            // Arrange
            var property1 = "cachedValue";
            var cachedList = new List<SampleEntity> { new SampleEntity { property1 = property1 } };
            _cacheMock.Setup(c => c.GetAsync<IReadOnlyList<SampleEntity>>(It.IsAny<string>()))
                      .ReturnsAsync(cachedList);

            // Act
            var result = await _service.GetCachedSamplesAsync(property1);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Single(result.Data); // فقط یک آیتم باید برگردد
            _cacheMock.Verify(c => c.GetAsync<IReadOnlyList<SampleEntity>>(It.IsAny<string>()), Times.Once);
            _specRepoMock.Verify(r => r.ListBySpecAsync(It.IsAny<SampleGetSpec>()), Times.Never); // دیتابیس نباید صدا زده شود
        }

        [Fact]
        public async Task GetCachedSamplesAsync_Should_Query_And_SetCache_WhenNotCached()
        {
            /*
             📌 هدف تست:
             بررسی اینکه اگر داده در Cache موجود نباشد،
             سرویس داده را از Repository می‌خواند و سپس در Cache ذخیره می‌کند.
             */

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
            _cacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>()), Times.Once); // داده باید در Cache ذخیره شود
        }
    }
}
