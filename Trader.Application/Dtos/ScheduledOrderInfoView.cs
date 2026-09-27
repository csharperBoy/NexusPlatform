namespace Trader.Application.Dtos
{
    public class ScheduledOrderInfoView
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public string? AccountName { get; set; }
        public string SymbolIsin { get; set; } = default!;
        public string? SymbolName { get; set; }
        public int Side { get; set; }
        public string Mode { get; set; } = "quantity";   // "quantity" | "totalValue"
        public string Quantity { get; set; } = "0";
        public string TotalValue { get; set; } = "0";
        public string Time { get; set; } = "00:00:00.000";
        public bool Fired { get; set; }
    }
}