namespace Trader.Application.Abstractions.Runtime;

public interface IMarketDataIngestionStage
{
    /// <summary>
    /// یک بار اجرای Stage 1: جمع‌آوری Symbols از Runهای فعال، Fetch و ذخیره در Store.
    /// </summary>
    Task RunOnceAsync(CancellationToken ct = default);
}