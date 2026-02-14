using Core.Application.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Holder
{
   /* public static class AccessContextHolder
    {
        private static readonly AsyncLocal<DataScopeContext?> _current = new();

        public static DataScopeContext Current
        {
            get => _current.Value
                ?? throw new InvalidOperationException("DataScopeContext not initialized");
            set => _current.Value = value;
        }
    }*/

}
