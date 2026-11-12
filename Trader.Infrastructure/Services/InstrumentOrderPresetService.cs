using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Brokers;
using Trader.Application.Abstractions.Brokers.Queries;
using Trader.Application.Dtos;
using Trader.Application.Dtos.Instruments;
using Trader.Application.Specifications.Instruments;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Services;

public class InstrumentOrderPresetService : IInstrumentOrderPresetService
{
    private readonly ITraderUnitOfWork<TraderDbContext> _uow;
    private readonly IBrokerClientFactory _brokerFactory;
    private readonly IBrokerSessionProvider _sessionProvider;
    private readonly ILogger<InstrumentOrderPresetService> _logger;

    public InstrumentOrderPresetService(
        ITraderUnitOfWork<TraderDbContext> uow,
        IBrokerClientFactory brokerFactory,
        IBrokerSessionProvider sessionProvider,
        ILogger<InstrumentOrderPresetService> logger)
    {
        _uow = uow;
        _brokerFactory = brokerFactory;
        _sessionProvider = sessionProvider;
        _logger = logger;
    }

    /* ═══════════ Queries ═══════════ */

    public async Task<IReadOnlyList<OrderPresetView>> GetListAsync()
    {
        var presets = await _uow.InstrumentOrderPresets.GetAllAsync(
            q => q.OrderBy(p => p.Name));
        var instruments = (await _uow.Instruments.GetAllAsync(
                q => q.OrderBy(i => i.SymbolName)))
            .ToDictionary(i => i.Id);

        return presets.Select(p => MapToView(p, instruments.GetValueOrDefault(p.InstrumentId)))
            .ToList();
    }

    public async Task<OrderPresetView?> GetByIdAsync(Guid id)
    {
        var preset = await _uow.InstrumentOrderPresets.GetByIdAsync(id);
        if (preset is null) return null;
        var instrument = await _uow.Instruments.GetByIdAsync(preset.InstrumentId);
        return MapToView(preset, instrument);
    }

    public async Task<OrderPresetView?> GetByInstrumentAsync(string isin)
    {
        var instrument = await _uow.InstrumentSpecs.GetBySpecAsync(
            new InstrumentByIsinSpec(isin));
        if (instrument is null) return null;

        var preset = await _uow.InstrumentOrderPresetSpecs.GetBySpecAsync(
            new InstrumentOrderPresetByInstrumentSpec(instrument.Id));
        return preset is null ? null : MapToView(preset, instrument);
    }

    /* ═══════════ Commands ═══════════ */

    public async Task<Guid> CreateAsync(CreateOrderPresetModel model)
    {
        var instrument = await _uow.Instruments.GetByIdAsync(model.InstrumentId)
            ?? throw new InvalidOperationException(
                $"Instrument {model.InstrumentId} not found.");

        var preset = InstrumentOrderPreset.Create(
            instrumentId: model.InstrumentId,
            name: model.Name,
            priceStrategy: (OrderPriceStrategy)model.PriceStrategy,
            fixedPrice: model.FixedPrice.HasValue ? new Price(model.FixedPrice.Value) : null,
            quantityStrategy: (OrderQuantityStrategy)model.QuantityStrategy,
            fixedQuantity: model.FixedQuantity.HasValue ? new Quantity(model.FixedQuantity.Value) : null,
            defaultSide: (OrderSide)model.DefaultSide,
            validityType: model.ValidityType,
            commission: model.Commission,
            defaultOrderMode: (OrderMode)model.DefaultOrderMode);

        await _uow.InstrumentOrderPresets.AddAsync(preset);
        await _uow.SaveChangesAsync();

        _logger.LogInformation(
            "Created OrderPreset {Id} for {Symbol} (priceStrategy={PS}, qtyStrategy={QS})",
            preset.Id, instrument.SymbolName, preset.PriceStrategy, preset.QuantityStrategy);

        return preset.Id;
    }

    public async Task UpdateAsync(Guid id, UpdateOrderPresetModel model)
    {
        var preset = await _uow.InstrumentOrderPresets.GetByIdAsync(id)
            ?? throw new InvalidOperationException($"OrderPreset {id} not found.");

        preset.Update(
            name: model.Name,
            priceStrategy: (OrderPriceStrategy)model.PriceStrategy,
            fixedPrice: model.FixedPrice.HasValue ? new Price(model.FixedPrice.Value) : null,
            quantityStrategy: (OrderQuantityStrategy)model.QuantityStrategy,
            fixedQuantity: model.FixedQuantity.HasValue ? new Quantity(model.FixedQuantity.Value) : null,
            defaultSide: (OrderSide)model.DefaultSide,
            validityType: model.ValidityType,
            commission: model.Commission,
            defaultOrderMode: (OrderMode)model.DefaultOrderMode);

        await _uow.InstrumentOrderPresets.UpdateAsync(preset);
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var preset = await _uow.InstrumentOrderPresets.GetByIdAsync(id)
            ?? throw new InvalidOperationException($"OrderPreset {id} not found.");

        await _uow.InstrumentOrderPresets.DeleteAsync(preset);
        await _uow.SaveChangesAsync();
    }

