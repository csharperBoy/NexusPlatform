namespace Trader.Application.EasyTrader.Models
{
    public record LoginResult(
        string AccessToken,
        string? IdToken,
        int ExpiresIn,
        string Scope);
}