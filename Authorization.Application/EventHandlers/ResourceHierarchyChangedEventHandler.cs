using Authorization.Application.Interfaces;
using Authorization.Domain.Events;
using Core.Application.Abstractions.Caching.PublicService;
using Core.Application.Common.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;
namespace Authorization.Application.EventHandlers
{
    
    
    public class ResourceHierarchyChangedEventHandler : DomainEventHandler<ResourceHierarchyChangedEvent>
    {
        private readonly IResourceInternalService _resourceService;
        private readonly IReadOnlyPolicyRegistry<string> _policies;

        private readonly ICachePublicService _cacheService;
        public ResourceHierarchyChangedEventHandler(
            IResourceInternalService resourceService, ICachePublicService cacheService,
            ILogger<DomainEventHandler<ResourceHierarchyChangedEvent>> logger,
            IReadOnlyPolicyRegistry<string> policies)
            : base(logger) // لاگ استاندارد از کلاس پایه
        {
            _resourceService = resourceService;
            _cacheService = cacheService;
            _policies = policies;
        }

        // فقط منطق اصلی هندل کردن رویداد اینجا نوشته می‌شود
        protected override async Task HandleEventAsync(ResourceHierarchyChangedEvent ev, CancellationToken cancellationToken)
        {

            var policy = _policies.Get<IAsyncPolicy>("DefaultRetry");
            await policy.ExecuteAsync(async ct =>
            {
                var cacheKey = "auth:resourcetree:full";
                await _cacheService.RemoveByPatternAsync(cacheKey);
            }, cancellationToken);
        }
    }
    
   /* public class ResourceHierarchyChangedEventHandler : INotificationHandler<ResourceHierarchyChangedEvent>
    {
        private readonly IResourceInternalService _resourceService;
        private readonly IReadOnlyPolicyRegistry<string> _policies;

        private readonly ICacheService _cacheService;
        public ResourceHierarchyChangedEventHandler(
           IResourceInternalService resourceService, ICacheService cacheService,
            ILogger<DomainEventHandler<ResourceHierarchyChangedEvent>> logger,
            IReadOnlyPolicyRegistry<string> policies)
        {
            _resourceService = resourceService;
            _cacheService = cacheService;
            _policies = policies;
        }


        public async Task Handle(ResourceHierarchyChangedEvent notification, CancellationToken cancellationToken)
        {
            var policy = _policies.Get<IAsyncPolicy>("DefaultRetry");
            await policy.ExecuteAsync(async ct =>
            {
                var cacheKey = "auth:resourcetree:full";
                await _cacheService.RemoveByPatternAsync(cacheKey);
            }, cancellationToken);
        }
    }*/
}