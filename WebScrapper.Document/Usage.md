# استفاده از ماژول WebScrapper در ماژول‌های دیگر

این فایل فقط یک سؤال را جواب می‌دهد: **اگر ماژول من باید از یک سایت بدون API داده بگیرد، چطور از WebScrapper استفاده کنم؟**

## ۱. خلاصهٔ در یک نگاه

| کاری که می‌خواهی | از چه چیزی استفاده کن |
|---|---|
| باز کردن یک صفحه و کار با عناصر | `IWebScrapperServicee<IElementHandle>` |
| خواندن تعداد ردیف‌های یک جدول | `Table_GetTableRowCount(tablePath)` |
| خواندن محتوای جدول | `Table_GetTableContent(...)` — ⚠️ فعلاً `NullReferenceException` می‌دهد |
| گرفتن Endpoint آماده | ❌ وجود ندارد؛ این ماژول `Presentation` ندارد |

## ۲. قرارداد عمومی

```csharp
// مسیر: WebScrapper.Application/Interfaces/IWebScrapperService.cs
public interface IWebScrapperServicee<TElement> where TElement : class
{
    Task InitializeAsync();
    Task NewTabPage<TPage, TWindow>(string url, TPage page, TWindow? window)
        where TPage : IPageContract
        where TWindow : IWindowContract<TPage>;
    Task GoToUrl(string url, string windowCode = "default", string pageCode = "default"); // NotImplemented
    Task Fill(ElementAccessPath elementPath, string value, TElement? BaseElement = null);
    Task Click(ElementAccessPath elementPath, TElement? BaseElement = null);
    Task<string> InnerText(ElementAccessPath elementPath, TElement? BaseElement = null);
    Task<bool> ElementIsExist(ElementAccessPath elementPath, TElement? BaseElement = null);
    Task WaitForLoad(ElementAccessPath elementPath, TElement? BaseElement = null);
    Task Wait(int millisecond);
    Task<string> GetCurrentUrl(string windowCode = "default", string pageCode = "default");

    Task<TableDto> Table_GetTableContent(TableElementAccessPath tableElementPath, TableRowDto? filterValues = null);
    Task<int> Table_GetTableRowCount(TableElementAccessPath tableElementPath);
    Task Table_ClickOnTableSubElement(TableElementAccessPath tableElementPath, string buttonColumnKey, TableRowDto? filterValues = null);
    Task Table_ClickOnTableRowSubElement(TableRowElementAccessPath tableRowElementPath, string buttonColumnKey);
    Task<TableRowDto> Table_GetTableRowContent(TableRowElementAccessPath tableRowElementPath);
    Task<List<MasterDetailTableDto>> Table_GetMasterDetailContent(TableElementAccessPath masterTablePath, TableElementAccessPath detailTablePath);
}
```

## ۳. مسیر توصیه‌شده

1. در csproj ماژول مصرف‌کننده، `WebScrapper.Application` را رفرنس بده و پکیج `Microsoft.Playwright` را هم اضافه کن (چون ژنریک قرارداد `IElementHandle` است).
2. در `Modules:Enabled` هاست، `{ "Name": "WebScrapper", "Order": 88 }` را اضافه کن تا `WebScrapper_AddInfrastructure` اجرا شود و `IPlaywright`/`IBrowser`/`IPage` ثبت شوند.
3. `IWebScrapperServicee<IElementHandle>` را در Constructor تزریق کن و **در ابتدای کار** یک بار `InitializeAsync()` را صدا بزن.

نمونهٔ واقعی:

