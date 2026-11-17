// Trader.Application/Dtos/Tuning/CreateStrategyTuningModel.cs
namespace Trader.Application.Dtos.Tuning;

public sealed class CreateStrategyTuningModel
{
    public string Name { get; set; } = default!;
    public string StrategyKey { get; set; } = default!;
    public string Isin { get; set; } = default!;
    public string? TargetCurrency { get; set; }
    public int PrimaryTimeFrame { get; set; }
    public int? SubCandleTimeFrame { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public decimal InitialCapital { get; set; }
    public decimal SlippagePercent { get; set; }
    public int SelectedScales { get; set; }       // flags
    public int PrimaryScale { get; set; }
    public int MinTradesPerWindow { get; set; }
    public int PopulationSize { get; set; } = 40;
    public int Generations { get; set; } = 50;
    public decimal MutationRate { get; set; } = 0.1m;
    public decimal CrossoverRate { get; set; } = 0.8m;
}