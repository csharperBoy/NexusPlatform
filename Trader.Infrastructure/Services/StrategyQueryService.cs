using System.Globalization;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Strategies;
using Trader.Application.Dtos.Strategies;

namespace Trader.Application.Services;

public class StrategyQueryService : IStrategyQueryService
{
    private readonly IStrategyFactory _factory;

    public StrategyQueryService(IStrategyFactory factory)
    {
        _factory = factory;
    }

    public IReadOnlyList<StrategyInfoView> GetRegisteredStrategies()
    {
        return _factory.GetRegisteredStrategies()
            .Select(d => new StrategyInfoView(
                d.Key,
                d.DisplayName,
                d.ConfigType.Name,
                ExtractConfigFields(d.ConfigType)))
            .ToList();
    }

    /// <summary>
    /// استخراج فیلدهای Config با مقادیر Default از C# Property Initializer.
    /// </summary>
    private static IReadOnlyList<StrategyConfigFieldView> ExtractConfigFields(
        Type configType)
    {
        // یک Sample می‌سازیم تا مقادیر Default را از Initializerها بخوانیم.
        object? defaultInstance = null;
        try
        {
            defaultInstance = Activator.CreateInstance(configType);
        }
        catch
        {
            // اگر نتوانست بسازد، DefaultValue null می‌ماند.
        }

        return configType
            .GetProperties()
            .Where(p => p.Name != nameof(StrategyConfig.StrategyKey))
            .Where(p => p.CanRead)
            .Select(p =>
            {
                string? defaultValue = null;

                if (defaultInstance is not null)
                {
                    try
                    {
                        var value = p.GetValue(defaultInstance);
                        if (value is not null)
                        {
                            // InvariantCulture برای اعداد انگلیسی
                            defaultValue = value is IFormattable f
                                ? f.ToString(null, CultureInfo.InvariantCulture)
                                : value.ToString();
                        }
                    }
                    catch
                    {
                        // Ignore
                    }
                }

                // Required = Reference Type بدون Default (Value Typeها همیشه مقدار دارند)
                var isRequired =
                    !p.PropertyType.IsValueType && defaultValue is null;

                return new StrategyConfigFieldView(
                    Name: p.Name,
                    TypeName: p.PropertyType.Name,
                    IsRequired: isRequired,
                    DefaultValue: defaultValue);
            })
            .ToList();
    }
}