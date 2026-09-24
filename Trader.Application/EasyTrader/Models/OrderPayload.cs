namespace Trader.Application.EasyTrader.Models
{
    public record OrderPayload
    {
        public Order Order { get; init; } = new();
    }

    public record Order
    {
        public long Price { get; init; }
        public long Quantity { get; init; }
        public int Side { get; init; }
        public int ValidityType { get; init; }
        public string CreateDateTime { get; init; } = default!;
        public decimal Commission { get; init; }
        public string SymbolIsin { get; init; } = default!;
        public string SymbolName { get; init; } = default!;
        public int OrderModelType { get; init; }
        public long TotalValue { get; init; }
        public int OrderFrom { get; init; }
    }
}