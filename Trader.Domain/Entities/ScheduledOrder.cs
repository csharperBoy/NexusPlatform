using Core.Domain.Common.EntityProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trader.Domain.Enums;

namespace Trader.Domain.Entities
{
    
    public class ScheduledOrder : BaseEntity
    {
        public Guid PlanId { get; private set; }
        public Guid AccountId { get; private set; }
        public string SymbolIsin { get; private set; } = default!;

        public OrderSide Side { get; private set; }
        public OrderMode Mode { get; private set; }
        public long Quantity { get; private set; }
        public long TotalValue { get; private set; }
        public TimeOnly Time { get; private set; }

        public bool Fired { get; private set; }
        public DateTimeOffset? FiredAt { get; private set; }
        public string? Result { get; private set; }

        private ScheduledOrder() { }

        internal static ScheduledOrder Create(
            Guid planId,
            Guid accountId,
            string symbolIsin,
            OrderSide side,
            OrderMode mode,
            long quantity,
            long totalValue,
            TimeOnly time)
        {
            return new ScheduledOrder
            {
                PlanId = planId,
                AccountId = accountId,
                SymbolIsin = symbolIsin,
                Side = side,
                Mode = mode,
                Quantity = quantity,
                TotalValue = totalValue,
                Time = time,
                Fired = false,
            };
        }

        public void MarkFired(string? result = null)
        {
            Fired = true;
            FiredAt = DateTimeOffset.UtcNow;
            Result = result;
        }

        public void ResetFired()
        {
            Fired = false;
            FiredAt = null;
            Result = null;
        }
    }
}
