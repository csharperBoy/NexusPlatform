using Contact.Application.Interfaces;
using Contact.Domain.Helper;
using Core.Application.Abstractions.Caching.PublicService;
using Core.Application.Common.Events;
using HR.Domain.Events.Employment;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Application.EventHandlers
{

    public class RemoveEmploymentEventHandler : DomainEventHandler<RemoveEmploymentEvent>
    {
        private readonly IContactInternalService _service;
        private readonly ICachePublicService _cacheService;
        private readonly IReadOnlyPolicyRegistry<string> _policies;
        public RemoveEmploymentEventHandler(IContactInternalService service,
            ICachePublicService cacheService,
            ILogger<DomainEventHandler<RemoveEmploymentEvent>> logger, IReadOnlyPolicyRegistry<string> policies) : base(logger) // لاگ استاندارد از کلاس پایه
        {
            _service = service;
            _cacheService = cacheService;
            _policies = policies;
        }

        // فقط منطق اصلی هندل کردن رویداد اینجا نوشته می‌شود

        protected override async Task HandleEventAsync(RemoveEmploymentEvent _event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("RemoveEmploymentEventHandler In Contact Start!!!");
            var policy = _policies.Get<IAsyncPolicy>("DefaultRetry");
            await policy.ExecuteAsync(async ct =>
            {
                await _service.DeActiveContactProfileAsync(_event.FkContactProfileId);
                await _service.ExpireAllContactAsync(_event.FkContactProfileId);
                await _service.SaveAsync();
                await _cacheService.RemoveByPatternAsync($"{CacheKeyHelper.PhoneBook_BaseChacheKey}:*"); // حذف کش مرتبط با پروفایل تماس
            }, cancellationToken);
        }


    }

}
