using Trader.Domain.Entities;

namespace Trader.Application.Runtime;

/// <summary>
/// پوشش OrderIntent + Ownership. مسیر Stage 3 → Stage 4.
/// </summary>
public sealed record OrderIntentEnvelope(
    Guid RunId,
    Guid TraderAccountId,
    OrderIntent Intent,
    DateTime CreatedAt);