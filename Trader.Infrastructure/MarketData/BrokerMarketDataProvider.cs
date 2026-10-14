using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Brokers;
using Trader.Application.Abstractions.Brokers.Queries;
using Trader.Application.Abstractions.MarketData;
using Trader.Application.Dtos;
using Trader.Application.Dtos.MarketData;
using Trader.Application.Runtime;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.MarketData;

/// <summary>
/// Market Data از طریق Broker API.
/// برای Phase 1 فقط EasyTrader.
/// Account مناسب را داخلی انتخاب می‌کند.
/// </summary>
public sealed class BrokerMarketDataProvider : IMarketDataProvider
{
    private static readonly HashSet<MarketType> _supported = new()
    {
        MarketType.IranStock,
        MarketType.IranOption
    };

    private readonly ITraderUnitOfWork<TraderDbContext> _uow;
    private readonly IBrokerClientFactory _brokerFactory;
    private readonly IBrokerSessionProvider _sessionProvider;
    private readonly ILogger<BrokerMarketDataProvider> _logger;

    public BrokerMarketDataProvider(
        ITraderUnitOfWork<TraderDbContext> uow,
        IBrokerClientFactory brokerFactory,
        IBrokerSessionProvider sessionProvider,
        ILogger<BrokerMarketDataProvider> logger)
    {
        _uow = uow;
        _brokerFactory = brokerFactory;
        _sessionProvider = sessionProvider;
        _logger = logger;
    }

    public string ProviderName => "BrokerMarketData";
    public IReadOnlySet<MarketType> SupportedMarkets => _supported;

    public async Task<MarketSnapshot?> GetSnapshotAsync(
        string isin, CancellationToken ct = default)
    {
        var list = await GetBatchSnapshotsAsync(new[] { isin }, ct);
        return list.Count > 0 ? list[0] : null;
    }

