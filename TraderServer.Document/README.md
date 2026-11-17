# ماژول TraderServer (دادهٔ پایه و اسنپ‌شات بازار)

> **در یک جمله:** TraderServer قرار بود «مغز دادهٔ معاملاتی» باشد — تعریف سهام و قرارداد اختیار، حساب‌های کارگزاری کاربران و ثبت لحظه‌ای دفتر سفارش از طریق یک سرویس کارگزاری خارجی — ولی امروز **قابل build نیست**، چون چهار پروژهٔ `BrokerageOperations.*` که به آن‌ها ارجاع می‌دهد در این مخزن وجود ندارند.

| | |
|---|---|
| **پروژه‌ها** | `TraderServer.Domain` / `TraderServer.Application` / `TraderServer.Infrastructure` / `TraderServer.Presentation` (+ `TraderServer.WebApi` به‌عنوان هاست) |
| **قرارداد عمومی** | `IStockService` و `ICollectorService` در `TraderServer.Application/Interface` (بیرون از الگوی قراردادهای `Core.Application`) |
| **وابستگی به ماژول‌های دیگر** | `Core.*`، و **`BrokerageOperations.Application/Domain/Infrastructure/Shared` که وجود ندارند** |
| **چه کسانی از آن استفاده می‌کنند** | هیچ‌کس؛ هیچ پروژه‌ای در مخزن به `TraderServer.*` ارجاع نمی‌دهد |
| **جایگاه پیشنهادی در مدل لایه‌ای** | لایه ۲ |

---

## ۱. این ماژول به چه درد می‌خورد؟ (به زبان ساده)

برای معامله در بورس، سه چیز لازم است: بدانی چه نمادهایی مجازند و چه قواعدی دارند (`Stock`)، بدانی قراردادهای اختیار معامله روی کدام نمادها باز شده‌اند (`OptionContract` و `Option`)، و بتوانی وضعیت لحظه‌ای بازار را ببینی (`SnapShotFromStockTrading` و `SnapShotFromOptionTrading`).

TraderServer قصد داشت این‌ها را نگه دارد و برای گرفتن دادهٔ لحظه‌ای، به‌جای صحبت مستقیم با سایت کارگزاری، از یک لایهٔ واسط به نام `BrokerageOperations` استفاده کند: `IBrokerageFactory` یک `IBrokerageOperationsService` برای «پلتفرم» حساب می‌سازد (مثلاً `EasyTrader`) و `GetSnapShotFromTrade` را صدا می‌زند. `CollectorService` همان خروجی را به موجودیت `SnapShotFromStockTrading` تبدیل و ذخیره می‌کند: پنج سطح سفارش خرید، پنج سطح فروش، قیمت آخرین معامله، قیمت پایانی، حجم و ارزش معاملات و تجمیع‌های حقیقی/حقوقی.

## ۲. چه چیزهایی داخلش هست؟

- **موجودیت‌ها:** `Stock` (با `Isin`، `TypeOfMarket`، ساعات پیش‌گشایش/معامله، `TPlus`، کارمزد خرید و فروش، حداقل ارزش سفارش، `StepPrice`، `PercentOfDailyTolerance`)، `BrokerageAccount` (`UserName`، `Password`، `Platform`، `FkUserId`)، `OptionContract`، `Option`، `SnapShotFromStockTrading`، `SnapShotFromOptionTrading` و `StockFundPortfolio`.
- **قواعد دامنه:** سازندهٔ `Stock` مقادیر پیش‌فرض بازار ایران را ست می‌کند (پیش‌گشایش ۰۸:۴۵، بازگشایی ۰۹:۰۰، بستن ۱۲:۳۰، `TPlus = 1`، کارمزد ۰٫۰۱، حداقل سفارش ۵٬۰۰۰٬۰۰۰ ریال، تلورانس روزانه ۳٪). `StockByIsinSpec` جست‌وجو بر اساس `Isin` را می‌دهد.
- **Application:** یک Command (`AddStockCommand`)، یک Handler (`AddStockCommandHandler`)، و دو اینترفیس `IStockService` و `ICollectorService`.
- **Infrastructure:** `TraderDbContext` (اسکیمای `trader`)، هفت Configuration، یک Migration، `TraderSeedData` (ثبت منبع `trader.stock` و مجوز `Full` برای نقش ادمین)، `StockService`، `CollectorService`، `ModuleInitializer`، Health Check با نام `TraderDatabase` و پردازشگر Outbox.
- **Presentation:** `StockController` با `POST api/Stock/add` و فیلتر `[AuthorizeResource("trader.stock", "Create")]`، و یک `CollectorController` که هیچ Endpoint‌ای ندارد.
- **هاست:** `TraderServer.WebApi` با `TraderServer` در `Modules:Enabled`، اتصال به `TraderDb` و همان مکانیزم Reflection پلتفرم برای بارگذاری ماژول.

