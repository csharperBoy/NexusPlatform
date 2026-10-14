namespace Trader.Application.Abstractions.Strategies;

/// <summary>
/// اطلاعات لازم برای UI هنگام ساخت Run. ConfigType برای ساخت فرم تنظیمات.
/// </summary>
public sealed record StrategyDescriptor(
    string Key,
    string DisplayName,
    Type ConfigType);