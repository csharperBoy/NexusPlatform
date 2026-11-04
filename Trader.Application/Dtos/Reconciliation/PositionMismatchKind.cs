namespace Trader.Application.Dtos.Reconciliation;

public enum PositionMismatchKind
{
    /// <summary>Position در DB و Broker هست، ولی تعداد مغایرت دارد.</summary>
    QuantityMismatch = 0,

    /// <summary>فقط در DB هست — Broker ندارد (Position Phantom).</summary>
    DbOnly = 1,

    /// <summary>فقط در Broker هست — DB ندارد (External/Manual Position).</summary>
    BrokerOnly = 2,
}