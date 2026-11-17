# ماژول WebScrapper (خزش مرورگری)

> **در یک جمله:** WebScrapper یک **قابلیت فنی** است، نه یک ماژول کسب‌وکار: با Playwright یک مرورگر واقعی Chromium را کنترل می‌کند تا صفحه باز کند، فیلد پر کند، کلیک کند و محتوای جدول را بخواند — جایی که سایت مقصد API ندارد.

| | |
|---|---|
| **پروژه‌ها** | `WebScrapper.Domain` / `WebScrapper.Application` / `WebScrapper.Infrastructure` — **بدون `WebScrapper.Presentation`** (عمدی) |
| **قرارداد عمومی** | `IWebScrapperServicee<TElement>` (توجه: «Servicee» با دو `e` — تایپوی موجود در کد) |
| **وابستگی به ماژول‌های دیگر** | هیچ ماژولی؛ فقط `Core.*` و پکیج `Microsoft.Playwright` |
| **چه کسانی از آن استفاده می‌کنند** | امروز هیچ کدی؛ تنها ارجاع، در `TraderServer.WebApi.csproj` است و در کد استفاده نمی‌شود |
| **جایگاه پیشنهادی در مدل لایه‌ای** | لایه ۰ (سرویس فنی) |

---

## ۱. این ماژول به چه درد می‌خورد؟ (به زبان ساده)

بعضی سامانه‌ها (سامانه‌های کارگزاری، پورتال‌های داخلی، سایت‌های گزارش) هیچ API ندارند و تنها راه گرفتن داده، «انجام دادن همان کاری است که یک کاربر با مرورگر می‌کند»: باز کردن آدرس، تایپ نام کاربری، کلیک روی دکمه و خواندن جدول نتیجه.

WebScrapper همین را کپسوله می‌کند. ایدهٔ اصلی، **آدرس‌دهی عنصر به‌جای هاردکد کردن کد مرورگر** است: برای هر عنصر یک `ElementAccessPath` می‌سازی که می‌گوید با کدام روش (FullXpath، Xpath، SelectorXpath، JSpath) و چه رشته‌ای آن را پیدا کند، چه نوعی دارد (`ElementTypeEnum`) و در کدام پنجره/صفحه (`windowCode`/`pageCode`) است. کد مصرف‌کننده فقط می‌گوید «این المان را پر کن» یا «این جدول را بخوان».

چون هیچ `Presentation` ندارد، مصرف آن **درون‌فرایندی** است: ماژولی که به scraping نیاز دارد، `IWebScrapperServicee<IElementHandle>` را تزریق می‌کند.

## ۲. چه چیزهایی داخلش هست؟

- **قرارداد `IWebScrapperServicee<TElement>`** با سه گروه متد:
  - ناوبری: `NewTabPage`، `NewWindow`، `GoToUrl`، `GetCurrentUrl`، `InitializeAsync`.
  - عنصر: `Fill`، `Click`، `InnerText`، `ElementIsExist`، `WaitForLoad`، `Wait`.
  - جدول: `Table_GetTableContent`، `Table_GetTableRowContent`، `Table_GetTableRowCount`، `Table_ClickOnTableSubElement`، `Table_ClickOnTableRowSubElement`، `Table_GetMasterDetailContent`.
- **DTOهای آدرس‌دهی** — `ElementAccessPath` (پایه)، `TableElementAccessPath` (جدول + `rowAccessPath` + متد `GetnumberOfRowFullXpath`)، `TableRowElementAccessPath` (ردیف + `columnsAccessPath`).
- **DTOهای نتیجه** — `TableDto`/`TableRowDto`/`TableColumnDto` (کلید-مقدار) و `MasterDetailTableDto` (یک ردیف اصلی + چند ردیف جزئیات).
- **قراردادهای دامنه** — `IPageContract` (با `code`، `Number`، `Title`) و `IWindowContract<TPage>` (با `number`، `code`، `title`، `pages`)؛ پیاده‌سازی‌های `PlaywrightPage` و `PlaywrightWindow` در Infrastructure.
- **شمارش‌ها** — `ElementPathEnum` (پنج روش آدرس‌دهی، از `FullXpath` تا `JSpath`) و `ElementTypeEnum` (`Button`، `InputBox`، `Table`، `TableColumn`، `TableRow` و…).
- **`PlaywrightScrapperService`** — پیاده‌سازی کامل با `FindElement`/`FindElements` خصوصی و کشف خودکار باز‌شدن ردیف‌های AG Grid در متد Master/Detail.
- **بدون دیتابیس** — هیچ `DbContext`، Migration، Outbox یا Seed ندارد؛ `WebScrapper_UseInfrastructure` کاری نمی‌کند.

## ۳. چه چیزی داخلش **نیست** (و نباید انتظارش را داشت)

- **Endpoint ندارد.** بدون `Presentation`، هیچ Route و Swagger‌ای برای scraping وجود ندارد؛ مصرف فقط کد است.
- **ذخیره‌سازی ندارد.** خروجی `TableDto` را خودت باید جایی بنویسی (Repository، فایل، صف).
- **Retry، Circuit Breaker و Timeout ندارد.** `Wait` فقط `Task.Delay` است؛ برای پایداری، سیاست Polly را در مصرف‌کننده بگذار.
- **تشخیص تغییر صفحه و انتظار هوشمند ندارد.** `WaitForLoad` هیچ چیزی را چک نمی‌کند.
- **منطق کسب‌وکار ندارد.** نمی‌داند داده سهام است یا شماره تلفن؛ فقط می‌خواند.

