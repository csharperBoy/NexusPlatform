namespace Trader.Application.Dtos.Account
{
    public class ClientAppSettingDto
    {
        public bool LightTheme { get; set; }
        public long BuyQuantity { get; set; }
        public long SellQuantity { get; set; }
        public decimal Tick { get; set; }
        public string TickType { get; set; } = default!;
        public bool PriceFromHeadline { get; set; }
        public bool OrderConfirmation { get; set; }
        public bool DivideOrderToMultiple { get; set; }
        public bool NotchUp { get; set; }
        public bool NotchDown { get; set; }
        public int PageSize { get; set; }
        public bool ApplyCommissionInPortfolio { get; set; }
        public bool UseClosingPriceInPortfolioTotalValue { get; set; }
        public bool ShowNotifications { get; set; }
        public bool DataTracker { get; set; }
        public bool UsePersianNumber { get; set; }
        public bool NoSleep { get; set; }
        public bool NoBalance { get; set; }
        public bool UserStatusBarToUp { get; set; }
        public bool PortfolioBasedOnLastPositivePeriod { get; set; }
        public int PortfolioTotalValueCalculateType { get; set; }
        public bool KeepOrderFormAfterSubmit { get; set; }
        public int PriceFromHeadlineSide { get; set; }
        public bool BlinkOnDataChange { get; set; }
        public int PortfolioBuyAveragePriceCalculationType { get; set; }
        public int PortfolioCalculationType { get; set; }
        public int SoldPortfolioBuyAveragePriceCalculationType { get; set; }
        public int SoldPortfolioCalculationType { get; set; }
        public decimal Volatility { get; set; }
        public decimal InterestRate { get; set; }
        public bool ShowAsLastPeriodAsset { get; set; }
    }
}