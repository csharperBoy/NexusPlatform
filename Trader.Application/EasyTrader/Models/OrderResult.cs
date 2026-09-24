namespace Trader.Application.EasyTrader.Models
{
    public record OrderResult(
        bool IsSuccessful,
        string? OrderId,
        string? Message,
        OmsError? Error);

    public record OmsError(
        int Code,
        string Name,
        string Message);
}