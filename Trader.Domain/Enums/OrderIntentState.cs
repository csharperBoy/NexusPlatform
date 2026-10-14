namespace Trader.Domain.Enums;

public enum OrderIntentState
{
    Pending = 0,       // ساخته شده، هنوز به Broker نرفته
    Dispatched = 1,    // HttpRequest فرستاده شده، منتظر Response
    Acknowledged = 2,  // Broker پذیرفت، OrderId داده
    Rejected = 3,      // Broker رد کرد
    Failed = 4         // خطای شبکه/سیستم قبل از پاسخ Broker
}