## ۳. چه چیزی داخلش **نیست** (و نباید انتظارش را داشت)

- **Query ندارد.** هیچ راه خواندنی برای سهام، حساب‌ها یا اسنپ‌شات‌ها وجود ندارد؛ نه Query، نه Controller.
- **مدیریت حساب کارگزاری ندارد.** `BrokerageAccount` فقط ذخیره می‌شود؛ نه Command‌ای برای ساخت آن هست، نه رمزنگاری‌ای برای `Password`.
- **مدیریت Option/OptionContract ندارد.** موجودیت و Configuration دارند، ولی هیچ سرویس یا Endpoint‌ای.
- **Outbox خالی است.** `stock.AddDomainEvent(...)` در `StockService` کامنت شده؛ رویدادی منتشر نمی‌شود.
- **تست ندارد.** پروژهٔ `TraderServer.Test` وجود ندارد.

## ۴. یک نگاه فنی کوتاه

- ثبت در DI: `TraderServer_AddDomain`، `TraderServer_AddApplication`، `TraderServer_AddInfrastructure`، `TraderServer_AddPresentation`، `TraderServer_AddHealthChecks` و `TraderServer_UseInfrastructure`.
- `TraderServer_AddInfrastructure` اول `services.BrokerageOperations_AddInfrastructure(configuration)` را صدا می‌زند و بعد `TraderDbContext` را با `MigrationsHistoryTable("__TraderMigrationsHistory", "Trader")` ثبت می‌کند.
- Repository مبتنی بر Specification برای `Option`، `OptionContract`، دو نوع SnapShot، `Stock` و `StockFundPortfolio` ثبت شده و `IUnitOfWork<TraderDbContext>` هم موجود است.
- `TraderServer_UseInfrastructure` با Reflection پیدا و اجرا می‌شود و `RunSmartMigrations` را روی `TraderDbContext` صدا می‌زند (در Development خطا را throw می‌کند، در Production فقط لاگ می‌کند و ادامه می‌دهد).
- `TraderServer_AddPresentation` دو بار `AddControllers().AddApplicationPart(...)` و دو بار `AddMediatR` را صدا می‌زند — تکرار بی‌اثر ولی نشانهٔ کپی‌کاری.

## ۵. وضعیت واقعی امروز (صادقانه)

