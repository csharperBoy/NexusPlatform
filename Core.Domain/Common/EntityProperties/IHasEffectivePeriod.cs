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
    public static class EffectivePeriodExtensions
    {
        /// <summary>
        /// بررسی فعال بودن رکورد در یک تاریخ مشخص
        /// </summary>
        public static bool IsActiveOn(this IHasEffectivePeriod entity, DateTime date)
        {
            // ۱. آیا شروع شده است؟ (بدون تاریخ شروع یا شروع در گذشته/امروز)
            bool hasStarted = !entity.EffectiveFrom.HasValue || entity.EffectiveFrom <= date;

            // ۲. آیا منقضی نشده است؟ (بدون تاریخ پایان یا پایان در آینده/امروز)
            bool hasNotEnded = !entity.EffectiveTo.HasValue || entity.EffectiveTo >= date;

            return hasStarted && hasNotEnded;
        }

        /// <summary>
        /// بررسی فعال بودن در لحظه کنونی (بر اساس UTC)
        /// </summary>
        public static bool IsCurrentlyActive(this IHasEffectivePeriod entity)
        {
            return entity.IsActiveOn(DateTime.UtcNow);
        }

        /// <summary>
        /// منقضی کردن رکورد
        /// </summary>
        public static void DoExpire(this IHasEffectivePeriod entity)
        {
            entity.SetEffectiveTo(DateTime.UtcNow);
            entity.SetIsCurrent(false);
        }

        /// <summary>
        /// تنظیم بازه زمانی با اعتبارسنجی
        /// </summary>
        public static void SetTemporalRange(this IHasEffectivePeriod entity, DateTime? effectiveFrom, DateTime? expiresAt)
        {
            if (effectiveFrom.HasValue && expiresAt.HasValue && effectiveFrom >= expiresAt)
                throw new ArgumentException("Effective from date must be before expiration date.");

            entity.SetEffectiveFrom(effectiveFrom);
            entity.SetEffectiveTo(expiresAt);
        }
    }
}
