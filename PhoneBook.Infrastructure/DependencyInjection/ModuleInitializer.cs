using Core.Application.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace PhoneBook.Infrastructure.DependencyInjection
{
   

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
                //var repo = scope.ServiceProvider.GetRequiredService<IRepository<PhoneBookDbContext, PhoneBookEntity, Guid>>();
                //var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork<PhoneBookDbContext>>();
                //await PhoneBookSeedData.SeedEntityAsync(repo, uow, _configuration, _logger);

                _logger.LogInformation("PhoneBook module initialization completed successfully.");
            }
            catch (Exception ex)
            {
                // 📌 ثبت خطا در صورت شکست عملیات
                _logger.LogError(ex, "An error occurred while initializing the PhoneBook module");
                throw;
            }
        }

        // 📌 متد StopAsync در پایان برنامه فراخوانی می‌شود (اینجا خالی است)
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
