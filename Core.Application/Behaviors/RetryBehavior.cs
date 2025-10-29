using MediatR;
using Polly;
using Polly.Registry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Behaviors
{
    public class RetryBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IReadOnlyPolicyRegistry<string> _policies;

        public RetryBehavior(IReadOnlyPolicyRegistry<string> policies)
        {
            _policies = policies;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
        {
            var policy = _policies.Get<IAsyncPolicy>("DefaultRetry");
            return await policy.ExecuteAsync(async _ => await next(), ct);
        }
    }
}
