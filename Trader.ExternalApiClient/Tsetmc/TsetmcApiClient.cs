using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Trader.ExternalApiClient.Tsetmc;

/// <summary>
/// کلاینت HTTP خام TSETMC. عمومی، بدون auth.
/// هیچ منطق دامنه‌ای اینجا نیست — فقط URL + GET + Deserialize.
///
/// ⚠️ هیچ‌کدام از endpointها نیازی به توکن ندارند.
/// </summary>
public sealed class TsetmcApiClient
{
    private const string HttpClientName = "Tsetmc";

    private readonly IHttpClientFactory _httpFactory;
    private readonly TsetmcOptions _options;
    private readonly ILogger<TsetmcApiClient> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public TsetmcApiClient(
        IHttpClientFactory httpFactory,
        IOptions<TsetmcOptions> options,
        ILogger<TsetmcApiClient> logger)
    {
        _httpFactory = httpFactory;
        _options = options.Value;
        _logger = logger;
    }

    /* ═══════════════ Static data ═══════════════ */

    /// <summary>
    /// GET /api/StaticData/GetStaticData
    ///
    /// ── چه داده‌ای برمی‌گرداند ──
    /// یک دیکشنری مرجع از کدها به نام‌های فارسی. شامل دو نوع:
    ///   1) PaperType — انواع اوراق (سهام، فرابورس پایه، تسهیلات مسکن، حق تقدم،
    ///      اوراق بدهی، اختیار معامله، آتی، صندوق، بورس کالا) — 9 رکورد.
    ///   2) IndustrialGroup — گروه‌های صنعتی (زراعت، خودرو، بانک، ...) — 67 رکورد.
    ///
    /// ── چه چیزی می‌فهمیم ──
    /// نگاشت (Type, Code) → Name فارسی. برای نمایش خروجی‌های دیگر
    /// (مثلاً گروه صنعتی یا نوع اوراق) به‌جای کد خام، به این جدول join می‌زنیم.
    /// این endpoint روز به روز ثابت است و در طول عمر کلاینت یک‌بار کافی است.
    ///
    /// ⚠️ فیلد Name در wire فاصله‌های انتهایی دارد — trim نمی‌کنیم، طبق قرارداد پروژه.
    /// </summary>
    public Task<TsetmcStaticDataResponse> GetStaticDataAsync(CancellationToken ct = default)
        => GetJsonAsync<TsetmcStaticDataResponse>(TsetmcEndpoints.StaticData, ct);

    /* ═══════════════ ClosingPrice (intraday) ═══════════════ */

    /// <summary>
    /// GET /api/ClosingPrice/GetClosingPriceHistory/{insCode}/{dEven}
    ///
    /// ── چه داده‌ای برمی‌گرداند ──
    /// سری زمانی درون‌روزی قیمت پایانی. هر رکورد یک snapshot در یک لحظه است:
    ///   • dEven         تاریخ (yyyyMMdd مثل 20261108)
    ///   • hEven         زمان (HHMMSS مثل 122958 = 12:29:58)
    ///   • pClosing      قیمت پایانی تجمعی تا آن لحظه
    ///   • pDrCotVal     آخرین قیمت معامله تا آن لحظه
    ///   • zTotTran      تعداد کل معاملات تا آن لحظه
    ///   • qTotTran5J    حجم کل معاملات تا آن لحظه
    ///   • qTotCap       ارزش کل معاملات تا آن لحظه
    ///
    /// ── چه چیزی می‌فهمیم ──
    /// کندل‌های درون‌روزی (هر چند ثانیه یک snapshot) → ساخت چارت intraday
    /// و استخراج حجم/ارزش تجمعی، سرعت معامله، تشخیص جهش قیمت.
    /// تعداد رکورد زیاد است (sample: 3522 رکورد برای یک روز) — برای روزهای
    /// با معاملات بالا حجم payload قابل توجه است.
    /// </summary>
    public Task<TsetmcClosingPriceHistoryResponse> GetClosingPriceHistoryAsync(
        string insCode, int dEven, CancellationToken ct = default)
        => GetJsonAsync<TsetmcClosingPriceHistoryResponse>(
            string.Format(
                CultureInfo.InvariantCulture,
                TsetmcEndpoints.ClosingPriceHistory,
                Uri.EscapeDataString(insCode), dEven),
            ct);

