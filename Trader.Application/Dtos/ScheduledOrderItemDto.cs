namespace Trader.Application.Dtos
{
    /// <summary>
    /// آیتم سفارش در Command پلن (فرمت مستقیم از فرانت).
    /// </summary>
    public record ScheduledOrderItemDto(
        Guid AccountId,
        string SymbolIsin,
        int Side,          // 0 = Buy, 1 = Sell
        string Mode,       // "quantity" | "totalValue"
        string Quantity,
        string TotalValue,
        string Time        // "HH:MM:SS.mmm"
    );
}