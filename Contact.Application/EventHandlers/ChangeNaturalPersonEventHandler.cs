using Contact.Application.Interfaces;
using Contact.Domain.Helper;
using Core.Application.Abstractions.Caching.PublicService;
using Core.Application.Common.Events;
using HR.Domain.Events.Location;
using Microsoft.Extensions.Logging;
using People.Domain.Events;
using Polly;
using Polly.Registry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Application.EventHandlers
{
    public class ChangeNaturalPersonEventHandler : DomainEventHandler<ChangeNaturalPersonEvent>
    {
        private readonly ICachePublicService _cacheService;
        private readonly IReadOnlyPolicyRegistry<string> _policies;
        public ChangeNaturalPersonEventHandler(
            ICachePublicService cacheService, ILogger<DomainEventHandler<ChangeNaturalPersonEvent>> logger, IReadOnlyPolicyRegistry<string> policies) : base(logger) // لاگ استاندارد از کلاس پایه
        {
            _policies = policies;
            _cacheService = cacheService;
        }

        // فقط منطق اصلی هندل کردن رویداد اینجا نوشته می‌شود

        protected override async Task HandleEventAsync(ChangeNaturalPersonEvent _event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("ChangeNaturalPersonEventHandler In Contact Start!!!");
            var policy = _policies.Get<IAsyncPolicy>("DefaultRetry");
            await policy.ExecuteAsync(async ct =>
            {
                await _cacheService.RemoveByPatternAsync($"{CacheKeyHelper.PhoneBook_BaseChacheKey}:*");
            }, cancellationToken);
        }


    }
}
