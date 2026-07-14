using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Common.EntityProperties
{
    public interface IHasEffectivePeriod
    {
        public DateOnly EffectiveFrom { get;  }
        public DateOnly? EffectiveTo { get; }
        public bool IsCurrent { get;  }
        /*
         #region Impelement IHasEffectivePeriod
        public DateOnly EffectiveFrom { get; private set; }
        public DateOnly? EffectiveTo { get; private set; }
        public bool IsCurrent { get; private set; }

        #endregion
         */
    }
    public static class EffectivePeriodExtention
    {
        public static bool IsActiveOn(this IHasEffectivePeriod entity , DateOnly date)
        {
            return entity.EffectiveFrom >= date && (entity.EffectiveTo.HasValue || entity.EffectiveTo >= date);
        }
        public static bool IsCurrentlyActive(this IHasEffectivePeriod entity)
        {
            return entity.IsActiveOn(DateOnly.FromDateTime( DateTime.Today));
        }
    }
}