- **ارجاع‌های شکسته، مهم‌ترین یافتهٔ این ماژول است.** `TraderServer.Domain.csproj` به `..\BrokerageOperations.Domain`، `TraderServer.Application.csproj` به `..\BrokerageOperations.Shared`، و `TraderServer.Infrastructure.csproj` و `TraderServer.WebApi.csproj` به `BrokerageOperations.Application` و `BrokerageOperations.Infrastructure` ارجاع می‌دهند. هیچ پوشه یا فایل سورسی `BrokerageOperations*` در مخزن نیست. کد هم واقعاً به آن‌ها وابسته است: `BrokerageOperations.Domain.Enums.BrokeragePlatformEnum` در `BrokerageAccount`، `BrokerageOperations.Shared.DTOs` (`StockDto`، `SnapShotDto`، `StockType`) در `CollectorService`، `BrokerageOperations.Application.Interface` (`IBrokerageOperationsService`، `IBrokerageFactory`) در همان سرویس، و `BrokerageOperations_AddInfrastructure` در DI. نتیجه: **این ماژول با وضعیت فعلی مخزن build نمی‌شود.**
- **شاهد اینکه قبلاً کار می‌کرد.** در `TraderServer.*/bin/Debug/net9.0` هنوز `BrokerageOperations.Domain.dll` و `BrokerageOperations.Shared.dll` باقی مانده‌اند؛ یعنی این پروژه‌ها روزی وجود داشته‌اند و بعداً از مخزن حذف یا جابه‌جا شده‌اند. تا وقتی سورسشان برنگردد، TraderServer یک قطعهٔ ناتمام است.
- **در Solution نیست.** هیچ‌کدام از پروژه‌های `TraderServer.*` در `NexusPlatform.sln` ثبت نشده‌اند؛ به همین دلیل ارجاع‌های شکسته در build روزمره دیده نمی‌شوند.
- **`CollectorService` عملاً بی‌فایده است.** `_account` هیچ‌وقت مقدار نمی‌گیرد (`SetAccount` در `ICollectorService` نیست و از بیرون قابل صدا زدن نیست)، پس `_account?.Platform ?? EasyTrader` همیشه `EasyTrader` می‌شود. `IBrokerageFactory` در سازنده صدا زده می‌شود، پس هر خطای ساخت سرویس در زمان ساخت خودِ Collector رخ می‌دهد.
- **تصادم اسکیما با ماژول `Trader`.** کلاس `TraderDbContext` در ماژول `Trader` (`Trader.Infrastructure/Data/TraderDbContext.cs`) و کلاس هم‌نامش در `TraderServer.Infrastructure` هر دو `HasDefaultSchema("trader")` دارند؛ اگر روزی هر دو در یک دیتابیس اجرا شوند، جدول‌ها و تاریخچهٔ Migration روی هم می‌افتند.
- **`StockFundPortfolio` در مدل EF نیست.** Configuration‌اش نوشته شده و Repository‌اش ثبت شده، ولی نه `DbSet` دارد و نه در `OnModelCreating` فراخوانی می‌شود؛ پس هر استفاده از آن در زمان اجرا شکست می‌خورد.
- **namespace اشتباه.** `TraderDbContext` در فایل `Data/TraderDbContext.cs` داخل namespace `TraderServer.Infrastructure.DependencyInjection` تعریف شده است (نه `...Infrastructure.Data`) — یک بدهی فنی که هر `using` را غافلگیر می‌کند.
- **منبع امنیتی تکراری.** `SnapShotFromOptionTrading` هم `[SecuredResource("Collector.SnapShotFromStockTrading")]` گرفته است؛ همان کلیدِ `SnapShotFromStockTrading`. کپی‌کاری در سطح مجوز.
- **رمز کارگزاری به‌صورت متن ساده.** `BrokerageAccount.Password` یک `string` ساده است و هیچ `ISecretProtector` یا رمزنگاری‌ای روی آن اعمال نمی‌شود؛ با توجه به اینکه `[SecuredResource("Trader.BrokerageAccount")]` هم برایش تعریف شده، این ریسک امنیتی جدی است.
- **بدون مصرف‌کننده.** هیچ پروژه‌ای در مخزن به `TraderServer.*` رفرنس نمی‌دهد؛ نه `Trader.WebApi`، نه `AkSteel.WebApi`. ماژول `Trader` در همان مخزن نسخهٔ بسیار کامل‌تری از همین دامنه است (سفارش، پوزیشن، بک‌تست) و به‌نظر نسل بعدی همین ایده است.
