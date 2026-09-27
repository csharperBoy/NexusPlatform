using Core.Domain.Common.EntityProperties;
using Trader.Domain.Enums;

namespace Trader.Domain.Entities
{
    public class SchedulePlan : BaseEntity
    {
        public string Name { get; private set; } = default!;
        public DateOnly Date { get; private set; }
        public bool Enabled { get; private set; }
        public TimeOnly AutoLoginAt { get; private set; }
        public TimeOnly AutoRefreshAt { get; private set; }

        /// <summary>
        /// پیش‌افتادگی اضافی (ms) — چقدر زودتر از زمان محاسبه‌شده fire کنیم.
        /// پیش‌فرض 0.
        /// </summary>
        public int FireLeadMs { get; private set; }
        public SchedulePlanStatus Status { get; private set; }
        public string? LastMessage { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? UpdatedAt { get; private set; }

        private readonly List<ScheduledOrder> _orders = new();
        public IReadOnlyCollection<ScheduledOrder> Orders => _orders.AsReadOnly();

        private SchedulePlan() { }

        public static SchedulePlan Create(
     string name,
     DateOnly date,
     bool enabled,
     TimeOnly autoLoginAt,
     TimeOnly autoRefreshAt,
     int fireLeadMs = 0)
        {
            return new SchedulePlan
            {
                Name = name,
                Date = date,
                Enabled = enabled,
                AutoLoginAt = autoLoginAt,
                AutoRefreshAt = autoRefreshAt,
                FireLeadMs = fireLeadMs < 0 ? 0 : fireLeadMs,
                Status = SchedulePlanStatus.Idle,
                CreatedAt = DateTimeOffset.UtcNow,
            };
        }

        public void SetInfo(
      string name,
      DateOnly date,
      bool enabled,
      TimeOnly autoLoginAt,
      TimeOnly autoRefreshAt,
      int fireLeadMs = 0)
        {
            Name = name;
            Date = date;
            Enabled = enabled;
            AutoLoginAt = autoLoginAt;
            AutoRefreshAt = autoRefreshAt;
            FireLeadMs = fireLeadMs < 0 ? 0 : fireLeadMs;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void Enable()
        {
            Enabled = true;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void Disable()
        {
            Enabled = false;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void SetStatus(SchedulePlanStatus status, string? message = null)
        {
            Status = status;
            LastMessage = message;
        }

        /* ─── مدیریت سفارش‌ها ─── */
        public ScheduledOrder AddOrder(
            Guid accountId,
            string symbolIsin,
            OrderSide side,
            OrderMode mode,
            long quantity,
            long totalValue,
            TimeOnly time)
        {
            var order = ScheduledOrder.Create(
                Id, accountId, symbolIsin,
                side, mode, quantity, totalValue, time);
            _orders.Add(order);
            return order;
        }

        public void RemoveOrder(Guid orderId)
            => _orders.RemoveAll(o => o.Id == orderId);

        public void ClearOrders() => _orders.Clear();

        public void ReplaceOrders(IEnumerable<ScheduledOrder> orders)
        {
            _orders.Clear();
            _orders.AddRange(orders);
        }
    }
}