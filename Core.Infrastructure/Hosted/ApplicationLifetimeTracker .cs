using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Infrastructure.Hosted
{
    public class ApplicationLifetimeTracker : IHostedService
    {
        private readonly IHostApplicationLifetime _lifetime;
      
        public bool IsStarted
        {
            get { return _lifetime.ApplicationStarted.IsCancellationRequested; }
            set { IsStarted = value; }
        }


        public ApplicationLifetimeTracker(IHostApplicationLifetime lifetime)
        {
            _lifetime = lifetime;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
           
            // وقتی میزبانی شروع می‌شود، فیلد را به true تغییر می‌دهیم
            //_lifetime.ApplicationStarted.Register(() => IsStarted = true);
            //IsStarted = true;
            return Task.CompletedTask;

           
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            //_lifetime.ApplicationStarted.Register(() => IsStarted = false);
            //IsStarted = false;
            await Task.CompletedTask;
        } 
    }
}
