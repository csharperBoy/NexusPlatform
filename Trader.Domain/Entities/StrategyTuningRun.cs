// Trader.Domain/Entities/StrategyTuningRun.cs
using Core.Domain.Common.EntityProperties;
using Trader.Domain.Enums;

namespace Trader.Domain.Entities;

public class StrategyTuningRun : BaseEntity, IAuditableEntity
{
    public string Name { get; private set; } = default!;
    public string StrategyKey { get; private set; } = default!;
    public string Isin { get; private set; } = default!;
    public string? TargetCurrency { get; private set; }
    public TimeFrame PrimaryTimeFrame { get; private set; }
    public TimeFrame? SubCandleTimeFrame { get; private set; }
    public DateTime DateFrom { get; private set; }
    public DateTime DateTo { get; private set; }
    public decimal InitialCapital { get; private set; }
    public decimal SlippagePercent { get; private set; }
    public TuningScale SelectedScales { get; private set; }
    public TuningScale PrimaryScale { get; private set; }
    public int MinTradesPerWindow { get; private set; }
    public SearchAlgorithm Algorithm { get; private set; }
    public int PopulationSize { get; private set; }
    public int Generations { get; private set; }
    public decimal MutationRate { get; private set; }
    public decimal CrossoverRate { get; private set; }
    public TuningStatus Status { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public int TotalEvaluations { get; private set; }
    public int CompletedEvaluations { get; private set; }
    public Guid? BestResultIdYearly { get; private set; }
    public Guid? BestResultIdSemi { get; private set; }
    public Guid? BestResultIdQuarterly { get; private set; }
    public Guid? BestResultIdMonthly { get; private set; }
    public string? ErrorMessage { get; private set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public void Touch() => ModifiedAt = DateTime.UtcNow;

    public string SelectedParametersJson { get; private set; } = "[]";

    public IReadOnlyList<string> GetSelectedParameters()
    {
        if (string.IsNullOrWhiteSpace(SelectedParametersJson) || SelectedParametersJson == "[]")
            return Array.Empty<string>();
        try
        {
            return System.Text.Json.JsonSerializer
                .Deserialize<List<string>>(SelectedParametersJson) ?? new();
        }
        catch { return Array.Empty<string>(); }
    }

    private StrategyTuningRun() { }

    public static StrategyTuningRun Create(
        string name, string strategyKey, string isin, string? targetCurrency,
        TimeFrame primaryTf, TimeFrame? subCandleTf,
        DateTime dateFrom, DateTime dateTo, decimal initialCapital, decimal slippagePercent,
        TuningScale selectedScales, TuningScale primaryScale, int minTradesPerWindow,
        SearchAlgorithm algorithm, int populationSize, int generations,

        decimal mutationRate, decimal crossoverRate,
    IReadOnlyList<string>? selectedParameters = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name required.", nameof(name));
        if (selectedScales == TuningScale.None)
            throw new ArgumentException("At least one scale required.", nameof(selectedScales));
        if ((selectedScales & primaryScale) == 0)
            throw new ArgumentException(
                "PrimaryScale must be included in SelectedScales.", nameof(primaryScale));
        if (dateFrom >= dateTo)
            throw new ArgumentException("DateFrom must be before DateTo.");
        if (initialCapital <= 0m)
            throw new ArgumentException("InitialCapital must be > 0.");
        if (populationSize < 10 || populationSize > 200)
            throw new ArgumentException("PopulationSize must be in [10,200].");
        if (generations < 10 || generations > 500)
            throw new ArgumentException("Generations must be in [10,500].");

        return new StrategyTuningRun
        {
            Name = name.Trim(),
            StrategyKey = strategyKey,
            Isin = isin,
            TargetCurrency = string.IsNullOrWhiteSpace(targetCurrency) ? null : targetCurrency.Trim().ToUpperInvariant(),
            PrimaryTimeFrame = primaryTf,
            SubCandleTimeFrame = subCandleTf,
            DateFrom = dateFrom,
            DateTo = dateTo,
            InitialCapital = initialCapital,
            SlippagePercent = slippagePercent,
            SelectedScales = selectedScales,
            PrimaryScale = primaryScale,
            MinTradesPerWindow = Math.Max(0, minTradesPerWindow),
            Algorithm = algorithm,
            PopulationSize = populationSize,
            Generations = generations,
            MutationRate = mutationRate,
            CrossoverRate = crossoverRate,
            Status = TuningStatus.Pending,
            TotalEvaluations = populationSize * generations,
            CompletedEvaluations = 0,

            SelectedParametersJson = System.Text.Json.JsonSerializer
            .Serialize(selectedParameters?.ToList() ?? new List<string>()),

            CreatedAt = DateTime.UtcNow,
        };
    }

    public void MarkRunning()
    {
        if (Status != TuningStatus.Pending)
            throw new InvalidOperationException($"Cannot MarkRunning from {Status}.");
        Status = TuningStatus.Running;
        StartedAt = DateTime.UtcNow;
        Touch();
    }

    public void ReportProgress(int completed) { CompletedEvaluations = completed; Touch(); }

    public void MarkCompleted(
        Guid? bestYearly, Guid? bestSemi, Guid? bestQuarterly, Guid? bestMonthly)
    {
        Status = TuningStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        BestResultIdYearly = bestYearly;
        BestResultIdSemi = bestSemi;
        BestResultIdQuarterly = bestQuarterly;
        BestResultIdMonthly = bestMonthly;
        CompletedEvaluations = TotalEvaluations;
        Touch();
    }

    public void MarkFailed(string reason)
    {
        Status = TuningStatus.Failed;
        CompletedAt = DateTime.UtcNow;
        ErrorMessage = reason;
        Touch();
    }

    public void MarkCancelled()
    {
        Status = TuningStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
        Touch();
    }
}