using Core.Application.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Provider
{
    public interface IDataScopeContextProvider
    {

        Task<DataScopeContext> GetAsync(CancellationToken ct);
    }

}
