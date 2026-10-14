namespace Trader.Application.Abstractions.Strategies;

public interface IStrategyFactory
{
    /// <summary>
    /// ساخت نمونه‌ی Strategy از روی Key و Config مشخص.
    /// Config باید با Key سازگار باشد.
    /// </summary>
    IStrategy Create(string strategyKey, StrategyConfig config);

    /// <summary>
    /// ساخت Config از JSON. نوع Config از روی Key تعیین می‌شود.
    /// </summary>
    StrategyConfig DeserializeConfig(string strategyKey, string configJson);

    /// <summary>لیست Strategyهای رجیستر‌شده برای UI.</summary>
    IReadOnlyList<StrategyDescriptor> GetRegisteredStrategies();
}