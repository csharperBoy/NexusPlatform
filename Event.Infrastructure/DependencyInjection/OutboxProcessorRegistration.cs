using Core.Application.Abstractions.Events;
using Event.Infrastructure.Processor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event.Infrastructure.DependencyInjection
{
    public class OutboxProcessorRegistration : IOutboxProcessorRegistration
    {
        public IServiceCollection AddOutboxProcessor<TDbContext>(IServiceCollection services)
            where TDbContext : DbContext
        {
            services.AddHostedService<HybridOutboxProcessor<TDbContext>>();
            return services;
        }
    }
}
