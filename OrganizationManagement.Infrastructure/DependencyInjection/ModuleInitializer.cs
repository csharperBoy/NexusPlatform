using Core.Application.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrganizationManagement.Infrastructure.Data;
namespace OrganizationManagement.Infrastructure.DependencyInjection
{
    /*
     📌 ModuleInitializer
     --------------------
     این کلاس یک Hosted Service است که در زمان راه‌اندازی برنامه (Startup) اجرا می‌شود
     و وظیفه‌اش مقداردهی اولیه (Initialization) ماژول Sample است.

     ✅ نکات کلیدی:
     - از IHostedService ارث‌بری می‌کند → یعنی در زمان شروع و پایان برنامه اجرا می‌شود.
     - در متد StartAsync:
       1. یک Scope جدید از DI Container ساخته می‌شود.
       2. سرویس‌های مورد نیاز (Repository و UnitOfWork) دریافت می‌شوند.
       3. متد SeedEntityAsync فراخوانی می‌شود تا داده‌های اولیه در دیتابیس درج شوند.
       4. لاگ‌ها وضعیت عملیات را ثبت می‌کنند.
     - در متد StopAsync هیچ عملیات خاصی انجام نمی‌شود (فقط Task.CompletedTask برمی‌گرداند).

     🛠 جریان کار:
     1. برنامه اجرا می‌شود.
     2. Hosted Service فعال شده و متد StartAsync اجرا می‌شود.
     3. داده‌های اولیه بررسی و در صورت نیاز درج می‌شوند.
     4. لاگ موفقیت یا خطا ثبت می‌شود.
     5. در پایان برنامه، متد StopAsync فراخوانی می‌شود (اینجا خالی است).

     📌 نتیجه:
     این کلاس تضمین می‌کند که ماژول Sample همیشه داده‌های اولیه مورد نیازش را داشته باشد
     و در زمان راه‌اندازی برنامه آماده‌ی استفاده باشد.
    */

    public class ModuleInitializer : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ModuleInitializer> _logger;
        private readonly IConfiguration _configuration;

        public ModuleInitializer(IServiceProvider serviceProvider, ILogger<ModuleInitializer> logger, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                _logger.LogInformation("Starting sample module initialization...");

                // 📌 اجرای Seed داده‌ها با Repository + UnitOfWork
                //var repo = scope.ServiceProvider.GetRequiredService<IRepository<OrganizationManagementDbContext, SampleEntity, Guid>>();
                //var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork<SampleDbContext>>();
                //await OrganizationManagementSeedData.SeedEntityAsync(repo, uow, _configuration, _logger);

                _logger.LogInformation("Sample module initialization completed successfully.");
            }
            catch (Exception ex)
            {
                // 📌 ثبت خطا در صورت شکست عملیات
                _logger.LogError(ex, "An error occurred while initializing the Sample module");
                throw;
            }
        }

        // 📌 متد StopAsync در پایان برنامه فراخوانی می‌شود (اینجا خالی است)
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
