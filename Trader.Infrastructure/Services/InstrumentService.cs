using Core.Domain.Common;
using Core.Shared.DTOs;
using Microsoft.Extensions.Logging;
using System.Reflection.Emit;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.MarketData;
using Trader.Application.Dtos.Instruments;
using Trader.Application.Specifications.Instruments;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Services;

public class InstrumentService : IInstrumentCommandService, IInstrumentQueryService
{
    private readonly ITraderUnitOfWork<TraderDbContext> _uow;
    private readonly ITradingScheduleCache _scheduleCache;
    private readonly ILogger<InstrumentService> _logger;

    public InstrumentService(
        ITraderUnitOfWork<TraderDbContext> uow,
        ITradingScheduleCache scheduleCache,
        ILogger<InstrumentService> logger)
    {
        _uow = uow;
        _scheduleCache = scheduleCache;
        _logger = logger;
    }

    /* ═══════════════════ Commands ═══════════════════ */

    public async Task<Guid> CreateAsync(CreateInstrumentModel model)
    {
        var type = (InstrumentType)model.Type;
        var market = (MarketType)model.Market;

        OptionContract? optionContract = null;
        if (model.OptionContract is not null)
        {
            optionContract = OptionContract.Create(
                (OptionType)model.OptionContract.OptionType,
                new Price(model.OptionContract.StrikePrice),
                model.OptionContract.ExpirationDate,
                model.OptionContract.UnderlyingIsin);
        }

        var instrument = Instrument.Create(
            model.Isin,
            model.SymbolName,
            model.DisplayName,
            type,
            market,
            model.BoardCode,
            optionContract,
            model.TradingScheduleId,
            model.BuyCommissionRate,
            model.SellCommissionRate,
            model.MinOrderQuantity,
            model.MaxOrderQuantity,
            model.MinOrderValueRial);

        await _uow.Instruments.AddAsync(instrument);
        await _uow.SaveChangesAsync();

        _scheduleCache.Invalidate(instrument.Isin);

        _logger.LogInformation(
            "Created Instrument {Id} isin={Isin} buyComm={BuyRate} sellComm={SellRate}",
            instrument.Id, instrument.Isin,
            instrument.BuyCommissionRate, instrument.SellCommissionRate);

        return instrument.Id;
    }


    public async Task<bool> UpdateAsync(
    Guid id,
    Optional<string?> symbolName,
    Optional<string?> displayName,
    Optional<string?> boardCode,
    Optional<Guid?> tradingScheduleId,
    Optional<decimal> buyCommissionRate,
    Optional<decimal> sellCommissionRate,
    Optional<long> minOrderQuantity,
    Optional<long> maxOrderQuantity,
    Optional<long?> minOrderValueRial)
    {
        var instrument = await _uow.Instruments.GetByIdAsync(id)
            ?? throw new InvalidOperationException($"Instrument {id} not found.");

        var hasChange = instrument.ApplyChange(
            symbolName,
            displayName,
            boardCode,
            tradingScheduleId,
            buyCommissionRate,
            sellCommissionRate,
            minOrderQuantity,
            maxOrderQuantity,
            minOrderValueRial);

        if (!hasChange)
        {
            _logger.LogInformation("No changes for Instrument {Id}", id);
            return false;
        }

        await _uow.Instruments.UpdateAsync(instrument);
        await _uow.SaveChangesAsync();

        _scheduleCache.Invalidate(instrument.Isin);

        _logger.LogInformation("Updated Instrument {Id}", id);
        return true;
    }

    public async Task DeactivateAsync(Guid id)
    {
        var instrument = await _uow.Instruments.GetByIdAsync(id)
            ?? throw new InvalidOperationException($"Instrument {id} not found.");

        instrument.Deactivate();
        await _uow.Instruments.UpdateAsync(instrument);
        await _uow.SaveChangesAsync();

        _scheduleCache.Invalidate(instrument.Isin);

        _logger.LogInformation("Deactivated Instrument {Id}", id);
    }

    /* ═══════════════════ Queries ═══════════════════ */

    public async Task<IReadOnlyList<InstrumentInfoView>> GetListAsync()
    {
        var instruments = await _uow.Instruments.GetAllAsync();
        var schedules = await _uow.TradingSchedules.GetAllAsync();
        var scheduleById = schedules.ToDictionary(s => s.Id);

        return instruments
            .OrderBy(i => i.SymbolName)
            .Select(i => MapToInfoView(i, scheduleById))
            .ToList();
    }

    public async Task<InstrumentInfoView?> GetByIsinAsync(string isin)
    {
        var spec = new InstrumentByIsinSpec(isin);
        var instrument = await _uow.InstrumentSpecs.GetBySpecAsync(spec);
        if (instrument is null) return null;

        TradingSchedule? schedule = null;
        if (instrument.TradingScheduleId is Guid sid)
            schedule = await _uow.TradingSchedules.GetByIdAsync(sid);

        var dict = schedule is null
            ? new Dictionary<Guid, TradingSchedule>()
            : new Dictionary<Guid, TradingSchedule> { [schedule.Id] = schedule };

        return MapToInfoView(instrument, dict);
    }

    public async Task<IReadOnlyList<SelectionListDto>> GetSelectionListAsync()
    {
        var spec = new ActiveInstrumentsSpec();
        var instruments = await _uow.InstrumentSpecs.ListBySpecAsync(spec);

        return instruments
            .Select(i => new SelectionListDto(
                i.Isin,
                 i.SymbolName,
                i.DisplayName))
            .ToList();
    }

    /* ═══════════════════ Helpers ═══════════════════ */

    private static InstrumentInfoView MapToInfoView(
    Instrument i,
    IReadOnlyDictionary<Guid, TradingSchedule> schedules)
    {
        TradingSchedule? schedule = null;
        if (i.TradingScheduleId is Guid sid)
            schedules.TryGetValue(sid, out schedule);

        return new InstrumentInfoView(
            i.Id,
            i.Isin,
            i.SymbolName,
            i.DisplayName,
            (int)i.Type,
            i.Type.ToString(),
            (int)i.Market,
            i.Market.ToString(),
            i.BoardCode,
            i.TradingScheduleId,
            schedule?.Code,
            schedule?.Name,
            i.BuyCommissionRate,
            i.SellCommissionRate,
            i.MinOrderQuantity,
            i.MaxOrderQuantity,
            i.MinOrderValueRial,
            i.IsActive,
            i.CreatedAt);
    }
}