    /* ═══════════ Resolve ═══════════ */

    public async Task<ResolvedOrderPresetView> ResolveAsync(
        Guid id, CancellationToken ct = default)
    {
        var preset = await _uow.InstrumentOrderPresets.GetByIdAsync(id)
            ?? throw new InvalidOperationException($"OrderPreset {id} not found.");
        var instrument = await _uow.Instruments.GetByIdAsync(preset.InstrumentId)
            ?? throw new InvalidOperationException(
                $"Instrument {preset.InstrumentId} not found.");

        /* ─── Price ─── */
        decimal price;
        string priceSource;
        string? warning = null;

        long? liveHigh = null, liveLow = null, liveLast = null, liveClose = null, liveFirst = null;
        Guid? usedAccountId = null;
        string? usedAccountName = null;

        if (preset.PriceStrategy == OrderPriceStrategy.Fixed)
        {
            price = preset.FixedPrice!.Value.Value;
            priceSource = "Fixed";
        }
        else
        {
            var live = await TryFetchLiveAsync(instrument.Isin, ct);
            liveHigh = live.Info.HighAllowedPrice;
            liveLow = live.Info.LowAllowedPrice;
            liveLast = live.Info.LastTradedPrice;
            liveClose = live.Info.ClosingPrice;
            liveFirst = live.Info.FirstTradedPrice;
            usedAccountId = live.AccountId;
            usedAccountName = live.AccountName;

            switch (preset.PriceStrategy)
            {
                case OrderPriceStrategy.HighAllowed:
                    if (liveHigh is > 0)
                    {
                        price = liveHigh.Value;
                        priceSource = "Live:HighAllowed";
                    }
                    else if (liveClose is > 0 && instrument.DailyPriceRangeUpPercent.HasValue)
                    {
                        price = RoundPrice(liveClose.Value * (1m + instrument.DailyPriceRangeUpPercent.Value / 100m));
                        priceSource = $"Fallback:Closing×(1+{instrument.DailyPriceRangeUpPercent}%)";
                        warning = "قیمت سقف مجاز از API در دسترس نبود — از قیمت پایانی × بازه بالا تخمین زده شد.";
                    }
                    else if (liveClose is > 0)
                    {
                        throw new InvalidOperationException(
                            "قیمت سقف مجاز از API در دسترس نیست و بازه بالا برای این نماد تعریف نشده (unlimited). " +
                            "برای پیش‌بینی از RangeUp، آن را در تنظیمات Instrument مقداردهی کنید.");
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            "قیمت سقف مجاز و قیمت پایانی در دسترس نیست (بازار بسته و API داده نمی‌دهد).");
                    }
                    break;

                case OrderPriceStrategy.LowAllowed:
                    if (liveLow is > 0)
                    {
                        price = liveLow.Value;
                        priceSource = "Live:LowAllowed";
                    }
                    else if (liveClose is > 0 && instrument.DailyPriceRangeDownPercent.HasValue)
                    {
                        price = RoundPrice(liveClose.Value * (1m - instrument.DailyPriceRangeDownPercent.Value / 100m));
                        priceSource = $"Fallback:Closing×(1-{instrument.DailyPriceRangeDownPercent}%)";
                        warning = "قیمت کف مجاز از API در دسترس نبود — از قیمت پایانی × بازه پایین تخمین زده شد.";
                    }
                    else if (liveClose is > 0)
                    {
                        throw new InvalidOperationException(
                            "قیمت کف مجاز از API در دسترس نیست و بازه پایین برای این نماد تعریف نشده (unlimited).");
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            "قیمت کف مجاز و قیمت پایانی در دسترس نیست.");
                    }
                    break;

                case OrderPriceStrategy.LastTraded:
                    if (liveLast is > 0) { price = liveLast.Value; priceSource = "Live:LastTraded"; }
                    else throw new InvalidOperationException(
                        "آخرین قیمت معامله در دسترس نیست.");
                    break;

                case OrderPriceStrategy.Closing:
                    if (liveClose is > 0) { price = liveClose.Value; priceSource = "Live:Closing"; }
                    else throw new InvalidOperationException("قیمت پایانی در دسترس نیست.");
                    break;

                case OrderPriceStrategy.FirstTraded:
                    if (liveFirst is > 0) { price = liveFirst.Value; priceSource = "Live:FirstTraded"; }
                    else throw new InvalidOperationException("اولین قیمت معامله در دسترس نیست.");
                    break;

                default:
                    throw new InvalidOperationException($"Unknown PriceStrategy {preset.PriceStrategy}.");
            }

