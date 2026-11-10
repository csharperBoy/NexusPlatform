namespace Trader.Domain.Enums;

public enum PriceCollectionJobState
{
    Created = 0,
    Running = 1,
    Stopped = 2,     // Recurring متوقف
    Completed = 3,   // Backfill تمام
    Faulted = 4,     // خطای غیرقابل بازیابی
}