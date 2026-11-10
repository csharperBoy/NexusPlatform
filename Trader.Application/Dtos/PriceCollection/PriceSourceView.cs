namespace Trader.Application.Dtos.PriceCollection;

public sealed record PriceSourceView(
    Guid Id,
    string Code,
    string Name,
    int Type,
    string TypeName,
    string ConfigJson,
    string FromAsset,
    string ToAsset,
    bool IsActive,
    DateTime CreatedAt);