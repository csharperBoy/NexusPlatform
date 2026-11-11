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
        var instrumentSpec = new InstrumentByIsinSpec(isin);
        var instrument = await _uow.InstrumentSpecs.GetBySpecAsync(instrumentSpec);
        if (instrument is null) return null;

        var presetSpec = new InstrumentOrderPresetByInstrumentSpec(instrument.Id);
        var preset = await _uow.InstrumentOrderPresetSpecs.GetBySpecAsync(presetSpec);

        return preset is null ? null : MapToView(preset, instrument);
    }

    public async Task<Guid> CreateAsync(CreateOrderPresetModel model)
    {
        var instrument = await _uow.Instruments.GetByIdAsync(model.InstrumentId)
            ?? throw new InvalidOperationException(
                $"Instrument {model.InstrumentId} not found.");

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
            "Created OrderPreset {Id} for instrument {InstrumentId} ({Symbol})",
            preset.Id, model.InstrumentId, instrument.SymbolName);

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

    private static OrderPresetView MapToView(
        InstrumentOrderPreset p, Instrument? instrument) => new(
        Id: p.Id,
        InstrumentId: p.InstrumentId,
        InstrumentIsin: instrument?.Isin ?? "",
        InstrumentSymbolName: instrument?.SymbolName ?? "",
        Name: p.Name,
        DefaultPrice: p.DefaultPrice.Value,
        DefaultQuantity: p.DefaultQuantity.Value,
        DefaultSide: (int)p.DefaultSide,
        ValidityType: p.ValidityType,
        Commission: p.Commission,
        DefaultOrderMode: (int)p.DefaultOrderMode,
        CreatedAt: p.CreatedAt);
}