    public async Task<IReadOnlyList<MarketSnapshot>> GetBatchSnapshotsAsync(
        IReadOnlyList<string> isins, CancellationToken ct = default)
    {
        if (isins.Count == 0)
            return Array.Empty<MarketSnapshot>();

        var account = await PickAccountAsync(ct);
        if (account is null)
        {
            _logger.LogWarning(
                "No eligible TraderAccount found for market data. Skipping ingestion.");
            return Array.Empty<MarketSnapshot>();
        }

        ExternalApiClient.ExternalSystemSession session;
        try
        {
            session = await _sessionProvider.GetSessionAsync(account.Id, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to acquire session for account {AccountId}.", account.Id);
            return Array.Empty<MarketSnapshot>();
        }

        var reader = _brokerFactory.Get<IBrokerMarketDataReader>(account.Broker);

        List<BatchMarketDataItemDto> dtos;
        try
        {
            dtos = await reader.GetBatchMarketDataAsync(session, isins, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Batch market data fetch failed for {Count} symbols.", isins.Count);
            return Array.Empty<MarketSnapshot>();
        }

        var now = DateTime.UtcNow;
        var source = ProviderName;

        var snapshots = new List<MarketSnapshot>(dtos.Count);
        foreach (var dto in dtos)
        {
            var snap = MapToSnapshot(dto, now, source);
            if (snap is not null)
                snapshots.Add(snap);
        }

        _logger.LogDebug(
            "Fetched {Count}/{Requested} snapshots from broker.",
            snapshots.Count, isins.Count);

        return snapshots;
    }
    public async Task<IReadOnlyList<Candle>> GetCandlesAsync(
    string isin, TimeFrame timeFrame, int count, CancellationToken ct = default)
    {
        if (count <= 0) return Array.Empty<Candle>();

        var account = await PickAccountAsync(ct);
        if (account is null) return Array.Empty<Candle>();

        var session = await _sessionProvider.GetSessionAsync(account.Id, ct);
        var reader = _brokerFactory.Get<IBrokerMarketDataReader>(account.Broker);

        // محاسبه‌ی روزهای مورد نیاز بر اساس TimeFrame و تعداد
        var intervalMinutes = (int)timeFrame;
        var totalMinutes = count * intervalMinutes;
        // در بازار ایران روزانه حدود ۴ ساعت معاملات مؤثر است (۲۴۰ دقیقه)
        // برای محافظه‌کاری ۲۴۰ دقیقه فرض می‌کنیم، تا تعداد روز کافی باشد
        const int tradingMinutesPerDay = 240;
        var days = Math.Max(1, (totalMinutes / tradingMinutesPerDay) + 2);

        List<CandleDto> raw;
        try
        {
            raw = await reader.GetCandlesAsync(session, isin, days, intervalMinutes, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Candle fetch failed for {Isin} tf={TimeFrame}", isin, timeFrame);
            return Array.Empty<Candle>();
        }

        if (raw.Count == 0) return Array.Empty<Candle>();

        // آخرین {count} کندل را برمی‌گردانیم (جدیدترین‌ها)
        var take = Math.Min(count, raw.Count);
        var slice = raw.Count > take
            ? raw.GetRange(raw.Count - take, take)
            : raw;

        var result = new List<Candle>(slice.Count);
        foreach (var dto in slice)
        {
            try
            {
                result.Add(new Candle(
                    OpenTime: DateTimeOffset.FromUnixTimeSeconds(dto.T).UtcDateTime,
                    Open: dto.O,
                    High: dto.H,
                    Low: dto.L,
                    Close: dto.C,
                    Volume: dto.V));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _logger.LogWarning(ex,
                    "Invalid candle timestamp {T} for {Isin}; skipping.", dto.T, isin);
            }
        }

        return result;
    }

    /* ═══════════════════ Helpers ═══════════════════ */

    /// <summary>
    /// برای Phase 1: اولین Account با Broker=EasyTrader.
    /// در آینده می‌تواند از Config یا Active Run Contexts بیاید.
    /// </summary>
    private async Task<TraderAccount?> PickAccountAsync(CancellationToken ct)
    {
        var accounts = await _uow.Accounts.GetAllAsync();
        return accounts.FirstOrDefault(a => a.Broker == BrokerType.EasyTrader);
    }

    private MarketSnapshot? MapToSnapshot(
    BatchMarketDataItemDto dto, DateTime capturedAt, string source)
    {
        var isin = dto.SymbolIsin;
        if (string.IsNullOrWhiteSpace(isin))
        {
            _logger.LogWarning("Batch item without SymbolIsin; skipping.");
            return null;
        }

        return new MarketSnapshot(
            Isin: isin,
            SymbolName: null, // در DTO batch موجود نیست

            StateCode: dto.StateCode,

            LastTradedPrice: dto.LastTradedPrice,
            ClosingPrice: dto.ClosingPrice,
            PreviousDayClosingPrice: dto.FeeOfPreviousDaysClosingPrice,
            PriceVar: dto.PriceVar,
            FirstTradedPrice: dto.FirstTradedPrice,
            HighPrice: dto.HighPrice,
            LowPrice: dto.LowPrice,
            HighAllowedPrice: dto.HighAllowedPrice,
            LowAllowedPrice: dto.LowAllowedPrice,
            PriceTickSize: dto.PriceTickSize,

            BestBuyPrice: dto.BestBuyPrice,
            BestBuyQuantity: dto.BestBuyQuantity,
            BestSellPrice: dto.BestSellPrice,
            BestSellQuantity: dto.BestSellQuantity,

            BuyRatio: dto.BuyRatio,
            SellRatio: dto.SellRatio,
            MinValidBuyVolume: dto.MinValidBuyVolume,
            MaxValidBuyVolume: dto.MaxValidBuyVolume,
            MinValidSellVolume: dto.MinValidSellVolume,
            MaxValidSellVolume: dto.MaxValidSellVolume,

            TotalTradeValue: dto.TotalTradeValue,
            TotalNumberOfTrades: dto.TotalNumberOfTrades,
            TotalNumberOfSharesTraded: dto.TotalNumberOfSharesTraded,

            CapturedAt: capturedAt,
            Source: source);
    }
}