    /// <summary>
    /// GET /api/ClosingPrice/GetClosingPriceDaily/{insCode}/{dEven}
    ///
    /// ── چه داده‌ای برمی‌گرداند ──
    /// یک رکورد خلاصه‌ی پایان روز (EOD):
    ///   • priceChange     تغییر قیمت (نسبت به دیروز، به ریال — منفی/مثبت)
    ///   • priceMin/Max    کمترین/بیشترین قیمت روز
    ///   • priceYesterday  قیمت پایانی دیروز
    ///   • priceFirst      اولین قیمت امروز
    ///   • pClosing        قیمت پایانی امروز
    ///   • pDrCotVal       آخرین قیمت معامله
    ///   • zTotTran        تعداد معاملات
    ///   • qTotTran5J      حجم کل
    ///   • qTotCap         ارزش کل
    ///   • iClose/yClose   فلگ‌های متادیتای قیمت پایانی
    ///
    /// ── چه چیزی می‌فهمیم ──
    /// سریع‌ترین راه گرفتن آمار یک‌خطی روزانه‌ی یک نماد. مناسب برای:
    /// ساخت جدول روزانه، محاسبه‌ی درصد تغییر، فیلتر نمادهای پرحجم،
    /// و چک سریع وضعیت روز بدون دانلود کل سری درون‌روزی.
    /// </summary>
    public Task<TsetmcClosingPriceDailyResponse> GetClosingPriceDailyAsync(
        string insCode, int dEven, CancellationToken ct = default)
        => GetJsonAsync<TsetmcClosingPriceDailyResponse>(
            string.Format(
                CultureInfo.InvariantCulture,
                TsetmcEndpoints.ClosingPriceDaily,
                Uri.EscapeDataString(insCode), dEven),
            ct);

    /* ═══════════════ Order book (BestLimits) ═══════════════ */

    /// <summary>
    /// GET /api/BestLimits/{insCode}/{dEven}
    ///
    /// ── چه داده‌ای برمی‌گرداند ──
    /// تاریخ کامل دفتر سفارش (order book) در طول روز. هر snapshot یک refID دارد
    /// و شامل 5 سطح (number=1..5) است:
    ///   • qTitMeDem   حجم تقاضا در آن سطح
    ///   • zOrdMeDem   تعداد سفارش‌های خرید در آن سطح
    ///   • pMeDem      قیمت خرید در آن سطح
    ///   • pMeOf       قیمت فروش متناظر
    ///   • zOrdMeOf    تعداد سفارش‌های فروش در آن سطح
    ///   • qTitMeOf    حجم عرضه در آن سطح
    ///
    /// ── چه چیزی می‌فهمیم ──
    /// تکامل عمق بازار در طول روز. کاربردها:
    ///   • تشخیص دیوار خرید/فروش (حجم بزرگ در یک قیمت)
    ///   • تحلیل فشار خرید/فروش (bid/ask pressure)
    ///   • شناسایی لحظه‌های تغییر روند (spread باز/بسته شدن)
    ///   • ساخت heatmap عمق بازار
    ///
    /// ⚠️ حجم داده بسیار زیاد است (sample: 14160 رکورد برای یک روز) —
    /// برای هر (insCode, dEven) فقط یک‌بار fetch کن و cache کن.
    /// </summary>
    public Task<TsetmcBestLimitsResponse> GetBestLimitsAsync(
        string insCode, int dEven, CancellationToken ct = default)
        => GetJsonAsync<TsetmcBestLimitsResponse>(
            string.Format(
                CultureInfo.InvariantCulture,
                TsetmcEndpoints.BestLimits,
                Uri.EscapeDataString(insCode), dEven),
            ct);

    /* ═══════════════ Trade history (tick) ═══════════════ */

    /// <summary>
    /// GET /api/Trade/GetTradeHistory/{insCode}/{dEven}/{withDetails}
    ///
    /// ── چه داده‌ای برمی‌گرداند ──
    /// لیست تک‌تک معاملات روز (tick data):
    ///   • nTran        شماره‌ی معامله (توالی)
    ///   • hEven        زمان (HHMMSS)
    ///   • qTitTran     حجم معامله
    ///   • pTran        قیمت معامله
    ///   • qTitNgJ      حجم باقی‌مانده (سفارش ناقص)
    ///   • canceled     1 اگر معامله کنسل شده، 0 اگر نه
    ///   • iSensVarP    فلگ حساسیت (معمولاً "\u0000")
    ///
    /// ── چه چیزی می‌فهمیم ──
    /// ریزترین سطح داده‌ی معاملات. کاربردها:
    ///   • محاسبه‌ی دقیق VWAP (میانگین وزنی حجمی)
    ///   • تشخیص معاملات بزرگ/بلوکی (qTitTran بالا)
    ///   • ساخت footprint و تحلیل جریان سفارش
    ///   • تحلیل سرعت معامله در هر بازه
    ///   • بازسازی دنباله‌ی اجرا (order flow)
    ///
    /// ⚠️ sample: 4199 رکورد برای یک روز. برای نمادهای بسیار پرحجم بیشتر هم می‌شود.
    /// پارامتر withDetails در نمونه همیشه true است؛ false احتمالاً خلاصه‌تر برمی‌گرداند.
    /// </summary>
    public Task<TsetmcTradeHistoryResponse> GetTradeHistoryAsync(
        string insCode, int dEven, bool withDetails = true,
        CancellationToken ct = default)
        => GetJsonAsync<TsetmcTradeHistoryResponse>(
            string.Format(
                CultureInfo.InvariantCulture,
                TsetmcEndpoints.TradeHistory,
                Uri.EscapeDataString(insCode),
                dEven,
                withDetails ? "true" : "false"),
            ct);

