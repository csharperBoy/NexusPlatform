// Trader.Application/Strategies/Tuning/IStrategyTuner.cs
using System.Text.Json.Nodes;
using Trader.Domain.Enums;

namespace Trader.Application.Strategies.Tuning;

public interface IStrategyTuner
{
    string TunerKey { get; }
    IReadOnlyList<ParameterDefinition> GetParameterSpace(TimeFrame primaryTf);
    /// بازگرداندن پیام خطا یا null اگر معتبر است.
    string? ValidateConfiguration(JsonObject config, TimeFrame primaryTf);
    /// اصلاح constraintها بعد از mutation/crossover.
    JsonObject RepairConstraints(JsonObject config, TimeFrame primaryTf);
    JsonObject GetDefaults(TimeFrame primaryTf);
}