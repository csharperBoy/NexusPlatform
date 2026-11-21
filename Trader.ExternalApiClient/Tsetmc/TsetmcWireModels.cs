using System.Text.Json.Serialization;

namespace Trader.ExternalApiClient.Tsetmc;

/* ═══════════════════ StaticData ═══════════════════ */

public sealed class TsetmcStaticDataResponse
{
    [JsonPropertyName("staticData")]
    public List<TsetmcStaticDataItem> StaticData { get; set; } = new();
}

public sealed class TsetmcStaticDataItem
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("code")] public int Code { get; set; }
    [JsonPropertyName("type")] public string? Type { get; set; }
    /// <summary>⚠️ دارای فاصله‌های انتهایی — دست‌نخورده.</summary>
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
}

/* ═══════════════════ ClosingPriceHistory (intraday) ═══════════════════ */

public sealed class TsetmcClosingPriceHistoryResponse
{
    [JsonPropertyName("closingPriceHistory")]
    public List<TsetmcClosingPriceHistoryItem> ClosingPriceHistory { get; set; } = new();
}

public sealed class TsetmcClosingPriceHistoryItem
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("insCode")] public string? InsCode { get; set; }
    [JsonPropertyName("dEven")] public int DEven { get; set; }
    /// <summary>HHMMSS — مثلاً 122958 = 12:29:58</summary>
    [JsonPropertyName("hEven")] public int HEven { get; set; }
    [JsonPropertyName("pClosing")] public decimal PClosing { get; set; }
    [JsonPropertyName("iClose")] public bool IClose { get; set; }
    [JsonPropertyName("yClose")] public bool YClose { get; set; }
    [JsonPropertyName("pDrCotVal")] public decimal PDrCotVal { get; set; }
    /// <summary>تعداد معاملات (wire: decimal با .0)</summary>
    [JsonPropertyName("zTotTran")] public decimal ZTotTran { get; set; }
    /// <summary>حجم معاملات (wire: decimal با .0)</summary>
    [JsonPropertyName("qTotTran5J")] public decimal QTotTran5J { get; set; }
    /// <summary>ارزش معاملات (wire: decimal با .00)</summary>
    [JsonPropertyName("qTotCap")] public decimal QTotCap { get; set; }
}

/* ═══════════════════ BestLimits (order book) ═══════════════════ */

public sealed class TsetmcBestLimitsResponse
{
    [JsonPropertyName("bestLimitsHistory")]
    public List<TsetmcBestLimitsItem> BestLimitsHistory { get; set; } = new();
}

public sealed class TsetmcBestLimitsItem
{
    [JsonPropertyName("idn")] public long Idn { get; set; }
    [JsonPropertyName("dEven")] public int DEven { get; set; }
    [JsonPropertyName("hEven")] public int HEven { get; set; }
    [JsonPropertyName("refID")] public long RefId { get; set; }
    /// <summary>سطح ۱ تا ۵</summary>
    [JsonPropertyName("number")] public int Number { get; set; }
    /// <summary>حجم تقاضا</summary>
    [JsonPropertyName("qTitMeDem")] public long QTitMeDem { get; set; }
    /// <summary>تعداد سفارش خرید</summary>
    [JsonPropertyName("zOrdMeDem")] public long ZOrdMeDem { get; set; }
    [JsonPropertyName("pMeDem")] public decimal PMeDem { get; set; }
    [JsonPropertyName("pMeOf")] public decimal PMeOf { get; set; }
    /// <summary>تعداد سفارش فروش</summary>
    [JsonPropertyName("zOrdMeOf")] public long ZOrdMeOf { get; set; }
    /// <summary>حجم عرضه</summary>
    [JsonPropertyName("qTitMeOf")] public long QTitMeOf { get; set; }
    [JsonPropertyName("title")] public string? Title { get; set; }
    [JsonPropertyName("insCode")] public string? InsCode { get; set; }
}

/* ═══════════════════ TradeHistory (tick-by-tick) ═══════════════════ */

public sealed class TsetmcTradeHistoryResponse
{
    [JsonPropertyName("tradeHistory")]
    public List<TsetmcTradeHistoryItem> TradeHistory { get; set; } = new();
}

