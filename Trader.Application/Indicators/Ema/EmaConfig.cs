namespace Trader.Application.Indicators.Ema;

public sealed class EmaConfig
{
    /// <summary>دوره‌ی EMA. حتماً > 0.</summary>
    public int Period { get; set; } = 20;
}