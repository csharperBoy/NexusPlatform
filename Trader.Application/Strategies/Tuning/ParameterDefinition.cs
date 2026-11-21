// Trader.Application/Strategies/Tuning/ParameterDefinition.cs
namespace Trader.Application.Strategies.Tuning;

public abstract record ParameterDefinition(
    string Name,
    ParameterImportance Importance,
    string? Description,
    string? DisplayName = null)
{
    public string EffectiveDisplayName => DisplayName ?? Name;
}

public sealed record IntParameter(
    string Name, int Min, int Max, int? Step,
    ParameterImportance Importance, string? Description, string? DisplayName = null)
    : ParameterDefinition(Name, Importance, Description, DisplayName);

public sealed record DecimalParameter(
    string Name, decimal Min, decimal Max, decimal? Step,
    ParameterImportance Importance, string? Description, string? DisplayName = null)
    : ParameterDefinition(Name, Importance, Description, DisplayName);

public sealed record BoolParameter(
    string Name, ParameterImportance Importance, string? Description, string? DisplayName = null)
    : ParameterDefinition(Name, Importance, Description, DisplayName);

public sealed record TimeFrameParameter(
    string Name, IReadOnlyList<int> AllowedValues,
    ParameterImportance Importance, string? Description, string? DisplayName = null)
    : ParameterDefinition(Name, Importance, Description, DisplayName);