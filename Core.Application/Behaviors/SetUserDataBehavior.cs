using Core.Application.Context;

using Core.Application.Provider;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
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

            if (_context.UserId == Guid.Empty)
            {
                await _provider.SetUserData(cancellationToken);
            }

           
            return await next();
        }
    }

}