```csharp
using Microsoft.Playwright;
using WebScrapper.Application.DTOs;
using WebScrapper.Application.Interfaces;
using WebScrapper.Domain.Common;
using WebScrapper.Domain.Enums;

internal sealed record ScrapPage(string code, int Number, string Title) : IPageContract;
internal sealed record ScrapWindow(int number, string code, string title, IEnumerable<ScrapPage> pages)
    : IWindowContract<ScrapPage>;

public class CollectPricesHandler : IRequestHandler<CollectPricesCommand, Result<int>>
{
    private readonly IWebScrapperServicee<IElementHandle> _scrapper;

    public CollectPricesHandler(IWebScrapperServicee<IElementHandle> scrapper) => _scrapper = scrapper;

    public async Task<Result<int>> Handle(CollectPricesCommand request, CancellationToken ct)
    {
        await _scrapper.InitializeAsync();

        // پنجره/صفحه فقط برای لاگ استفاده می‌شوند؛ هر پیاده‌سازی IPageContract قبول است
        await _scrapper.NewTabPage(
            "https://example.ir/prices",
            new ScrapPage("prices", 1, "قیمت‌ها"),
            new ScrapWindow(1, "main", "پنجرهٔ اصلی", Array.Empty<ScrapPage>()));

        var search = new ElementAccessPath(
            _Title: "کادر جست‌وجو",
            _DefaultAccessPath: ElementPathEnum.SelectorXpath,
            _SelectorXpath: "#txtSymbol",
            _ElementType: ElementTypeEnum.InputBox);

        await _scrapper.Fill(search, request.Symbol);
        await _scrapper.Wait(1000);

        var rowPath = new TableRowElementAccessPath(
            "ردیف جدول",
            new List<ElementAccessPath>
            {
                new("نماد", _Code: "symbol", _DefaultAccessPath: ElementPathEnum.SelectorXpath,
                    _SelectorXpath: "td:nth-child(1)", _ElementType: ElementTypeEnum.TableColumn),
                new("قیمت", _Code: "price", _DefaultAccessPath: ElementPathEnum.SelectorXpath,
                    _SelectorXpath: "td:nth-child(2)", _ElementType: ElementTypeEnum.TableColumn),
                new("جزئیات", _Code: "details", _DefaultAccessPath: ElementPathEnum.SelectorXpath,
                    _SelectorXpath: "button.details", _ElementType: ElementTypeEnum.Button)
            },
            _DefaultAccessPath: ElementPathEnum.SelectorXpath,
            _SelectorXpath: "tbody tr",
            _ElementType: ElementTypeEnum.TableRow);

        var tablePath = new TableElementAccessPath(
            "جدول قیمت‌ها", rowPath,
            _DefaultAccessPath: ElementPathEnum.SelectorXpath,
            _SelectorXpath: "table#prices",
            _ElementType: ElementTypeEnum.Table);

        var count = await _scrapper.Table_GetTableRowCount(tablePath);
        return Result<int>.Ok(count);
    }
}
```

## ۴. قواعد

- **همیشه `DefaultAccessPath` را ست کن.** اگر `null` بماند، سرویس به `FullXpath` برمی‌گردد و اگر آن هم `null` باشد `QuerySelectorAsync(null)` خطا می‌دهد.
- **CSS Selector بده، نه XPath.** هر چهار مقدار `ElementPathEnum` در نهایت به `QuerySelectorAsync` می‌رسند؛ رشته‌هایی مثل `//div[@id='x']` پیدا نمی‌شوند.
- **`_Code` را یکتا و معنادار بگذار.** در عملیات جدول، `TableColumnDto.key` همان `_Code` است و متدهای کلیک با `buttonColumnKey` روی همین مقدار تطبیق می‌دهند.
- **`ElementTypeEnum` را درست بگذار.** فقط عناصری که `TableColumn` هستند در خواندن جدول شرکت می‌کنند و فقط `Button`ها قابل کلیک‌اند.
- **`InitializeAsync` را یک بار صدا بزن.** این متد یک مرورگر جدید با `Headless = false` می‌سازد و مرورگر تزریق‌شده را دور می‌ریزد؛ روی سرور بدون نمایشگر آماده به کار نخواهد بود.
- **`NewWindow` و `GoToUrl` را صدا نزن.** هر دو `NotImplementedException` می‌دهند.
- **`Table_GetTableContent` را فعلاً صدا نزن.** به‌خاطر مقدار اولیه نداشتن `TableDto.rows` و `TableRowDto.columns` استثنا می‌دهد؛ تا اصلاح شود، از `Table_GetTableRowCount` و `InnerText` استفاده کن.
- **سرویس را Scoped نگه دار.** هر Scope یک Chromium تازه Launch می‌کند و سرویس مرورگر را نمی‌بندد؛ تعداد Scope را کم نگه دار و در عملیات دسته‌ای یک نمونه را دوباره استفاده کن.

## ۵. چک‌لیست وقتی می‌خواهی از WebScrapper استفاده کنی

1. آیا سایت واقعاً API ندارد؟ اگر دارد، scraping گران‌ترین انتخاب است.
2. `WebScrapper.Application` را رفرنس دادی و پکیج `Microsoft.Playwright` را هم اضافه کردی؟
3. `WebScrapper` در `Modules:Enabled` هاست هست و مرورگرهای Playwright نصب شده‌اند؟
4. `DefaultAccessPath` و `_Code` را روی همهٔ عناصر ست کردی؟
5. برای جدول، فقط متدهایی را صدا می‌زنی که امروز کار می‌کنند؟
6. برای خطا، Timeout و تکرار، سیاست خودت را در مصرف‌کننده گذاشتی (این ماژول Retry ندارد)؟
