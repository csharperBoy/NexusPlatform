namespace Trader.Domain.Enums;

public enum TradingRunState
{
    Created = 0,   // ساخته شده، تنظیمات ناقص
    Ready = 1,     // تنظیمات کامل، آماده Start
    Running = 2,   // در حال اجرا
    Stopped = 3,   // متوقف شده توسط کاربر یا Market Close
    Faulted = 4    // خطای غیرقابل جبران، نیاز به بررسی دستی
}