using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
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
    private readonly ILogger<InstrumentOrderPresetService> _logger;

    public InstrumentOrderPresetService(
        ITraderUnitOfWork<TraderDbContext> uow,
        ILogger<InstrumentOrderPresetService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<Guid> CreateAsync(CreateOrderPresetModel model)
    {
        var preset = InstrumentOrderPreset.Create(
            model.InstrumentId,
            model.Name,
            new Price(model.DefaultPrice),
            new Quantity(model.DefaultQuantity),
            (OrderSide)model.DefaultSide,
            model.ValidityType,
            model.Commission,
            (OrderMode)model.DefaultOrderMode);

        await _uow.InstrumentOrderPresets.AddAsync(preset);
        await _uow.SaveChangesAsync();

        _logger.LogInformation(
            "Created InstrumentOrderPreset {Id} for instrument {InstrumentId}",
            preset.Id, model.InstrumentId);

        return preset.Id;
    }

    public async Task UpdateAsync(Guid id, UpdateOrderPresetModel model)
    {
        var preset = await _uow.InstrumentOrderPresets.GetByIdAsync(id)
            ?? throw new InvalidOperationException($"OrderPreset {id} not found.");

        preset.Update(
            model.Name,
            new Price(model.DefaultPrice),
            new Quantity(model.DefaultQuantity),
            (OrderSide)model.DefaultSide,
            model.ValidityType,
            model.Commission,
            (OrderMode)model.DefaultOrderMode);

        await _uow.InstrumentOrderPresets.UpdateAsync(preset);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Updated InstrumentOrderPreset {Id}", id);
    }

    public async Task DeleteAsync(Guid id)
    {
        var preset = await _uow.InstrumentOrderPresets.GetByIdAsync(id)
            ?? throw new InvalidOperationException($"OrderPreset {id} not found.");

        await _uow.InstrumentOrderPresets.DeleteAsync(preset);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Deleted InstrumentOrderPreset {Id}", id);
    }

    public async Task<OrderPresetView?> GetByInstrumentAsync(string isin)
    {
        var instrumentSpec = new InstrumentByIsinSpec(isin);
        var instrument = await _uow.InstrumentSpecs.GetBySpecAsync(instrumentSpec);
        if (instrument is null) return null;

        var presetSpec = new InstrumentOrderPresetByInstrumentSpec(instrument.Id);
        var preset = await _uow.InstrumentOrderPresetSpecs.GetBySpecAsync(presetSpec);

        return preset is null ? null : MapToView(preset);
    }

    private static OrderPresetView MapToView(InstrumentOrderPreset p) => new(
        p.Id,
        p.InstrumentId,
        p.Name,
        p.DefaultPrice.Value,
        p.DefaultQuantity.Value,
        (int)p.DefaultSide,
        p.ValidityType,
        p.Commission,
        (int)p.DefaultOrderMode);
}