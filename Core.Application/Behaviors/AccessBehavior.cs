using Core.Application.Context;

using Core.Application.Provider;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Behaviors
{
    public class AccessBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IDataScopeContextProvider _provider;
        private readonly DataScopeContext _context;

        public AccessBehavior(
            IDataScopeContextProvider provider,
            DataScopeContext context)
        {
            _provider = provider;
            _context = context;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var ctx = await _provider.GetAsync(cancellationToken);

            // مقداردهی Scoped Instance
            typeof(DataScopeContext)
                .GetProperty(nameof(DataScopeContext.UserId))!
                .SetValue(_context, ctx.UserId);

            typeof(DataScopeContext)
                .GetProperty(nameof(DataScopeContext.PersonId))!
                .SetValue(_context, ctx.PersonId);

            typeof(DataScopeContext)
                .GetProperty(nameof(DataScopeContext.OrganizationUnitIds))!
                .SetValue(_context, ctx.OrganizationUnitIds);

            typeof(DataScopeContext)
                .GetProperty(nameof(DataScopeContext.PositionIds))!
                .SetValue(_context, ctx.PositionIds);

            typeof(DataScopeContext)
                .GetProperty(nameof(DataScopeContext.RoleIds))!
                .SetValue(_context, ctx.RoleIds);

            typeof(DataScopeContext)
                .GetProperty(nameof(DataScopeContext.Permissions))!
                .SetValue(_context, ctx.Permissions);

            return await next();
        }
    }

}
