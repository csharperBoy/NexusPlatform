using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Abstractions.Events
{
    public interface IOutboxProcessorRegistration
    {
        IServiceCollection AddOutboxProcessor<TDbContext>(IServiceCollection services)
            where TDbContext : DbContext;
    }
}