public sealed class TsetmcTradeHistoryItem
{
    [JsonPropertyName("insCode")] public string? InsCode { get; set; }
    [JsonPropertyName("dEven")] public int DEven { get; set; }
    /// <summary>شماره‌ی معامله</summary>
    [JsonPropertyName("nTran")] public long NTran { get; set; }
    [JsonPropertyName("hEven")] public int HEven { get; set; }
    [JsonPropertyName("qTitTran")] public long QTitTran { get; set; }
    [JsonPropertyName("pTran")] public decimal PTran { get; set; }
    [JsonPropertyName("qTitNgJ")] public long QTitNgJ { get; set; }
    /// <summary>⚠️ می‌تواند "\u0000" باشد (کاراکتر null).</summary>
    [JsonPropertyName("iSensVarP")] public string? ISensVarP { get; set; }
    [JsonPropertyName("pPhSeaCotJ")] public decimal PPhSeaCotJ { get; set; }
    [JsonPropertyName("pPbSeaCotJ")] public decimal PPbSeaCotJ { get; set; }
    [JsonPropertyName("iAnuTran")] public int IAnuTran { get; set; }
    [JsonPropertyName("xqVarPJDrPRf")] public decimal XqVarPJDrPRf { get; set; }
    [JsonPropertyName("canceled")] public int Canceled { get; set; }
}

/* ═══════════════════ InstrumentHistory ═══════════════════ */

public sealed class TsetmcInstrumentHistoryResponse
{
    [JsonPropertyName("instrumentHistory")]
    public TsetmcInstrumentHistoryItem? InstrumentHistory { get; set; }
}

public sealed class TsetmcInstrumentHistoryItem
{
    /// <summary>⚠️ در sample مقدارش "0" بود حتی وقتی URL insCode واقعی داشت.</summary>
    [JsonPropertyName("insCode")] public string? InsCode { get; set; }
    /// <summary>نام کامل شرکت</summary>
    [JsonPropertyName("lVal30")] public string? LVal30 { get; set; }
    /// <summary>نماد</summary>
    [JsonPropertyName("lVal18AFC")] public string? LVal18AFC { get; set; }
    [JsonPropertyName("flow")] public int Flow { get; set; }
    [JsonPropertyName("cIsin")] public string? CIsin { get; set; }
    /// <summary>تعداد کل سهام (wire: decimal با .0)</summary>
    [JsonPropertyName("zTitad")] public decimal ZTitad { get; set; }
    [JsonPropertyName("baseVol")] public long BaseVol { get; set; }
    /// <summary>شناسه‌ی نماد در EasyTrader (IRO1...)</summary>
    [JsonPropertyName("instrumentID")] public string? InstrumentId { get; set; }
    [JsonPropertyName("cgrValCot")] public string? CgrValCot { get; set; }
    [JsonPropertyName("cComVal")] public string? CComVal { get; set; }
    [JsonPropertyName("lastDate")] public long LastDate { get; set; }
    [JsonPropertyName("sourceID")] public long SourceId { get; set; }
    [JsonPropertyName("flowTitle")] public string? FlowTitle { get; set; }
    [JsonPropertyName("cgrValCotTitle")] public string? CgrValCotTitle { get; set; }
}

/* ═══════════════════ ClientTypeHistory (حقیقی/حقوقی) ═══════════════════ */

public sealed class TsetmcClientTypeResponse
{
    [JsonPropertyName("clientType")]
    public TsetmcClientTypeItem? ClientType { get; set; }
}

public sealed class TsetmcClientTypeItem
{
    [JsonPropertyName("recDate")] public int RecDate { get; set; }
    [JsonPropertyName("insCode")] public string? InsCode { get; set; }

