using Contact.Application.Interfaces;
using Core.Application.Abstractions;
using Core.Application.Common.Events;
using HR.Domain.Events.Location;
using MediatR;
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

    public class RemoveLocationEventHandler : DomainEventHandler<RemoveLocationEvent>
    {
        private readonly IContactInternalService _service;
        private readonly IReadOnlyPolicyRegistry<string> _policies;
        public RemoveLocationEventHandler(IContactInternalService service, ILogger<DomainEventHandler<RemoveLocationEvent>> logger, IReadOnlyPolicyRegistry<string> policies)  : base(logger) // لاگ استاندارد از کلاس پایه
        {
            _service = service;
            _policies = policies;
        }

        // فقط منطق اصلی هندل کردن رویداد اینجا نوشته می‌شود

        protected override async Task HandleEventAsync(RemoveLocationEvent _event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("RemoveLocationEventHandler In Contact Start!!!");
            var policy = _policies.Get<IAsyncPolicy>("DefaultRetry");
            await policy.ExecuteAsync(async ct =>
            {
                await _service.DeActiveContactProfileAsync(_event.ProfileId);
                await _service.ExpireAllContactAsync(_event.ProfileId);
                await _service.SaveAsync();
            }, cancellationToken);
        }


    }
}
