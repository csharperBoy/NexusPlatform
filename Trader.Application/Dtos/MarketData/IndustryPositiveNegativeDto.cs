namespace Trader.Application.Dtos.MarketData;

public class IndustryPositiveNegativeDto
{
    public string IndustryCode { get; set; } = "";
    public string IndustryName { get; set; } = "";
    public long NegativeCount { get; set; }
    public long ZeroCount { get; set; }
    public long PositiveCount { get; set; }
}