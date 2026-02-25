using Core.Domain.Attributes;
using Core.Domain.Common;
using Core.Domain.Common.EntityProperties;
using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraderServer.Domain.Entities
{
    [SecuredResource("Collector.SnapShotFromStockTrading")]
    public class SnapShotFromStockTrading :BaseEntity, IAuditableEntity, IDataScopedEntity, IAggregateRoot
    {
        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر
        #endregion
        #region IDataScopedEntity Impelement
        public Guid? OwnerOrganizationUnitId { get; protected set; }
        public Guid? OwnerPositionId { get; protected set; }
        public Guid? OwnerPersonId { get; protected set; }
        public Guid? OwnerUserId { get; protected set; }

        public void SetOwners(Guid? userId, Guid? personId, Guid? positiontId, Guid? orgUnitId)
        {
            OwnerUserId = userId;
            OwnerPersonId = personId;
            OwnerPositionId = positiontId;
            OwnerOrganizationUnitId = orgUnitId;
        }
        public void SetPersonOwner(Guid personId)
        {
            OwnerPersonId = personId;
        }
        public void SetUserOwner(Guid userId)
        {
            OwnerUserId = userId;
        }
        public void SetPositionOwner(Guid positiontId)
        {
            OwnerPositionId = positiontId;
        }
        public void SetOrganizationUnitOwner(Guid orgUnitId)
        {
            OwnerOrganizationUnitId = orgUnitId;
        }
        #endregion

        /// <summary>
        /// کلید خارجی به سهام
        /// </summary>
        public Guid FkStockId { get; set; }
        /// <summary>
        /// شناسه
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// تاریخ و ساعت
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// آخرین معامله
        /// </summary>
        public int? LastPrice { get; set; }

        /// <summary>
        /// قیمت پایانی
        /// </summary>
        public int? ClosePrice { get; set; }

        /// <summary>
        /// تعداد معاملات
        /// </summary>
        public long? TotalTradesCount { get; set; }

        /// <summary>
        /// حجم معاملات
        /// </summary>
        public long? TotalTradedVolume { get; set; }

        /// <summary>
        /// ارزش معاملات
        /// </summary>
        public long? TotalTradedValue { get; set; }

        /// <summary>
        /// ارزش بازار
        /// </summary>
        public long? MarketCapitalization { get; set; }

        public int? BuyOrder1Price { get; set; }

        public long? BuyOrder1Volume { get; set; }

        public long? BuyOrder1Count { get; set; }

        public int? BuyOrder2Price { get; set; }

        public long? BuyOrder2Volume { get; set; }

        public long? BuyOrder2Count { get; set; }

        public int? BuyOrder3Price { get; set; }

        public long? BuyOrder3Volume { get; set; }

        public long? BuyOrder3Count { get; set; }

        public int? BuyOrder4Price { get; set; }

        public long? BuyOrder4Volume { get; set; }

        public long? BuyOrder4Count { get; set; }

        public int? BuyOrder5Price { get; set; }

        public long? BuyOrder5Volume { get; set; }

        public long? BuyOrder5Count { get; set; }

        public int? SellOrder1Price { get; set; }

        public long? SellOrder1Volume { get; set; }

        public long? SellOrder1Count { get; set; }

        public int? SellOrder2Price { get; set; }

        public long? SellOrder2Volume { get; set; }

        public long? SellOrder2Count { get; set; }

        public int? SellOrder3Price { get; set; }

        public long? SellOrder3Volume { get; set; }

        public long? SellOrder3Count { get; set; }

        public int? SellOrder4Price { get; set; }

        public long? SellOrder4Volume { get; set; }

        public long? SellOrder4Count { get; set; }

        public int? SellOrder5Price { get; set; }

        public long? SellOrder5Volume { get; set; }

        public long? SellOrder5Count { get; set; }
        /// <summary>
        /// مجموع حجم سفارشات خرید
        /// </summary>
        public long? TotalBuyOrderVolume { get; set; }
        /// <summary>
        /// مجموع حجم سفارشات فروش
        /// </summary>
        public long? TotalSellOrderVolume { get; set; }
        /// <summary>
        /// مجموع تعداد سفارشات خرید
        /// </summary>
        public long? TotalBuyOrdersCount { get; set; }
        /// <summary>
        /// مجموع تعداد سفارشات فروش
        /// </summary>
        public long? TotalSellOrdersCount { get; set; }
        /// <summary>
        /// مجموع حجم خرید حقیقی
        /// </summary>
        public long? TotalBuyTruePersonalityVolume { get; set; }
        /// <summary>
        /// مجموع تعداد خرید حقیقی
        /// </summary>
        public long? TotalBuyTruePersonalityCount { get; set; }
        /// <summary>
        /// مجموع حجم فروش حقیقی
        /// </summary>
        public long? TotalSellTruePersonalityVolume { get; set; }
        /// <summary>
        /// مجموع تعداد فروش حقیقی
        /// </summary>
        public long? TotalSellTruePersonalityCount { get; set; }

        /// <summary>
        /// مجموع حجم خرید حقوقی
        /// </summary>
        public long? TotalBuyLegalPersonalityVolume { get; set; }
        /// <summary>
        /// مجموع تعداد خرید حقوقی
        /// </summary>
        public long? TotalBuyLegalPersonalityCount { get; set; }
        /// <summary>
        /// مجموع حجم فروش حقوقی
        /// </summary>
        public long? TotalSellLegalPersonalityVolume { get; set; }
        /// <summary>
        /// مجموع تعداد فروش حقوقی
        /// </summary>
        public long? TotalSellLegalPersonalityCount { get; set; }

    }
}
