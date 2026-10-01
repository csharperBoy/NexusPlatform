namespace Trader.Application.Dtos.Order;

public class OrderHistoryQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;

    /// <summary>اگه خالی باشه، همه‌ی stateها برمی‌گردن.</summary>
    public List<int>? OrderStateRawFilter { get; set; }

    /// <summary>اگه خالی باشه، همه‌ی نمادها.</summary>
    public List<string>? SymbolIsins { get; set; }
}