namespace Trader.Application.Dtos.MarketData;

public class TseIndexDto
{
    public string SymbolIsin { get; set; } = "";
    public string SymbolTitle { get; set; } = "";
    public DateTimeOffset DayOfEvent { get; set; }
    public long IndexChanges { get; set; }
    public long LastIndexValue { get; set; }
    public double PercentVariation { get; set; }
}