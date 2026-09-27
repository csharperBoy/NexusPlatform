namespace Trader.Application.Dtos
{
    public class SymbolInfoView
    {
        public Guid Id { get; set; }
        public string SymbolName { get; set; } = default!;
        public string SymbolIsin { get; set; } = default!;
        public long Price { get; set; }
        public long Quantity { get; set; }
        public int Side { get; set; }
        public int ValidityType { get; set; }
        public decimal Commission { get; set; }
        public int OrderModelType { get; set; }
        public int OrderFrom { get; set; }
    }
}