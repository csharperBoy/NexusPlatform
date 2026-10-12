using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.ExternalApiClient.EasyTrader
{

    /// <summary>نتیجه‌ی یک API که انتظار redirect داره (authorize/login/callback).</summary>
    public readonly record struct RedirectResult(
        int StatusCode,
        string? Location,
        string? Body);

    /// <summary>نتیجه‌ی GET /Login — URL نهایی + HTML صفحه.</summary>
    public sealed record LoginPageResult(
        string LoginUrl,
        string Html);

    /// <summary>نتیجه‌ی GET callback — status + location + code + state استخراج‌شده.</summary>
    public sealed record OidcCallbackResult(
        int StatusCode,
        string? Location,
        string? Code,
        string? State);

    /// <summary>نتیجه‌ی ارسال سفارش + timings.</summary>
    public sealed record OrderSendResult(
        OrderResponse Response,
        long FireAtUnixMs,
        long ReceivedAtUnixMs);

    /// <summary>نتیجه‌ی خام HTTP برای endpointهایی که خروجی ساخت‌یافته ندارن.</summary>
    public readonly record struct RawHttpResult(
        bool IsSuccessful,
        int StatusCode,
        string? Body,
        long FireAtUnixMs,
        long ReceivedAtUnixMs);

}