    /* ═══════════════ Instrument metadata ═══════════════ */

    /// <summary>
    /// GET /api/Instrument/GetInstrumentHistory/{insCode}/{dEven}
    ///
    /// ── چه داده‌ای برمی‌گرداند ──
    /// متادیتای ثابت نماد:
    ///   • lVal30            نام کامل شرکت ("ایران خودرو")
    ///   • lVal18AFC         نماد ("خودرو")
    ///   • instrumentID      ISIN نماد در EasyTrader ("IRO1IKCO0001")
    ///   • cIsin             ISIN شرکت ("IRO1IKCO0008")
    ///   • zTitad            تعداد کل سهام شرکت
    ///   • baseVol           حجم پایه (minimum order size)
    ///   • flow              کد بازار (1=بورس، 2=فرابورس، ...)
    ///   • flowTitle         نام بازار
    ///   • cgrValCot + Title کد و نام گروه صنعتی (مثلاً "خودرو و ساخت قطعات")
    ///   • cComVal           کد کمیته
    ///
    /// ── چه چیزی می‌فهمیم ──
    /// این endpoint پل بین شناسه‌های داخلی TSETMC و شناسه‌های بیرونی است:
    ///   • insCode (TSETMC) ⇄ instrumentID (ISIN رایج در کارگزاری‌ها)
    ///   • نگاشت نماد به گروه صنعتی
    ///   • market cap مقدماتی (zTitad × قیمت)
    ///   • تشخیص بازار (بورس/فرابورس/پایه)
    ///
    /// ⚠️ quirk سرور: فیلد insCode در response همیشه "0" برمی‌گردد
    /// (حتی وقتی URL insCode واقعی داشت). پس برای نگاشت به TSETMC
    /// از خود پارامتر ورودی استفاده کن، نه این فیلد.
    /// </summary>
    public Task<TsetmcInstrumentHistoryResponse> GetInstrumentHistoryAsync(
        string insCode, int dEven, CancellationToken ct = default)
        => GetJsonAsync<TsetmcInstrumentHistoryResponse>(
            string.Format(
                CultureInfo.InvariantCulture,
                TsetmcEndpoints.InstrumentHistory,
                Uri.EscapeDataString(insCode), dEven),
            ct);

    /* ═══════════════ Client type (حقیقی/حقوقی) ═══════════════ */

    /// <summary>
    /// GET /api/ClientType/GetClientTypeHistory/{insCode}/{dEven}
    ///
    /// ── چه داده‌ای برمی‌گرداند ──
    /// خلاصه‌ی خرید/فروش تفکیک‌شده‌ی حقیقی و حقوقی برای یک روز:
    ///   • buy_I_Volume / buy_N_Volume    حجم خرید حقیقی(I) / حقوقی(N)
    ///   • buy_I_Value  / buy_N_Value     ارزش خرید حقیقی / حقوقی
    ///   • buy_I_Count  / buy_N_Count    تعداد خریداران حقیقی / حقوقی
    ///   • sell_I_* / sell_N_*           همان‌ها برای فروش
    ///
    /// ── چه چیزی می‌فهمیم ──
    /// تحلیل جریان پول هوشمند. کاربردها:
    ///   • پول حقیقی خالص = (buy_I_Value − sell_I_Value)
    ///   • تشخیص ورود/خروج پول حقیقی (سیگنال روند)
    ///   • نسبت خرید حقوقی به فروش حقوقی (فشار حقوقی)
    ///   • میانگین سایز خرید هر کد حقیقی (ارزش/تعداد)
    ///
    /// ⚠️ Buy_I_Count و Sell_I_Count در wire با ".0" می‌آیند (مثلاً 3406.0) —
    /// به همین دلیل decimal نگه داشته شده‌اند.
    /// </summary>
    public Task<TsetmcClientTypeResponse> GetClientTypeHistoryAsync(
        string insCode, int dEven, CancellationToken ct = default)
        => GetJsonAsync<TsetmcClientTypeResponse>(
            string.Format(
                CultureInfo.InvariantCulture,
                TsetmcEndpoints.ClientTypeHistory,
                Uri.EscapeDataString(insCode), dEven),
            ct);

    /* ═══════════════ StaticThreshold (دامنه‌ی نوسان) ═══════════════ */

