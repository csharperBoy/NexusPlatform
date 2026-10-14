namespace Trader.Application.Abstractions.Strategies;

/// <summary>
/// هر Strategy یک Registration برای Factory فراهم می‌کند.
/// به این ترتیب StrategyFactory نیازی به Reflection ندارد.
/// </summary>
public interface IStrategyRegistration
{
    string Key { get; }
    string DisplayName { get; }
    Type ConfigType { get; }

    IStrategy Create(StrategyConfig config);
}