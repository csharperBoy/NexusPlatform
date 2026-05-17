using Base.Domain.Entities;
using Base.Infrastructure.Data;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Authorization.PublicService;
using Core.Application.Abstractions.Identity.PublicService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Infrastructure.DependencyInjection
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
                _logger.LogInformation("Starting base module initialization...");

                var resourcePublicService = scope.ServiceProvider.GetRequiredService<IResourcePublicService>();
                var permissionPublicService = scope.ServiceProvider.GetRequiredService<IPermissionPublicService>();
                var roleService = scope.ServiceProvider.GetRequiredService<IRolePublicService>();

                // روش 1: استفاده از متد یکپارچه
                await BaseSeedData.SeedBaseForAuthorizationAsync(resourcePublicService,
                    permissionPublicService, roleService,
                    _logger);

                // 📌 اجرای Seed داده‌ها با Repository + UnitOfWork
                var repo = scope.ServiceProvider.GetRequiredService<IRepository<BaseDbContext, Menu, Guid>>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork<BaseDbContext>>();
                await BaseSeedData.SeedBaseAsync(repo, uow, _configuration, _logger);


                _logger.LogInformation("Base module initialization completed successfully.");
            }
            catch (Exception ex)
            {
                // 📌 ثبت خطا در صورت شکست عملیات
                _logger.LogError(ex, "An error occurred while initializing the Base module");
                throw;
            }
        }

        // 📌 متد StopAsync در پایان برنامه فراخوانی می‌شود (اینجا خالی است)
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
