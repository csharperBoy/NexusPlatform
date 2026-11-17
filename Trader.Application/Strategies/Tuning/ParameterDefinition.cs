// Trader.Application/Strategies/Tuning/ParameterDefinition.cs
namespace Trader.Application.Strategies.Tuning;

public abstract record ParameterDefinition(string Name);

public sealed record IntParameter(string Name, int Min, int Max, int? Step = null)
    : ParameterDefinition(Name);

public sealed record DecimalParameter(string Name, decimal Min, decimal Max, decimal? Step = null)
    : ParameterDefinition(Name);

public sealed record BoolParameter(string Name) : ParameterDefinition(Name);

public sealed record TimeFrameParameter(string Name, IReadOnlyList<int> AllowedValues)
    : ParameterDefinition(Name);