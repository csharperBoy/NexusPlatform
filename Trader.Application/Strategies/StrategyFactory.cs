using System.Text.Json;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.Strategies;

namespace Trader.Application.Strategies;

public sealed class StrategyFactory : IStrategyFactory
{
    private readonly IReadOnlyDictionary<string, StrategyRegistration> _registrations;
    private readonly ILogger<StrategyFactory> _logger;

    public StrategyFactory(
        IEnumerable<IStrategyRegistration> registrations,
        ILogger<StrategyFactory> logger)
    {
        _logger = logger;

        _registrations = registrations.ToDictionary(
            r => r.Key,
            r => new StrategyRegistration(
                r.Key,
                r.DisplayName,
                r.ConfigType,
                r.Create), // ← متد گروهی، نه Property
            StringComparer.OrdinalIgnoreCase);
    }

    public IStrategy Create(string strategyKey, StrategyConfig config)
    {
        if (!_registrations.TryGetValue(strategyKey, out var reg))
            throw new InvalidOperationException(
                $"Strategy '{strategyKey}' is not registered.");

        if (!reg.ConfigType.IsInstanceOfType(config))
            throw new InvalidOperationException(
                $"Config type mismatch for strategy '{strategyKey}'. " +
                $"Expected {reg.ConfigType.Name}, got {config.GetType().Name}.");

        return reg.Factory(config);
    }

    public StrategyConfig DeserializeConfig(string strategyKey, string configJson)
    {
        if (!_registrations.TryGetValue(strategyKey, out var reg))
            throw new InvalidOperationException(
                $"Strategy '{strategyKey}' is not registered.");

        if (string.IsNullOrWhiteSpace(configJson))
            throw new ArgumentException("Config JSON is empty.", nameof(configJson));

        var config = JsonSerializer.Deserialize(configJson, reg.ConfigType,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            as StrategyConfig;

        if (config is null)
            throw new InvalidOperationException(
                $"Failed to deserialize config for strategy '{strategyKey}'.");

        return config;
    }

    public IReadOnlyList<StrategyDescriptor> GetRegisteredStrategies()
    {
        return _registrations.Values
            .Select(r => new StrategyDescriptor(r.Key, r.DisplayName, r.ConfigType))
            .ToList();
    }

    private sealed record StrategyRegistration(
        string Key,
        string DisplayName,
        Type ConfigType,
        Func<StrategyConfig, IStrategy> Factory);
}