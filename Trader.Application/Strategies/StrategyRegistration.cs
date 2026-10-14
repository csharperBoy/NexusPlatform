namespace Trader.Application.Abstractions.Strategies;

/// <summary>
/// کلاس پایه‌ی Generic برای Registration. Strategyها از این ارث می‌برند.
/// </summary>
public abstract class StrategyRegistration<TConfig> : IStrategyRegistration
    where TConfig : StrategyConfig
{
    public abstract string Key { get; }
    public abstract string DisplayName { get; }
    public Type ConfigType => typeof(TConfig);

    public IStrategy Create(StrategyConfig config)
    {
        if (config is not TConfig typed)
            throw new InvalidOperationException(
                $"Expected {typeof(TConfig).Name}, got {config.GetType().Name}.");
        return CreateStrategy(typed);
    }

    protected abstract IStrategy CreateStrategy(TConfig config);
}