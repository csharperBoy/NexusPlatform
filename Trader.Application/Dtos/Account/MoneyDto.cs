namespace Trader.Application.Dtos.Account
{
    public class MoneyDto
    {
        /// <summary>موجودی اصلی (ریال) — معمولاً همان T1</summary>
        public long T0 { get; set; }

        /// <summary>موجودی قابل برداشت</summary>
        public long T1 { get; set; }

        /// <summary>موجودی بلوکه‌شده (در سفارش‌های باز)</summary>
        public long T2 { get; set; }

        /// <summary>قدرت خرید (Buying Power)</summary>
        public long BuyPowerT0 { get; set; }
        public long BuyPowerT1 { get; set; }
        public long BuyPowerT2 { get; set; }

        /// <summary>بلوکه از T2</summary>
        public long BlockT2 { get; set; }

        /// <summary>بلوکه برداشت</summary>
        public long WithdrawBlockT2 { get; set; }

        /// <summary>بلوکه مارجین (برای اعتبار)</summary>
        public long MarginBlock { get; set; }

        /// <summary>اعتبار استفاده‌شده</summary>
        public long Credit { get; set; }

        /// <summary>اعتبار آوند</summary>
        public long AvandCredit { get; set; }

        /// <summary>موجودی قابل برداشت از کیف پول</summary>
        public long WalletWithdrawBalanceT0 { get; set; }

        /// <summary>ارزش اعتبار وارانت</summary>
        public long WarrantValueCredit { get; set; }

        /// <summary>موجودی صندوق حامی</summary>
        public long HamiBalance { get; set; }

        /// <summary>بلوکه (عمومی)</summary>
        public long Block { get; set; }
    }
}