            // Sanity: price must be within daily range if bounds are defined
            if (liveClose is > 0)
            {
                decimal? maxAllowed = instrument.DailyPriceRangeUpPercent.HasValue
                    ? liveClose.Value * (1m + instrument.DailyPriceRangeUpPercent.Value / 100m)
                    : (decimal?)null;

                decimal? minAllowed = instrument.DailyPriceRangeDownPercent.HasValue
                    ? liveClose.Value * (1m - instrument.DailyPriceRangeDownPercent.Value / 100m)
                    : (decimal?)null;

                var outsideBounds =
                    (maxAllowed.HasValue && price > maxAllowed.Value) ||
                    (minAllowed.HasValue && price < minAllowed.Value);

                if (outsideBounds)
                {
                    var minStr = minAllowed.HasValue ? minAllowed.Value.ToString("N0") : "∞";
                    var maxStr = maxAllowed.HasValue ? maxAllowed.Value.ToString("N0") : "∞";
                    warning = (warning is null ? "" : warning + " ") +
                        $"قیمت resolve شده ({price:N0}) خارج از بازه‌ی روزانه ({minStr}..{maxStr}) است — ممکن است Broker رد کند.";
                }
            }
        }

        /* ─── Quantity ─── */
        long quantity;
        string quantitySource;

        if (preset.QuantityStrategy == OrderQuantityStrategy.Fixed)
        {
            quantity = preset.FixedQuantity!.Value.Value;
            quantitySource = "Fixed";
        }
        else if (preset.QuantityStrategy == OrderQuantityStrategy.MaxOrderQuantity)
        {
            quantity = instrument.MaxOrderQuantity;
            quantitySource = "Instrument:MaxOrderQuantity";
        }
        else if (preset.QuantityStrategy == OrderQuantityStrategy.MinOrderQuantity)
        {
            quantity = instrument.MinOrderQuantity;
            quantitySource = "Instrument:MinOrderQuantity";
        }
        else
        {
            throw new InvalidOperationException(
                $"Unknown QuantityStrategy {preset.QuantityStrategy}.");
        }

        return new ResolvedOrderPresetView(
    PresetId: preset.Id,
    InstrumentId: preset.InstrumentId,
    InstrumentIsin: instrument.Isin,
    InstrumentSymbolName: instrument.SymbolName,
    Price: price,
    PriceSource: priceSource,
    Quantity: quantity,
    QuantitySource: quantitySource,
    Side: (int)preset.DefaultSide,
    SideName: preset.DefaultSide == OrderSide.Buy ? "خرید" : "فروش",
    DefaultOrderMode: (int)preset.DefaultOrderMode,
    Commission: preset.Commission,
    LiveHighAllowed: liveHigh,
    LiveLowAllowed: liveLow,
    LiveLastTraded: liveLast,
    LiveClosing: liveClose,
    LiveFirstTraded: liveFirst,
    InstrumentRangeUpPercent: instrument.DailyPriceRangeUpPercent,      // ← جدید
    InstrumentRangeDownPercent: instrument.DailyPriceRangeDownPercent,  // ← جدید
    UsedAccountId: usedAccountId,
    UsedAccountName: usedAccountName,
    Warning: warning);
    }

    /* ═══════════ Helpers ═══════════ */

    private async Task<(Guid AccountId, string AccountName, SymbolMarketDataDto Info)>
    TryFetchLiveAsync(string isin, CancellationToken ct)
    {
        var accounts = await _uow.Accounts.GetAllAsync();
        var account = accounts.FirstOrDefault(a => a.Broker == BrokerType.EasyTrader)
            ?? throw new InvalidOperationException(
                "هیچ حساب EasyTrader فعالی برای دریافت قیمت لحظه‌ای وجود ندارد.");

        var session = await _sessionProvider.GetSessionAsync(account.Id, ct);
        var reader = _brokerFactory.Get<IBrokerMarketDataReader>(account.Broker);
        var info = await reader.GetSymbolInfoAsync(session, isin, ct);

        return (account.Id, account.Name, info);
    }

    private static decimal RoundPrice(decimal value) =>
        Math.Round(value, 0, MidpointRounding.AwayFromZero);

    private static OrderPresetView MapToView(
        InstrumentOrderPreset p, Instrument? instrument) => new(
        Id: p.Id,
        InstrumentId: p.InstrumentId,
        InstrumentIsin: instrument?.Isin ?? "",
        InstrumentSymbolName: instrument?.SymbolName ?? "",
        Name: p.Name,
        PriceStrategy: (int)p.PriceStrategy,
        PriceStrategyName: OrderPresetStrategyNames.PriceStrategy(p.PriceStrategy),
        FixedPrice: p.FixedPrice?.Value,
        QuantityStrategy: (int)p.QuantityStrategy,
        QuantityStrategyName: OrderPresetStrategyNames.QuantityStrategy(p.QuantityStrategy),
        FixedQuantity: p.FixedQuantity?.Value,
        DefaultSide: (int)p.DefaultSide,
        DefaultSideName: p.DefaultSide == OrderSide.Buy ? "خرید" : "فروش",
        ValidityType: p.ValidityType,
        Commission: p.Commission,
        DefaultOrderMode: (int)p.DefaultOrderMode,
        DefaultOrderModeName: p.DefaultOrderMode == OrderMode.Quantity ? "تعداد" : "ارزش کل",
        CreatedAt: p.CreatedAt);
}