    /// <summary>
    /// GET /api/MarketData/GetStaticThreshold/{insCode}/{dEven}
    ///
    /// ── چه داده‌ای برمی‌گرداند ──
    /// دامنه‌ی مجاز نوسان قیمت (سقف و کف) برای چند روز اخیر:
    ///   • psGelStaMax    سقف مجاز روز
    ///   • psGelStaMin    کف مجاز روز
    /// نمونه: دو رکورد (دیروز + امروز) برمی‌گرداند.
    ///
    /// ── چه چیزی می‌فهمیم ──
    /// محدوده‌ی مجاز معامله در آن روز. کاربردها:
    ///   • نمایش درصد فاصله‌ی قیمت فعلی تا سقف/کف
    ///   • تشخیص صف خرید (قیمت == psGelStaMax)
    ///   • تشخیص صف فروش (قیمت == psGelStaMin)
    ///   • محاسبه‌ی حاشیه‌ی امن برای سفارش‌گذاری
    ///   • چک مجاز بودن قیمت سفارش قبل از ارسال
    /// </summary>
    public Task<TsetmcStaticThresholdResponse> GetStaticThresholdAsync(
        string insCode, int dEven, CancellationToken ct = default)
        => GetJsonAsync<TsetmcStaticThresholdResponse>(
            string.Format(
                CultureInfo.InvariantCulture,
                TsetmcEndpoints.StaticThreshold,
                Uri.EscapeDataString(insCode), dEven),
            ct);

    /* ═══════════════ Shareholder ═══════════════ */

    /// <summary>
    /// GET /api/Shareholder/{insCode}/{dEven}
    ///
    /// ── چه داده‌ای برمی‌گرداند ──
    /// لیست سهامداران عمده (بالای 1%) به‌همراه تغییرات چند روز اخیر:
    ///   • shareHolderName   نام سهامدار
    ///   • numberOfShares    تعداد سهام
    ///   • perOfShares       درصد مالکیت
    ///   • change            1 اگر در این روز تغییری داشته
    ///   • changeAmount      مقدار تغییر (تعداد سهام)
    ///   • cIsin             ISIN شرکت
    /// نمونه: ~35 رکورد شامل دو روز (امروز + دیروز) برمی‌گرداند.
    ///
    /// ── چه چیزی می‌فهمیم ──
    /// ساختار مالکیت و تغییرات آن. کاربردها:
    ///   • تشخیص ورود/خروج سهامداران عمده (سیگنال مهم)
    ///   • تحلیل تمرکز مالکیت (چند سهامدار درصد بالا دارند)
    ///   • تشخیص تغییر کنترل شرکت
    ///   • رصد رفتار نهادهای مالی (صندوق بازنشستگی، سرمایه‌گذاری‌ها)
    ///
    /// ⚠️ dEven در رکوردها گاهی یک روز جلوتر از پارامتر URL است
    /// (sample: URL=20261108 ولی رکوردها 20261108 و 20261109) — به این quirk توجه کن.
    /// </summary>
    public Task<TsetmcShareholderResponse> GetShareholderAsync(
        string insCode, int dEven, CancellationToken ct = default)
        => GetJsonAsync<TsetmcShareholderResponse>(
            string.Format(
                CultureInfo.InvariantCulture,
                TsetmcEndpoints.Shareholder,
                Uri.EscapeDataString(insCode), dEven),
            ct);

    /* ═══════════════ Helpers ═══════════════ */

    private async Task<T> GetJsonAsync<T>(string path, CancellationToken ct)
        where T : class
    {
        var client = _httpFactory.CreateClient(HttpClientName);
        using var req = new HttpRequestMessage(HttpMethod.Get, path);

        using var res = await client.SendAsync(req, ct);
        var body = await res.Content.ReadAsStringAsync(ct);

        if (!res.IsSuccessStatusCode)
            throw new TsetmcException(
                $"Tsetmc GET {path} failed {(int)res.StatusCode}. " +
                $"Body: {Truncate(body, 300)}",
                (int)res.StatusCode, body);

        if (string.IsNullOrWhiteSpace(body))
            throw new TsetmcException(
                $"Tsetmc GET {path} returned empty body.",
                (int)res.StatusCode);

        try
        {
            return JsonSerializer.Deserialize<T>(body, JsonOpts)
                ?? throw new TsetmcException(
                    $"Tsetmc GET {path} deserialization returned null.");
        }
        catch (JsonException jex)
        {
            throw new TsetmcException(
                $"Tsetmc GET {path} JSON parse failed. " +
                $"Preview: {Truncate(body, 300)}. Inner: {jex.Message}",
                (int)res.StatusCode, body);
        }
    }

    private static string Truncate(string? s, int max)
    {
        if (string.IsNullOrEmpty(s)) return s ?? string.Empty;
        return s.Length <= max ? s : s[..max] + "...";
    }
}