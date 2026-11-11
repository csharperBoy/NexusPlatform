using Trader.Domain.Enums;

namespace Trader.Infrastructure.Orders;

internal static class OrderLabels
{
    public static string State(OrderStateKind s) => s switch
    {
        OrderStateKind.Pending => "در انتظار",
        OrderStateKind.Open => "باز",
        OrderStateKind.PartiallyExecuted => "نیمه اجرا",
        OrderStateKind.FullyExecuted => "کامل اجرا",
        OrderStateKind.Canceled => "لغو شده",
        OrderStateKind.Rejected => "رد شده",
        OrderStateKind.Expired => "منقضی",
        _ => "نامشخص",
    };

    public static string IntentState(OrderIntentState s) => s switch
    {
        OrderIntentState.Pending => "در انتظار",
        OrderIntentState.Dispatched => "ارسال شده",
        OrderIntentState.Acknowledged => "تأیید شده",
        OrderIntentState.Rejected => "رد شده",
        OrderIntentState.Failed => "خطا",
        OrderIntentState.RiskBlocked => "مسدود توسط Risk",
        _ => "نامشخص",
    };

    public static string Side(OrderSide s) => s switch
    {
        OrderSide.Buy => "خرید",
        OrderSide.Sell => "فروش",
        _ => s.ToString(),
    };

    public static string Mode(OrderMode m) => m switch
    {
        OrderMode.Quantity => "تعداد",
        OrderMode.TotalValue => "ارزش کل",
        _ => m.ToString(),
    };
}