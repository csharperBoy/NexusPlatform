
using Azure;
using Core.Application.Abstractions.Identity;
using Core.Application.Context;
using Core.Application.Provider;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Core.Infrastructure.DependencyInjection
{
    /*
    public class CoreModuleInitializer : IHostedService
    {
        private readonly ILogger<CoreModuleInitializer> _logger;
        private readonly IConfiguration _configuration;
        private readonly IDataScopeContextProvider _provider;
        private readonly DataScopeContext _context;
        //private readonly IUserPublicService _userService;

        public CoreModuleInitializer(
            IDataScopeContextProvider provider,
            ILogger<CoreModuleInitializer> logger, 
            IConfiguration configuration,
            //IUserPublicService userService
            DataScopeContext context
            )
        {
            _provider = provider;
            _context = context;
            _logger = logger;
            _configuration  = configuration;
            //_userService = userService;
        }

        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            
            var ctx = await _userService.GetInitializerUserContext();
            // مقداردهی Scoped Instance
            typeof(DataScopeContext)
                .GetProperty(nameof(DataScopeContext.UserId))!
                .SetValue(_context, ctx.UserId);

            typeof(DataScopeContext)
                .GetProperty(nameof(DataScopeContext.PersonId))!
                .SetValue(_context, ctx.PersonId);

            typeof(DataScopeContext)
                .GetProperty(nameof(DataScopeContext.OrganizationUnitIds))!
                .SetValue(_context, ctx.OrganizationUnitIds);

            typeof(DataScopeContext)
                .GetProperty(nameof(DataScopeContext.PositionIds))!
                .SetValue(_context, ctx.PositionIds);

            typeof(DataScopeContext)
                .GetProperty(nameof(DataScopeContext.RoleIds))!
                .SetValue(_context, ctx.RoleIds);

            typeof(DataScopeContext)
                .GetProperty(nameof(DataScopeContext.Permissions))!
                .SetValue(_context, ctx.Permissions);

        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
*/
}