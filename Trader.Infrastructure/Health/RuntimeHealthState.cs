using Trader.Application.Abstractions.Health;

namespace Trader.Infrastructure.Health;

public sealed class RuntimeHealthState : IRuntimeHealthState
{
    private long _lastTickAtTicks;
    private long _lastBacktestPollAtTicks;
    private long _lastHistoricalPollAtTicks;

    public DateTime? LastTickAt
    {
        get
        {
            var t = Interlocked.Read(ref _lastTickAtTicks);
            return t > 0 ? new DateTime(t, DateTimeKind.Utc) : null;
        }
    }

    public DateTime? LastBacktestPollAt
    {
        get
        {
            var t = Interlocked.Read(ref _lastBacktestPollAtTicks);
            return t > 0 ? new DateTime(t, DateTimeKind.Utc) : null;
        }
    }



    public void MarkTickNow()
        => Interlocked.Exchange(ref _lastTickAtTicks, DateTime.UtcNow.Ticks);

    public void MarkBacktestPollNow()
        => Interlocked.Exchange(ref _lastBacktestPollAtTicks, DateTime.UtcNow.Ticks);

}