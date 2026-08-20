using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Common.EntityProperties
{
    public interface ISoftRemovable
    {
        public bool IsRemove { get;  }

        public Task SetIsRemove(bool value);

        /*
        #region ISoftRemovable Impelement
        public bool IsRemove { get; private set; } = false;

        public async Task SetIsRemove(bool value)
        {
            IsRemove = value;
            Touch();
            await Task.CompletedTask;
        }
        #endregion
        */
    }
    public static class SoftRemovableExtention
    {
        public static async Task SoftRemove(this ISoftRemovable entity  )
        {
            await entity.SetIsRemove(true);
            
            await Task.CompletedTask;

        }
    }
}
