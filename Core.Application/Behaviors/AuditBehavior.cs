using Core.Application.Abstractions.Auditing;
using Core.Application.Abstractions.Security;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Behaviors
{
    public class AuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IAuditService _auditService;
        private readonly ICurrentUserService _currentUser;

        public AuditBehavior(IAuditService auditService, ICurrentUserService currentUser)
        {
            _auditService = auditService;
            _currentUser = currentUser;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var response = await next();

            await _auditService.LogAsync(
                action: typeof(TRequest).Name,
                entityName: request.GetType().Name,
                entityId: Guid.NewGuid().ToString(), // یا از request استخراج کن
                userId: _currentUser.UserId,
                changes: request);

            return response;
        }
    }

}
