using System.Text.Json.Serialization;

namespace Trader.ExternalApiClient.Tgju;

/// <summary>
/// پاسخ خام DataTables. هیچ parse منطقی اینجا نیست —
/// خام همان‌طور که server می‌دهد.
///
/// ستون‌های هر ردیف (طبق مشاهده):
///   [0]=Close, [1]=Low, [2]=High, [3]=Open,
///   [4]=Change(HTML), [5]=ChangePct(HTML),
///   [6]=Gregorian date (yyyy/MM/dd), [7]=Persian date
/// </summary>
public sealed class TgjuDataTablesResponse
{
    [JsonPropertyName("draw")] public string? Draw { get; set; }
    [JsonPropertyName("recordsTotal")] public int RecordsTotal { get; set; }
    [JsonPropertyName("recordsFiltered")] public int RecordsFiltered { get; set; }
    [JsonPropertyName("data")] public List<string[]> Data { get; set; } = new();
}