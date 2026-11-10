namespace Trader.Application.Dtos.PriceCollection;

public sealed record CreatePriceSourceModel(
    string Code,
    string Name,
    int Type,
    string ConfigJson,
    string FromAsset,
    string ToAsset);