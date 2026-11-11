namespace Trader.Domain.Enums;

public enum HistoricalJobState
{
    Created = 0,
    Running = 1,
    Stopped = 2,     // Recurring متوقف — کاربر می‌تواند Start مجدد کند
    Completed = 3,   // Backfill تمام — ترمینال
    Faulted = 4,     // خطای غیرقابل بازیابی
}