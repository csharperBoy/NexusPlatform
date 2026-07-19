using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Common.EntityProperties
{
    public interface IHasEffectivePeriod
    {
        public DateTime? EffectiveFrom { get; }
        public DateTime? EffectiveTo { get; }
        public bool IsCurrent { get; }
        public void SetEffectiveFrom(DateTime? value);
        public void SetEffectiveTo(DateTime? value);
        public void SetIsCurrent(bool value);
        /*
         #region Impelement IHasEffectivePeriod
        public DateTime? EffectiveFrom { get; private set; }
        public DateTime? EffectiveTo { get; private set; }
        public bool IsCurrent { get; private set; }

         public void SetEffectiveFrom(DateTime? value)
        {
            EffectiveFrom = value;
            Touch();
        }

        public void SetEffectiveTo(DateTime? value)
        {
            EffectiveTo = value;
            Touch();
        }
        public void SetIsCurrent(bool value)
        {
            IsCurrent = value;
            Touch();
        }
        #endregion
         */
    }
    public static class EffectivePeriodExtention
    {
        public static bool IsActiveOn(this IHasEffectivePeriod entity, DateTime date)
        {
            return entity.EffectiveFrom >= date && (entity.EffectiveTo.HasValue || entity.EffectiveTo >= date);
        }
        public static bool IsCurrentlyActive(this IHasEffectivePeriod entity)
        {
            return entity.IsActiveOn(DateTime.Today);
        }
        public static void DoExpire(this IHasEffectivePeriod entity)
        {
            entity.SetEffectiveTo(DateTime.UtcNow.AddMinutes(-1));
            entity.SetIsCurrent(false);

        }
        public static void SetTemporalRange(this IHasEffectivePeriod entity, DateTime? effectiveFrom, DateTime? expiresAt)
        {
            if (effectiveFrom.HasValue && expiresAt.HasValue && effectiveFrom >= expiresAt)
                throw new ArgumentException("Effective from date must be before expiration date.");

            entity.SetEffectiveFrom(effectiveFrom);
            entity.SetEffectiveTo(expiresAt);
            
        }
    }
}
