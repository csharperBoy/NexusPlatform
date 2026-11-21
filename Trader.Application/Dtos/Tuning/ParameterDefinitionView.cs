// Trader.Application/Dtos/Tuning/ParameterDefinitionView.cs
namespace Trader.Application.Dtos.Tuning;

public sealed record ParameterDefinitionView(
    string Name,
    string DisplayName,
    string TypeName,
    int Importance,
    string ImportanceLabel,
    string? Description,
    object? MinValue,
    object? MaxValue,
    IReadOnlyList<object>? AllowedValues);