    [JsonPropertyName("buy_I_Volume")] public decimal Buy_I_Volume { get; set; }
    [JsonPropertyName("buy_N_Volume")] public decimal Buy_N_Volume { get; set; }
    [JsonPropertyName("buy_I_Value")] public decimal Buy_I_Value { get; set; }
    [JsonPropertyName("buy_N_Value")] public decimal Buy_N_Value { get; set; }
    [JsonPropertyName("buy_N_Count")] public long Buy_N_Count { get; set; }
    [JsonPropertyName("sell_I_Volume")] public decimal Sell_I_Volume { get; set; }
    [JsonPropertyName("buy_I_Count")] public decimal Buy_I_Count { get; set; }
    [JsonPropertyName("sell_N_Volume")] public decimal Sell_N_Volume { get; set; }
    [JsonPropertyName("sell_I_Value")] public decimal Sell_I_Value { get; set; }
    [JsonPropertyName("sell_N_Value")] public decimal Sell_N_Value { get; set; }
    [JsonPropertyName("sell_N_Count")] public long Sell_N_Count { get; set; }
    [JsonPropertyName("sell_I_Count")] public decimal Sell_I_Count { get; set; }
}

/* ═══════════════════ ClosingPriceDaily (EOD) ═══════════════════ */

public sealed class TsetmcClosingPriceDailyResponse
{
    [JsonPropertyName("closingPriceDaily")]
    public TsetmcClosingPriceDailyItem? ClosingPriceDaily { get; set; }
}

public sealed class TsetmcClosingPriceDailyItem
{
    [JsonPropertyName("priceChange")] public decimal PriceChange { get; set; }
    [JsonPropertyName("priceMin")] public decimal PriceMin { get; set; }
    [JsonPropertyName("priceMax")] public decimal PriceMax { get; set; }
    [JsonPropertyName("priceYesterday")] public decimal PriceYesterday { get; set; }
    [JsonPropertyName("priceFirst")] public decimal PriceFirst { get; set; }
    [JsonPropertyName("last")] public bool Last { get; set; }
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("insCode")] public string? InsCode { get; set; }
    [JsonPropertyName("dEven")] public int DEven { get; set; }
    [JsonPropertyName("hEven")] public int HEven { get; set; }
    [JsonPropertyName("pClosing")] public decimal PClosing { get; set; }
    [JsonPropertyName("iClose")] public bool IClose { get; set; }
    [JsonPropertyName("yClose")] public bool YClose { get; set; }
    [JsonPropertyName("pDrCotVal")] public decimal PDrCotVal { get; set; }
    [JsonPropertyName("zTotTran")] public decimal ZTotTran { get; set; }
    [JsonPropertyName("qTotTran5J")] public decimal QTotTran5J { get; set; }
    [JsonPropertyName("qTotCap")] public decimal QTotCap { get; set; }
}

/* ═══════════════════ StaticThreshold (دامنه‌ی نوسان) ═══════════════════ */

public sealed class TsetmcStaticThresholdResponse
{
    [JsonPropertyName("staticThreshold")]
    public List<TsetmcStaticThresholdItem> StaticThreshold { get; set; } = new();
}

public sealed class TsetmcStaticThresholdItem
{
    [JsonPropertyName("insCode")] public string? InsCode { get; set; }
    [JsonPropertyName("dEven")] public int DEven { get; set; }
    [JsonPropertyName("hEven")] public int HEven { get; set; }
    /// <summary>سقف مجاز</summary>
    [JsonPropertyName("psGelStaMax")] public decimal PsGelStaMax { get; set; }
    /// <summary>کف مجاز</summary>
    [JsonPropertyName("psGelStaMin")] public decimal PsGelStaMin { get; set; }
}

/* ═══════════════════ Shareholder (سهامداران) ═══════════════════ */

public sealed class TsetmcShareholderResponse
{
    [JsonPropertyName("shareShareholder")]
    public List<TsetmcShareholderItem> ShareShareholder { get; set; } = new();
}

public sealed class TsetmcShareholderItem
{
    [JsonPropertyName("shareHolderID")] public long ShareHolderId { get; set; }
    [JsonPropertyName("shareHolderName")] public string? ShareHolderName { get; set; }
    [JsonPropertyName("cIsin")] public string? CIsin { get; set; }
    [JsonPropertyName("dEven")] public int DEven { get; set; }
    [JsonPropertyName("numberOfShares")] public decimal NumberOfShares { get; set; }
    [JsonPropertyName("perOfShares")] public decimal PerOfShares { get; set; }
    [JsonPropertyName("change")] public int Change { get; set; }
    [JsonPropertyName("changeAmount")] public decimal ChangeAmount { get; set; }
    [JsonPropertyName("shareHolderShareID")] public long ShareHolderShareId { get; set; }
}