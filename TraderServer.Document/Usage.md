# استفاده از ماژول TraderServer در ماژول‌های دیگر

این فایل فقط یک سؤال را جواب می‌دهد: **اگر ماژول من می‌خواهد سهمی را ثبت کند یا اسنپ‌شات بازار بگیرد، چطور از TraderServer استفاده کنم؟**

> ⚠️ **قبل از هر چیز:** این ماژول امروز build نمی‌شود، چون پروژه‌های `BrokerageOperations.*` در مخزن نیستند. هر نمونه‌کدی که در ادامه می‌بینی، **پس از بازگرداندن آن پروژه‌ها** معتبر است.

## ۱. خلاصهٔ در یک نگاه

| کاری که می‌خواهی | از چه چیزی استفاده کن |
|---|---|
| ثبت یک سهم جدید (درون‌فرایندی) | `AddStockCommand` از طریق `IMediator`، یا `IStockService.AddStock(...)` |
| گرفتن سهم بر اساس ISIN | `IStockService.GetStockByISIN(isin)` |
| ثبت اسنپ‌شات معاملات | `ICollectorService.SaveSnapShotStockTrading(...)` — ⚠️ امروز عملاً کار نمی‌کند |
| ثبت سهم از راه دور (HTTP) | `POST api/Stock/add` |
| خواندن سهام، حساب‌ها یا اسنپ‌شات‌ها | ❌ هیچ Query و هیچ Endpoint خواندنی وجود ندارد |

## ۲. قرارداد عمومی

```csharp
// مسیر: TraderServer.Application/Interface/IStockService.cs
public interface IStockService
{
    Task<Stock> GetStockByISIN(string ISIN);
    Task<Guid> AddStock(AddStockCommand command);
}

// مسیر: TraderServer.Application/Interface/ICollectorService.cs
public interface ICollectorService
{
    Task SaveSnapShotStockTrading(SaveSnapShotRequest req);
}

// مسیر: TraderServer.Application/Commands/AddStockCommand.cs
public record AddStockCommand(
    string Isin,
    string? Title,
    decimal? BuyCommissionRate,
    decimal? SellCommissionRate,
    StockTypeOfMarketEnum? TypeOfMarket = null,
    TimeOnly? PreOpeningTimeStart = null,
    TimeOnly? PreOpeningTimeEnd = null,
    TimeOnly? OpenTime = null,
    TimeOnly? CloseTime = null,
    int? TPlus = null,
    int? MinValueBuyOrder = null,
    int? MinValueSellOrder = null,
    int? StepPrice = null,
    bool? IsActive = null,
    string? CodeOfTsetmc = null,
    DateOnly? ReleaseDate = null,
    double? PercentOfDailyTolerance = null) : IRequest<Result<Guid>>;
```

## ۳. مسیر توصیه‌شده

1. در ماژول مصرف‌کننده فقط `TraderServer.Application` را رفرنس بده (نه `Infrastructure`).
2. برای نوشتن، `AddStockCommand` را با `IMediator.Send` بفرست؛ این تنها مسیری است که به موجودیت دامنه وابسته نمی‌شود.
3. اگر به `Stock` نیاز داری، بدان که `IStockService.GetStockByISIN` موجودیت دامنه را برمی‌گرداند و این نشت دامنه است؛ خروجی را بلافاصله به DTO خودت نگاشت کن.
4. `TraderServer` باید در `Modules:Enabled` هاست باشد و `TraderServer_AddInfrastructure` اجرا شده باشد؛ در غیر این صورت `IStockService` در DI وجود ندارد.

نمونه:

```csharp
using MediatR;
using Core.Shared.Results;
using TraderServer.Application.Commands;

public class RegisterSymbolHandler : IRequestHandler<RegisterSymbolCommand, Result<Guid>>
{
    private readonly IMediator _mediator;

    public RegisterSymbolHandler(IMediator mediator) => _mediator = mediator;

    public async Task<Result<Guid>> Handle(RegisterSymbolCommand request, CancellationToken ct)
    {
        var command = new AddStockCommand(
            Isin: request.Isin,
            Title: request.Title,
            BuyCommissionRate: 0.0035m,
            SellCommissionRate: 0.0088m,
            TPlus: 2,
            IsActive: true);

        return await _mediator.Send(command, ct);
    }
}
```

یا از راه دور:

```
POST /api/Stock/add
[AuthorizeResource("trader.stock", "Create")]
{ "isin": "IRO1FOLD0001", "title": "فولاد مبارکه", "buyCommissionRate": 0.0035, "sellCommissionRate": 0.0088 }
```

## ۴. قواعد

- **وابسته به Infrastructure نشو.** `TraderDbContext`، `Stock`، `BrokerageAccount`، Configurationها و `TraderSeedData` جزو API عمومی نیستند.
- **`IStockService` را وقتی تزریق کن که چاره‌ای نداری.** امضای آن موجودیت دامنه را برمی‌گرداند؛ مسیر تمیز، Command و MediatR است.
- **`Password` حساب کارگزاری را از این ماژول نخواه.** `BrokerageAccount.Password` متن ساده است و هیچ رمزنگاری‌ای ندارد؛ هر مصرف‌کننده‌ای که آن را بخواند، ریسک را تکثیر می‌کند.
- **`SaveSnapShotStockTrading` را فعلاً صدا نزن.** `CollectorService` به `IBrokerageFactory` وابسته است، `_account` هیچ‌وقت مقدار نمی‌گیرد و پلتفرم همیشه `EasyTrader` فرض می‌شود؛ در حال حاضر این مسیر در بهترین حالت دادهٔ اشتباه می‌دهد.
- **مجوز را از یاد نبر.** منبع `trader.stock` در `TraderSeedData` ثبت می‌شود؛ اگر Endpoint خودت را می‌سازی، همان کلید و اکشن (`Create`/`View`) را استفاده کن، نه یک کلید جدید.
- **در دیتابیس مشترک احتیاط کن.** اسکیمای پیش‌فرض `trader` است و ماژول `Trader` هم همان اسکیما را می‌خواهد؛ اگر هر دو در یک دیتابیس فعال شوند، جدول‌ها روی هم می‌افتند.

## ۵. چک‌لیست وقتی می‌خواهی از TraderServer استفاده کنی

1. آیا پروژه‌های `BrokerageOperations.*` برگشته‌اند و `TraderServer` build می‌شود؟
2. `TraderServer` در `Modules:Enabled` هاست هست؟
3. فقط `TraderServer.Application` را رفرنس دادی؟
4. برای نوشتن از `AddStockCommand` استفاده کردی، نه از Repository؟
5. خروجی `Stock` را به DTO خودت نگاشت کردی و موجودیت دامنه را از مرز ماژول رد نکردی؟
6. برای خواندن داده، به‌جای این ماژول، منبع دیگری (مثلاً `Trader` یا مستقیم دیتابیس تحلیلی) انتخاب کردی؟
