# استفاده از ماژول StreamBus در ماژول‌های دیگر

این فایل فقط یک سؤال را جواب می‌دهد: **اگر ماژول من می‌خواهد داده‌ای را از یک استریم gRPC مصرف کند، چطور آن را وصل کنم که نه gRPC را بشناسم و نه به Infrastructure وابسته شوم؟**

## ۱. خلاصهٔ در یک نگاه

| کاری که می‌خواهی | از چه چیزی استفاده کن |
|---|---|
| مصرف پیام از یک استریم و پخش آن در MediatR | `AddStreamBusConsumer<TRequest,TResponse>(...)` + یک `INotificationHandler<StreamMessageNotification<TResponse>>` |
| ارسال پیام روی استریم باز | `IStreamBusClient<,>.SendAsync(...)` — در عمل از داخل Worker |
| انتخاب آدرس/متد/پیام اولیه | `StreamConsumerOptions<,>` |
| دیدن وضعیت اتصال | `IStreamBusClient<,>.State` (نوع `StreamConnectionState`) |
| ساختن کلاینت با `new` یا استفادهٔ مستقیم از gRPC | **نکن.** |

## ۲. قرارداد عمومی

```csharp
// مسیر: StreamBus.Application/Abstractions/IStreamBusClient.cs
public interface IStreamBusClient<TRequest, TResponse> : IAsyncDisposable
    where TRequest : class
    where TResponse : class
{
    StreamConnectionState State { get; }

    Task ConnectAsync(string address, Method<TRequest, TResponse> grpcMethod,
        CancellationToken cancellationToken = default);

    Task SendAsync(TRequest message, CancellationToken cancellationToken = default);

    IAsyncEnumerable<TResponse> ReceiveAllAsync(CancellationToken cancellationToken = default);
}

// مسیر: StreamBus.Application/Options/StreamConsumerOptions.cs
public class StreamConsumerOptions<TRequest, TResponse>
{
    public string ServerAddress { get; set; } = string.Empty;
    public Method<TRequest, TResponse>? GrpcMethod { get; set; }
    public TRequest? InitialRequest { get; set; }
}

// مسیر: StreamBus.Application/Notifications/StreamMessageNotification.cs
public record StreamMessageNotification<TMessage>(TMessage Message) : INotification
    where TMessage : class;
```

> ✅ **نکتهٔ خوب این ماژول:** برخلاف Cach و Event، قراردادش در `StreamBus.Application` است نه در `Core.Application`. پس ماژول مصرف‌کننده فقط به `StreamBus.Application` رفرنس می‌دهد و کاری با Core ندارد.
>
> ⚠️ **اما یک استثنا:** متد `AddStreamBusConsumer<,>` در `StreamBus.Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs` است. یعنی ثبت مصرف‌کننده باید در لایهٔ Infrastructure یا در Host انجام شود، نه در Application.

## ۳. مسیر توصیه‌شده

**گام ۱ — تعریف قرارداد gRPC.** چون در ریپو هیچ `.proto`ی نیست، `Method<TRequest,TResponse>` را خودت می‌سازی. ساده‌ترین راهِ بدون وابستگی اضافه، مارشالر JSON است:

```csharp
// Trader.Infrastructure/DependencyInjection/StreamBusRegistration.cs
using System.Text.Json;
using Grpc.Core;                                   // Method<,> و Marshallers
using StreamBus.Infrastructure.DependencyInjection; // AddStreamBusConsumer

public static class StreamBusRegistration
{
    public static IServiceCollection AddTickFeed(
        this IServiceCollection services, IConfiguration configuration)
    {
        var method = new Method<TickRequest, TickResponse>(
            type: MethodType.DuplexStreaming,
            serviceName: "nexus.market.TickFeed",
            name: "Subscribe",
            requestMarshaller: Marshallers.Create(
                (TickRequest r) => JsonSerializer.SerializeToUtf8Bytes(r),
                (byte[] d) => JsonSerializer.Deserialize<TickRequest>(d)!),
            responseMarshaller: Marshallers.Create(
                (TickResponse r) => JsonSerializer.SerializeToUtf8Bytes(r),
                (byte[] d) => JsonSerializer.Deserialize<TickResponse>(d)!));

        services.AddStreamBusConsumer<TickRequest, TickResponse>(opt =>
        {
            opt.ServerAddress = configuration["StreamBus:TickFeed:Address"]
                                ?? "https://localhost:7042";
            opt.GrpcMethod = method;
            opt.InitialRequest = new TickRequest { Channel = "market" };
        });

        return services;
    }
}
```

**گام ۲ — هندلر را در Application بنویس.** فقط `StreamBus.Application` را رفرنس بده:

