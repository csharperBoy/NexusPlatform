using Core.Application.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sample.Application.Interfaces;
using Sample.Domain.Entities;


namespace Sample.Infrastructure.Data
{
    public static class SampleSeedData
    {
        public static async Task SeedEntityAsync(
            IRepository<SampleDbContext, SampleEntity, Guid> repository,
            IUnitOfWork<SampleDbContext> unitOfWork,
            IConfiguration config,
            ILogger logger)
        {
            // بررسی وجود داده
            var exists = await repository.ExistsAsync(e => e.property1 == "SeededValue1");
            if (!exists)
            {
                var samples = new List<SampleEntity>
                {
                    new SampleEntity { property1 = "SeededValue1" },
                    new SampleEntity { property1 = "SeededValue2" }
                };

                await repository.AddRangeAsync(samples);
                await unitOfWork.SaveChangesAsync();

                logger.LogInformation("✅ Sample seed data inserted successfully via Repository + UnitOfWork.");
            }
            else
            {
                logger.LogInformation("ℹ️ Sample seed data already exists, skipping.");
            }
        }

    }
}