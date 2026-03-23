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
    public class SetUserDataBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IUserDataContextProvider _provider;
        private readonly UserDataContext _context;

        public SetUserDataBehavior(
            IUserDataContextProvider provider,
            UserDataContext context)
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
            typeof(UserDataContext)
                .GetProperty(nameof(UserDataContext.UserId))!
                .SetValue(_context, ctx.UserId);

            typeof(UserDataContext)
                .GetProperty(nameof(UserDataContext.PersonId))!
                .SetValue(_context, ctx.PersonId);

            typeof(UserDataContext)
                .GetProperty(nameof(UserDataContext.OrganizationUnitIds))!
                .SetValue(_context, ctx.OrganizationUnitIds);

            typeof(UserDataContext)
                .GetProperty(nameof(UserDataContext.PositionIds))!
                .SetValue(_context, ctx.PositionIds);

            typeof(UserDataContext)
                .GetProperty(nameof(UserDataContext.RoleIds))!
                .SetValue(_context, ctx.RoleIds);

            typeof(UserDataContext)
                .GetProperty(nameof(UserDataContext.Permissions))!
                .SetValue(_context, ctx.Permissions);

            return await next();
        }
    }

}