```csharp
// Trader.Application/Notifications/TickReceivedHandler.cs
using MediatR;
using StreamBus.Application.Notifications;

public sealed class TickReceivedHandler
    : INotificationHandler<StreamMessageNotification<TickResponse>>
{
    private readonly ILogger<TickReceivedHandler> _logger;

    public TickReceivedHandler(ILogger<TickReceivedHandler> logger) => _logger = logger;

    public Task Handle(
        StreamMessageNotification<TickResponse> notification,
        CancellationToken cancellationToken)
    {
        var tick = notification.Message;

        _logger.LogDebug("Tick received: {Symbol} {Price}", tick.Symbol, tick.Price);
        // منطق سبک: به‌روزرسانی حافظه/کش یا enqueue برای پردازش بعدی

        return Task.CompletedTask;
    }
}
```

هندلر را دستی ثبت نکن؛ اگر اسمبلی Application ماژول تو لود شده باشد، `Core_AddInfrastructure` با `RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies())` آن را پیدا می‌کند.

**گام ۳ — مطمئن شو ماژول StreamBus فعال است.** `IStreamBusClient<,>` را خود `StreamBus_AddInfrastructure` ثبت می‌کند، پس `StreamBus` باید در `Modules:Enabled` باشد؛ وگرنه Worker در اولین Resolve خطا می‌دهد.

## ۴. قواعد

- **به `StreamBus.Infrastructure` از Application رفرنس نده.** فقط `AddStreamBusConsumer` در Infrastructure است؛ آن را در لایهٔ Infrastructure ماژول خودت یا در `Program.cs` صدا بزن.
- **`GrpcStreamBusClient` را با `new` نساز و `IStreamBusClient<,>` را خودت Register نکن.** ثبتش با `ResilientStreamBusClient` انجام شده؛ ثبت دوباره، رفتار مقاوم را دور می‌زند.
- **کلاینت را Singleton نکن.** Worker خودش Scope مناسب را می‌گیرد و کلاینت را در پایان آزاد می‌کند؛ نگه‌داشتن دستی، اتصال و Dispose را از دست می‌دهد.
- **هر جفت `TRequest/TResponse` فقط یک مصرف‌کننده.** `StreamConsumerOptions<,>` به‌صورت Singleton ثبت می‌شود؛ دو ثبت برای یک جفت نوع، هر دو Worker را با آخرین تنظیمات بالا می‌آورد.
- **`MethodType` باید `DuplexStreaming` باشد.** کلاینت روی `AsyncDuplexStreamingCall` بنا شده و متد ServerStreaming یا Unary با این قرارداد کار نمی‌کند.
- **هندلر را سریع و بدون استثنا نگه دار.** Worker پیام‌ها را پشت‌سرهم پردازش می‌کند و خطای هندلر را فقط لاگ و رد می‌کند؛ یک هندلر کند صف را می‌خواباند و یک استثنا پیام را بی‌صدا دور می‌ریزد. کار سنگین را به صف/Job بسپار.
- **روی دادهٔ استریم به‌عنوان «منبع حقیقت» حساب نکن.** بعد از هر قطعی، پیام‌های فاصلهٔ reconnect از دست می‌روند و Retry روی `SendAsync` می‌تواند باعث تکرار شود؛ هندلر را idempotent بنویس.
- **آدرس باید `https://` باشد.** هیچ تنظیمی برای gRPC بدون TLS و هیچ مکانیزمی برای توکن/Header وجود ندارد. به حجم پیام هم تکیه نکن؛ سقف دریافت برداشته شده است.

## ۵. چک‌لیست وقتی می‌خواهی یک Stream جدید اضافه کنی

1. آیا واقعاً استریم لازم است، یا یک Unary Call کافی است؟ برای دادهٔ یک‌بارمصرف، StreamBus ابزار اضافی است.
2. قرارداد `Method<TRequest,TResponse>` را ساختی و `MethodType.DuplexStreaming` است؟
3. `AddStreamBusConsumer<,>` در لایهٔ Infrastructure/Host صدا زده شده (نه Application)?
4. `ServerAddress` با `https://` شروع می‌شود و در appsettings است، نه hardcode؟
5. `InitialRequest` را فرستادی (اگر سرور برای شروع اشتراک به آن نیاز دارد)?
6. هندلر `INotificationHandler<StreamMessageNotification<TResponse>>` در Application ماژول توست و MediatR پیدایش می‌کند؟
7. `StreamBus` در `Modules:Enabled` فعال است و اسمبلی‌هایش لود می‌شوند؟
8. رفتار «قطعی اتصال» را تست کردی؟ (چه چیزی از دست می‌رود و چطور جبران می‌شود)
9. برای هر جفت نوع، فقط یک مصرف‌کننده ثبت شده؟
10. مصرف‌کننده‌ها را با `grep` پیدا کن: `AddStreamBusConsumer`، `StreamMessageNotification`.
