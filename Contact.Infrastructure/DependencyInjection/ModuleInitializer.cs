using Contact.Infrastructure.Data;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Authorization.PublicService;
using Core.Application.Abstractions.Identity.PublicService;
using Core.Application.Abstractions.Navigation.PublicService;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace Contact.Infrastructure.DependencyInjection
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
                var resourceService = scope.ServiceProvider.GetRequiredService<IResourcePublicService>();
                var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionPublicService>();
                var roleService = scope.ServiceProvider.GetRequiredService<IRolePublicService>();
                var menuService = scope.ServiceProvider.GetRequiredService<IMenuPublicService>();
                await ContactSeedData.SeedContactForAuthorizationAsync(resourceService, permissionService, roleService, _logger);
                await ContactSeedData.SeedContactsForNavigationAsync(menuService, _logger);

                _logger.LogInformation("Contact module initialization completed successfully.");
            }
            catch (Exception ex)
            {
                // 📌 ثبت خطا در صورت شکست عملیات
                _logger.LogError(ex, "An error occurred while initializing the Contact module");
                throw;
            }
        }

        // 📌 متد StopAsync در پایان برنامه فراخوانی می‌شود (اینجا خالی است)
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
