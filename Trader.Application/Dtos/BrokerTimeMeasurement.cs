namespace Trader.Application.Dtos
{
    /// <summary>
    /// نتیجه اندازه‌گیری زمان سرور کارگزاری.
    /// </summary>
    public record BrokerTimeMeasurement(
        /// <summary>serverTs - clientTs (شامل offset ساعت + تأخیر رفت)</summary>
        long Diff,
        /// <summary>تأخیر یک‌طرفه تخمینی (RTT/2)</summary>
        long OneWayLatencyMs,
        /// <summary>کل RTT</summary>
        long RttMs);
}