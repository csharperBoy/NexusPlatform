namespace Trader.Domain.Enums;

public enum OrderIntentState
{
    Pending = 0,
    Dispatched = 1,
    Acknowledged = 2,
    Rejected = 3,       // Broker رد کرد
    Failed = 4,         // خطای شبکه/سیستم قبل از پاسخ Broker
    RiskBlocked = 5,    // ← NEW: Risk Guard مسدود کرد (قبل از Dispatch)
}