namespace Trader.Application.Abstractions.Runtime;

/// <summary>
/// در Startup اجرا می‌شود تا Runهایی که در DB حالت Running دارند،
/// در Runtime فعال شوند و Positionها با Broker Sync شوند.
/// </summary>
public interface IRuntimeRecoveryService
{
    Task RecoverAsync(CancellationToken ct = default);
}