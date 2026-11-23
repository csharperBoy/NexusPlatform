namespace Trader.Application.Abstractions.CorporateActions;

public interface ICorporateActionService
{
    /// <summary>
    /// Fetch CA marks از broker برای یک ISIN، parse، و persist آن‌هایی که جدیدند.
    /// Return: تعداد insert شده. Idempotent (dedupe by Isin+ExDate+Type+SourceId).
    /// </summary>
    Task<int> FetchAndSaveAsync(
        string isin, DateTime from, DateTime to, CancellationToken ct = default);
}