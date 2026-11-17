
// Trader.Domain/Enums/TuningScale.cs
namespace Trader.Domain.Enums;

[Flags]
public enum TuningScale
{
    None = 0,
    Yearly = 1,
    SemiAnnual = 2,
    Quarterly = 4,
    Monthly = 8,
    All = 15,
}