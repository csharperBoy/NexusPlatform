using Core.Application.Abstractions.Events;
using Core.Infrastructure.Events;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Infrastructure.DependencyInjection
{
    public static class EventServiceCollectionExtensions
    {
        public static IServiceCollection AddOutboxServices(this IServiceCollection services)
        {
            // ثبت سرویس Outbox پایه
            services.AddTransient(typeof(IOutboxService<>), typeof(OutboxService<>));


            return services;
        }
    }
}
