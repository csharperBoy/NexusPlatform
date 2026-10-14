using Trader.Domain.Entities;

namespace Trader.Application.Runtime;

/// <summary>
/// پوشش Signal + Ownership. مسیر Stage 2 → Stage 3.
/// </summary>
public sealed record SignalEnvelope(
    Guid RunId,
    Signal Signal,
    DateTime ProducedAt);