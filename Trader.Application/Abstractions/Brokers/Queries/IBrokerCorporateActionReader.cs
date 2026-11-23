using Trader.ExternalApiClient;

namespace Trader.Application.Abstractions.Brokers.Queries;

public interface IBrokerCorporateActionReader : IBroker
{
    Task<IReadOnlyList<CorporateActionMarkDto>> GetCorporateActionsAsync(
        ExternalSystemSession session,
        string isin,
        DateTime from,
        DateTime to,
        CancellationToken ct = default);
}

public sealed record CorporateActionMarkDto(
    DateTime ExDateUtc,
    string Label,          // "D" | "C"
    string SourceId,       // "dividend20210721" | "capital20250422"
    string RawText,        // HTML
    long UnixSeconds);