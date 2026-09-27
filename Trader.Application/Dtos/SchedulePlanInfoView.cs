namespace Trader.Application.Dtos
{
    public class SchedulePlanInfoView
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Date { get; set; } = default!;         // "YYYY-MM-DD"
        public bool Enabled { get; set; }
        public string AutoLoginAt { get; set; } = default!;  // "HH:MM:SS"
        public string AutoRefreshAt { get; set; } = default!;

        /// <summary>پیش‌افتادگی اضافی (ms) — پیش‌فرض 0</summary>
        public int FireLeadMs { get; set; }
        public List<ScheduledOrderInfoView> Orders { get; set; } = new();

        public string Status { get; set; } = "idle";
        public string? Message { get; set; }
    }
}