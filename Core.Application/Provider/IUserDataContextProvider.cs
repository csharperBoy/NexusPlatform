using Core.Application.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Provider
{
    public interface IUserDataContextProvider
    {

        Task<UserDataContext> GetAsync(CancellationToken ct);
    }

}
