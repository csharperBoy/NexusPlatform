namespace Trader.Application.Abstractions.Strategies;

/// <summary>
/// کلاس پایه برای Configهای Strategy.
/// هر Strategy یک Config مشخص دارد که از این ارث می‌برد.
/// Serialize/Deserialize به JSON از طریق JsonSerializer.
/// </summary>
public abstract class StrategyConfig
{
    /// <summary>شناسه‌ی Strategy که این Config به آن تعلق دارد.</summary>
    public abstract string StrategyKey { get; }
}