## ۴. یک نگاه فنی کوتاه

- ثبت در DI: `WebScrapper_AddDomain`، `WebScrapper_AddApplication`، `WebScrapper_AddInfrastructure`، `WebScrapper_AddHealthChecks` و `WebScrapper_UseInfrastructure`؛ با Reflection از `Modules:Enabled` پیدا و اجرا می‌شوند.
- چرخهٔ عمر در `WebScrapper_AddInfrastructure`: `IPlaywright` به‌صورت **Singleton**، `IBrowser` و `IBrowserContext` و `IPage` به‌صورت **Scoped** (با `AddScopedWithAsync` که در `Core.Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs` تعریف شده) و خودِ سرویس **Scoped**.
- راه‌اندازی مرورگر با خواندن `Playwright:Headless` از تنظیمات انجام می‌شود و یک `AddInitScriptAsync("delete navigator.webdriver;")` هم برای کاهش اثر تشخیص خودکار اضافه شده است.
- پیش‌نیاز اجرا: نصب مرورگرهای Playwright (`pwsh bin/Debug/net9.0/playwright.ps1 install`) — بدون آن، اولین Launch شکست می‌خورد.
- `FindElement` بر اساس `DefaultAccessPath` تصمیم می‌گیرد، ولی **همهٔ حالت‌ها** را به `QuerySelectorAsync`/`QuerySelectorAllAsync` می‌فرستد؛ یعنی موتور واقعی همیشه CSS است (بخش ۵).
- `EnsureViews`/`EnsureTrigger` این ماژول کامنت شده‌اند و `WebScrapper_AddHealthChecks` هیچ چکی اضافه نمی‌کند.
- **جایگاه قرارداد درست است.** برخلاف Audit و PhoneBook که قراردادشان در `Core.Application/Abstractions/...` نشسته، قرارداد این ماژول در `WebScrapper.Application/Interfaces` است؛ یعنی مصرف‌کننده فقط به خود ماژول وابسته می‌شود، نه به Core.

## ۵. وضعیت واقعی امروز (صادقانه)

- **دو متد عمداً پیاده نشده‌اند.** `NewWindow` و `GoToUrl` بدنه‌ای جز `throw new NotImplementedException()` ندارند. یعنی «پنجرهٔ جدید» و «رفتن به آدرس در صفحهٔ موجود» کار نمی‌کند و تنها مسیر واقعی، `NewTabPage` است (که آن هم فقط `_page` را جایگزین می‌کند و `window`/`page` ورودی را نادیده می‌گیرد).
- **تمام متدهای خواندن جدول با `NullReferenceException` می‌شکنند.** در `TableDto` و `TableRowDto` لیست‌های `rows` و `columns` مقدار اولیه ندارند و در سرویس هم `new TableDto()` بدون مقدار‌دهی ساخته می‌شود؛ اولین `tableRow.columns.Add(column)` استثنا می‌دهد. این شامل `Table_GetTableContent`، `Table_GetTableRowContent` و `Table_GetMasterDetailContent` است. تنها متد جدولی که کار می‌کند `Table_GetTableRowCount` است.
- **فیلتر جدول روی دادهٔ اشتباه کار می‌کند.** در `Table_ClickOnTableSubElement` متد `InnerText(columnPath)` بدون `BaseElement` صدا زده می‌شود، پس مقدار از کل صفحه خوانده می‌شود نه از ردیف جاری؛ نتیجهٔ فیلتر بی‌معنی است.
- **XPath واقعی پشتیبانی نمی‌شود.** `ElementPathEnum.Xpath` و `FullXpath` هم به `QuerySelectorAsync` پاس داده می‌شوند که فقط CSS Selector می‌فهمد؛ اگر رشته‌ات `//div[@id='x']` باشد، عنصر پیدا نمی‌شود و `FindElement` استثنای `Elementnot found!!!` می‌دهد. `localXpath` هم هیچ‌جا استفاده نمی‌شود.
- **`WaitForLoad` گمراه‌کننده است.** ورودی `elementPath` را نمی‌خواند و فقط ۲۰۰۰ میلی‌ثانیه صبر می‌کند؛ روی شبکهٔ کند یا صفحهٔ سنگین، ناکافی است.
- **مرورگر بسته نمی‌شود.** `PlaywrightScrapperService` نه `IDisposable` است و نه `IAsyncDisposable`؛ هیچ‌جا `CloseAsync` صدا زده نمی‌شود. چون `IBrowser` در هر Scope یک Chromium جدید Launch می‌کند، هر Request می‌تواند یک فرایند مرورگر باقی بگذارد. `InitializeAsync` هم `_playwright`/`_browser` تزریق‌شده را دور می‌ریزد و یک مرورگر دوم با `Headless = false` می‌سازد (تنظیم `Playwright:Headless` را نادیده می‌گیرد و روی سرور بدون نمایشگر شکست می‌خورد).
- **وابستگی Playwright به مصرف‌کننده نشت می‌کند.** ژنریک قرارداد `IElementHandle` است؛ پس هر مصرف‌کننده مجبور است پکیج `Microsoft.Playwright` را هم رفرنس بدهد. این با ایدهٔ «پنهان کردن جزئیات مرورگر» در تناقض است.
- **هیچ مصرف‌کننده و تستی ندارد؛** جست‌وجوی `IWebScrapperServicee` در مخزن فقط به خود ماژول می‌رسد.
- **وابستگی به AG Grid.** منطق Master/Detail به کلاس‌های `ag-row-group` و `ag-details-row` قفل شده؛ برای هر سایت دیگر باید بازنویسی شود.
