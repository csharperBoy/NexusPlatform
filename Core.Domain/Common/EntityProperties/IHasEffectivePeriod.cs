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
        public Task SetEffectiveFrom(DateTime? value);
        public Task SetEffectiveTo(DateTime? value);
        public Task SetIsCurrent(bool value);
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
        public static async Task DoExpire(this IHasEffectivePeriod entity)
        {
           await entity.SetEffectiveTo(DateTime.UtcNow.AddMinutes(-1));
           await entity.SetIsCurrent(false);
            await Task.CompletedTask;

        }
        public static async Task SetTemporalRange(this IHasEffectivePeriod entity, DateTime? effectiveFrom, DateTime? expiresAt)
        {
            if (effectiveFrom.HasValue && expiresAt.HasValue && effectiveFrom >= expiresAt)
                throw new ArgumentException("Effective from date must be before expiration date.");

           await entity.SetEffectiveFrom(effectiveFrom);
          await  entity.SetEffectiveTo(expiresAt);
            
        }
    }
}
