namespace Trader.Domain.Enums;

public enum TimeFrame
{
    M1 = 1,
    M5 = 5,
    M15 = 15,
    M30 = 30,
    H1 = 60,
    H4 = 240,
    D1 = 1440,
    W1 = 10080
}

public static class TimeFrameExtensions
{
    public static TimeSpan ToDuration(this TimeFrame tf)
        => TimeSpan.FromMinutes((int)tf);

    public static string ToCode(this TimeFrame tf) => tf switch
    {
        TimeFrame.M1 => "1m",
        TimeFrame.M5 => "5m",
        TimeFrame.M15 => "15m",
        TimeFrame.M30 => "30m",
        TimeFrame.H1 => "1h",
        TimeFrame.H4 => "4h",
        TimeFrame.D1 => "1d",
        TimeFrame.W1 => "1w",
        _ => throw new ArgumentOutOfRangeException(nameof(tf))
    };
}