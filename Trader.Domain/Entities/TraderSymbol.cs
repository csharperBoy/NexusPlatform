using Core.Domain.Common.EntityProperties;
using Trader.Domain.Enums;

namespace Trader.Domain.Entities
{
    public class TraderSymbol : BaseEntity
    {
        public string SymbolName { get; private set; } = default!;
        public string SymbolIsin { get; private set; } = default!;

        public long Price { get; private set; }
        public long Quantity { get; private set; }
        public OrderSide Side { get; private set; }
        public int ValidityType { get; private set; }
        public decimal Commission { get; private set; }
        public int OrderModelType { get; private set; }
        public int OrderFrom { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? UpdatedAt { get; private set; }

        private TraderSymbol() { }

        public static TraderSymbol Create(
            string symbolName,
            string symbolIsin,
            long price,
            long quantity,
            OrderSide side,
            int validityType,
            decimal commission,
            int orderModelType,
            int orderFrom)
        {
            return new TraderSymbol
            {
                SymbolName = symbolName,
                SymbolIsin = symbolIsin,
                Price = price,
                Quantity = quantity,
                Side = side,
                ValidityType = validityType,
                Commission = commission,
                OrderModelType = orderModelType,
                OrderFrom = orderFrom,
                CreatedAt = DateTimeOffset.UtcNow,
            };
        }

        public void SetInfo(
            string symbolName,
            string symbolIsin,
            long price,
            long quantity,
            OrderSide side,
            int validityType,
            decimal commission,
            int orderModelType,
            int orderFrom)
        {
            SymbolName = symbolName;
            SymbolIsin = symbolIsin;
            Price = price;
            Quantity = quantity;
            Side = side;
            ValidityType = validityType;
            Commission = commission;
            OrderModelType = orderModelType;
            OrderFrom = orderFrom;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}