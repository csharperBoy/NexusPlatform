namespace Trader.Application.Dtos.MarketData;

public class MarketWatchCategoryDto
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int Order { get; set; }
    public bool IsDefault { get; set; }
    public string? CustomerIsin { get; set; }
    public DateTimeOffset? CreateDateTime { get; set; }
    public List<string> SymbolIsins { get; set; } = new();
}