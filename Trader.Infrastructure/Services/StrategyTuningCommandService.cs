// Trader.Infrastructure/Services/StrategyTuningCommandService.cs
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos.Tuning;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Services;

public sealed class StrategyTuningCommandService : IStrategyTuningCommandService
{
    private readonly ITraderUnitOfWork<TraderDbContext> _uow;
    private readonly ILogger<StrategyTuningCommandService> _logger;

    public StrategyTuningCommandService(
        ITraderUnitOfWork<TraderDbContext> uow,
        ILogger<StrategyTuningCommandService> logger)
    {
        _uow = uow; _logger = logger;
    }

    public async Task<Guid> CreateAsync(CreateStrategyTuningModel m, CancellationToken ct)
    {
        var run = StrategyTuningRun.Create(
            m.Name, m.StrategyKey, m.Isin, m.TargetCurrency,
            (TimeFrame)m.PrimaryTimeFrame,
            m.SubCandleTimeFrame.HasValue ? (TimeFrame)m.SubCandleTimeFrame.Value : null,
            m.DateFrom, m.DateTo, m.InitialCapital, m.SlippagePercent,
            (TuningScale)m.SelectedScales, (TuningScale)m.PrimaryScale,
            m.MinTradesPerWindow, SearchAlgorithm.Genetic,
            m.PopulationSize, m.Generations, m.MutationRate, m.CrossoverRate);
        await _uow.StrategyTuningRuns.AddAsync(run);
        _logger.LogInformation("TuningRun {Id} queued: {Strategy} {Isin}",
            run.Id, m.StrategyKey, m.Isin);
        return run.Id;
    }

    public async Task<bool> CancelAsync(Guid id, CancellationToken ct)
    {
        var run = await _uow.StrategyTuningRuns.GetByIdAsync(id);
        if (run is null) return false;
        if (run.Status != TuningStatus.Pending && run.Status != TuningStatus.Running)
            return false;
        run.MarkCancelled();
        await _uow.StrategyTuningRuns.UpdateAsync(run);
        return true;
    }

    public Task SaveAsync(CancellationToken ct) => _uow.SaveChangesAsync(ct);
}