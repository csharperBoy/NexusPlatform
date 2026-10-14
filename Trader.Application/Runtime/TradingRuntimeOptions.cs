namespace Trader.Application.Runtime;

public sealed class TradingRuntimeOptions
{
    public const string SectionName = "Trader:Runtime";

    /// <summary>فاصله‌ی Heartbeat برای Stage 1+2+3.</summary>
    public TimeSpan HeartbeatInterval { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>فاصله‌ی Poll وضعیت سفارش‌ها (Stage 4).</summary>
    public TimeSpan OrderLifecycleInterval { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>ظرفیت Channel برای Signals.</summary>
    public int SignalChannelCapacity { get; set; } = 1000;

    /// <summary>حداکثر تعداد Order باز در هر Batch برای Poll.</summary>
    public int OrderPollBatchSize { get; set; } = 100;
}