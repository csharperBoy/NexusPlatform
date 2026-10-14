using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
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
    private readonly ILogger<InstrumentService> _logger;

    public InstrumentService(
        ITraderUnitOfWork<TraderDbContext> uow,
        ILogger<InstrumentService> logger)
    {
        _uow = uow;
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
            optionContract);

        await _uow.Instruments.AddAsync(instrument);
        await _uow.SaveChangesAsync();

        _logger.LogInformation(
            "Created Instrument {Id} isin={Isin} name={Symbol}",
            instrument.Id, instrument.Isin, instrument.SymbolName);

        return instrument.Id;
    }

    public async Task UpdateAsync(Guid id, UpdateInstrumentModel model)
    {
        var instrument = await _uow.Instruments.GetByIdAsync(id)
            ?? throw new InvalidOperationException($"Instrument {id} not found.");

        instrument.Update(model.SymbolName, model.DisplayName, model.BoardCode);

        await _uow.Instruments.UpdateAsync(instrument);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Updated Instrument {Id}", id);
    }

    public async Task DeactivateAsync(Guid id)
    {
        var instrument = await _uow.Instruments.GetByIdAsync(id)
            ?? throw new InvalidOperationException($"Instrument {id} not found.");

        instrument.Deactivate();

        await _uow.Instruments.UpdateAsync(instrument);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Deactivated Instrument {Id}", id);
    }

    /* ═══════════════════ Queries ═══════════════════ */

    public async Task<IReadOnlyList<InstrumentInfoView>> GetListAsync()
    {
        var instruments = await _uow.Instruments.GetAllAsync();
        return instruments
            .OrderBy(i => i.SymbolName)
            .Select(MapToInfoView)
            .ToList();
    }

    public async Task<InstrumentInfoView?> GetByIsinAsync(string isin)
    {
        var spec = new InstrumentByIsinSpec(isin);
        var instrument = await _uow.InstrumentSpecs.GetBySpecAsync(spec);
        return instrument is null ? null : MapToInfoView(instrument);
    }

    public async Task<IReadOnlyList<InstrumentSelectionView>> GetSelectionListAsync()
    {
        var spec = new ActiveInstrumentsSpec();
        var instruments = await _uow.InstrumentSpecs.ListBySpecAsync(spec);

        return instruments
            .Select(i => new InstrumentSelectionView(
                i.Id, i.Isin, i.SymbolName, i.DisplayName))
            .ToList();
    }

    private static InstrumentInfoView MapToInfoView(Instrument i) => new(
        i.Id,
        i.Isin,
        i.SymbolName,
        i.DisplayName,
        (int)i.Type,
        i.Type.ToString(),
        (int)i.Market,
        i.Market.ToString(),
        i.BoardCode,
        i.IsActive,
        i.CreatedAt);
}