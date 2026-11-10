namespace Trader.Domain.Enums;

/// <summary>
/// نوع Provider یک PriceSource (D38).
/// هر Type یک IPriceSourceProvider دارد — افزودن Provider جدید فقط enum + Provider است.
/// </summary>
public enum PriceSourceType
{
    Tgju = 1,
    TelegramChannel = 2,   // Reserved — Provider در B2
    CustomApi = 3,         // Reserved
}