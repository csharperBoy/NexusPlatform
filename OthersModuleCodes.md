این ماژول های جانبی هست که داریم و ممکنه بدردمون بخوره:
?   StreamBus.Application
?   ?   StreamBus.Application.csproj
?   ?   Abstractions
?   ?   ?   IStreamBusClient.cs
?   ?   DependencyInjection
?   ?   ?   ServiceCollectionExtensions.cs
?   ?   Notifications
?   ?   ?   StreamMessageNotification.cs
?   ?   Options
?   ?   ?   StreamConsumerOptions.cs
?   StreamBus.Domain
?   ?   StreamBus.Domain.csproj
?   ?   DependencyInjection
?   ?   ?   ServiceCollectionExtensions.cs
?   ?   Enums
?   ?   ?   StreamConnectionState.cs
?   ?   Exceptions
?   ?   ?   StreamBusException.cs
?   StreamBus.Infrastructure
?   ?   StreamBus.Infrastructure.csproj
?   ?   BackgroundServices
?   ?   ?   StreamBusConsumerWorker.cs
?   ?   DependencyInjection
?   ?   ?   ApplicationBuilderExtensions.cs
?   ?   ?   HealthCheckExtensions.cs
?   ?   ?   ModuleInitializer.cs
?   ?   ?   ServiceCollectionExtensions.cs
?   ?   Services
?   ?   ?   GrpcStreamBusClient.cs
?   ?   ?   ResilientStreamBusClient.cs
?   StreamBus.Presentation
?   ?   StreamBus.Presentation.csproj
?   ?   DependencyInjection
?   ?   ?   ServiceCollectionExtensions.cs
این اینترفیس هارو در اختیارمون میزاره:

namespace StreamBus.Application.Abstractions
{
    public interface IStreamBusClient<TRequest, TResponse> : IAsyncDisposable
    where TRequest : class
    where TResponse : class
    {
        StreamConnectionState State { get; }

        /// <summary>
        /// اتصال به سرور gRPC به صورت کاملاً ژنریک
        /// </summary>
        Task ConnectAsync(string address, Method<TRequest, TResponse> grpcMethod, CancellationToken cancellationToken = default);

        /// <summary>
        /// ارسال یک پیام روی استریم باز
        /// </summary>
        Task SendAsync(TRequest message, CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت مداوم پیام‌ها از سرور به صورت Async Stream
        /// </summary>
        IAsyncEnumerable<TResponse> ReceiveAllAsync(CancellationToken cancellationToken = default);
    }
}

همینطور این:

?   Event.Application
?   ?   Event.Application.csproj
?   Event.Domain
?   ?   Event.Domain.csproj
?   Event.Infrastructure
?   ?   Event.Infrastructure.csproj
?   ?   DependencyInjection
?   ?   ?   ApplicationBuilderExtensions.cs
?   ?   ?   HealthCheckExtensions.cs
?   ?   ?   OutboxProcessorRegistration.cs
?   ?   ?   ServiceCollectionExtensions.cs
?   ?   Processor
?   ?   ?   HybridOutboxProcessor.cs
?   ?   ?   OutboxProcessor.cs
?   ?   ?   PostgresOutboxProcessor.cs
?   ?   ?   SqlDependencyOutboxProcessor.cs
?   ?   ?   TriggerBasedOutboxProcessor.cs
?   ?   Services
?   ?   ?   MediatorEventBus.cs
?   ?   ?   OutboxService.cs
?   Event.Presentation
?   ?   Event.Presentation.csproj
?   ?   DependencyInjection
?   ?   ?   ServiceCollectionExtensions.cs

این اینترفیس هاییه که در اختیارمون میزاره:
namespace Core.Application.Abstractions.Events
{
    /*
     📌 IEventBus
     ------------
     این اینترفیس قرارداد مکانیزم انتشار رویدادها (Event Bus) در لایه Application است.
     هدف آن فراهم کردن یک API عمومی برای انتشار رویدادهای دامنه (Domain Events) یا
     رویدادهای بین‌ماژولی (Integration Events) به سایر بخش‌های سیستم می‌باشد.

     ✅ نکات کلیدی:
     - متدها:
       1. PublishAsync<TEvent>(TEvent @event)
          → انتشار یک رویداد منفرد.
          → رویداد باید از INotification ارث‌بری کند (الگوی MediatR).

       2. PublishAsync<TEvent>(params TEvent[] events)
          → انتشار چند رویداد به صورت همزمان.
          → برای سناریوهایی که چند رویداد در یک تراکنش تولید می‌شوند.

     🛠 جریان کار:
     1. موجودیت‌ها در لایه Domain رویداد تولید می‌کنند (مثلاً SampleCreatedIntegrationEvent).
     2. این رویدادها وارد Outbox می‌شوند یا مستقیم به EventBus داده می‌شوند.
     3. EventBus متد PublishAsync را فراخوانی می‌کند.
     4. MediatR یا مکانیزم مشابه، رویداد را به Handlerهای مربوطه ارسال می‌کند.
     5. Handlerها منطق واکنش به رویداد را اجرا می‌کنند (مثل ارسال نوتیفیکیشن، هماهنگی با سرویس دیگر).

     📌 نتیجه:
     این اینترفیس پایه‌ی مکانیزم Event-Driven در معماری ماژولار است و تضمین می‌کند
     که انتشار رویدادها به صورت استاندارد و مستقل از جزئیات پیاده‌سازی انجام شود.
     پیاده‌سازی آن می‌تواند در لایه Infrastructure باشد (مثلاً EventBus مبتنی بر MediatR یا پیام‌رسانی مثل RabbitMQ).
    */

    public interface IEventBus
    {
        Task PublishAsync<TEvent>(TEvent @event) where TEvent : INotification; // 📌 انتشار یک رویداد
        Task PublishAsync<TEvent>(params TEvent[] events) where TEvent : INotification; // 📌 انتشار چند رویداد
    }
}

namespace Core.Application.Abstractions.Events
{
    /*
     📌 IOutboxProcessorRegistration
     --------------------------------
     این اینترفیس قرارداد ثبت (Registration) پردازشگر Outbox در DI Container است.
     هدف آن فراهم کردن یک نقطه‌ی استاندارد برای اضافه کردن OutboxProcessor به سرویس‌ها می‌باشد.

     ✅ نکات کلیدی:
     - Outbox Pattern:
       → برای اطمینان از انتشار قابل اعتماد رویدادها استفاده می‌شود.
       → رویدادها ابتدا در دیتابیس ذخیره می‌شوند (Outbox Table).
       → سپس OutboxProcessor آن‌ها را خوانده و منتشر می‌کند.
     - این اینترفیس تضمین می‌کند که هر DbContext بتواند OutboxProcessor مخصوص به خودش را رجیستر کند.
     - متد اصلی: AddOutboxProcessor<TDbContext>
       → یک OutboxProcessor برای DbContext مشخص رجیستر می‌کند.
       → پارامتر services → DI Container که سرویس‌ها در آن رجیستر می‌شوند.
       → محدودیت TDbContext : DbContext → فقط DbContextهای EF Core پشتیبانی می‌شوند.

     🛠 جریان کار:
     1. در لایه Infrastructure، پیاده‌سازی این اینترفیس ارائه می‌شود.
     2. در زمان راه‌اندازی برنامه (Startup/Program.cs)، متد AddOutboxProcessor فراخوانی می‌شود.
     3. OutboxProcessor به DI Container اضافه می‌شود.
     4. OutboxProcessor در پس‌زمینه اجرا شده و رویدادهای ذخیره‌شده در Outbox Table را منتشر می‌کند.

     📌 نتیجه:
     این اینترفیس پایه‌ی مکانیزم **Outbox Registration** است و تضمین می‌کند
     که ماژول‌ها بتوانند به صورت ماژولار OutboxProcessor خود را رجیستر کنند،
     بدون اینکه به جزئیات پیاده‌سازی وابسته باشند.
    */

    public interface IOutboxProcessorRegistration
    {
        IServiceCollection AddOutboxProcessor<TDbContext>(IServiceCollection services)
            where TDbContext : DbContext;
    }
}

namespace Core.Application.Abstractions.Events
{
    /*
     📌 IOutboxService<TDbContext>
     -----------------------------
     این اینترفیس قرارداد سرویس Outbox در معماری Event-Driven است.
     هدف آن مدیریت چرخه‌ی عمر رویدادهای دامنه (Domain Events) در جدول Outbox می‌باشد،
     تا انتشار رویدادها به صورت قابل اعتماد و مقاوم در برابر خطا انجام شود.

     ✅ نکات کلیدی:
     - Outbox Pattern:
       → رویدادها ابتدا در دیتابیس ذخیره می‌شوند (Outbox Table).
       → سپس پردازشگر Outbox آن‌ها را خوانده و منتشر می‌کند.
       → این الگو از دست رفتن رویدادها در صورت خطا یا قطعی ارتباط جلوگیری می‌کند.

     - متدها:
       1. AddEventsAsync(IEnumerable<IDomainEvent> domainEvents)
          → افزودن رویدادهای دامنه به جدول Outbox.

       2. GetPendingMessagesAsync(int batchSize = 100)
          → دریافت پیام‌های منتشرنشده (Pending) برای پردازش.
          → امکان تعیین اندازه Batch برای پردازش دسته‌ای.

       3. MarkAsProcessingAsync(Guid messageId)
          → علامت‌گذاری پیام به عنوان "در حال پردازش".

       4. MarkAsCompletedAsync(Guid messageId)
          → علامت‌گذاری پیام به عنوان "پردازش موفق".

       5. MarkAsFailedAsync(Guid messageId, Exception ex)
          → علامت‌گذاری پیام به عنوان "پردازش ناموفق" همراه با جزئیات خطا.

       6. CleanupProcessedMessagesAsync(DateTime olderThan)
          → پاک‌سازی پیام‌های پردازش‌شده قدیمی‌تر از تاریخ مشخص.
          → برای جلوگیری از رشد بی‌رویه جدول Outbox.

     🛠 جریان کار:
     1. موجودیت‌ها رویداد دامنه تولید می‌کنند.
     2. رویدادها توسط UnitOfWork در جدول Outbox ذخیره می‌شوند.
     3. OutboxProcessor پیام‌های Pending را با استفاده از IOutboxService می‌خواند.
     4. پیام‌ها منتشر می‌شوند و وضعیت آن‌ها به Completed یا Failed تغییر می‌کند.
     5. پیام‌های قدیمی پاک‌سازی می‌شوند تا جدول Outbox سبک بماند.

     📌 نتیجه:
     این اینترفیس پایه‌ی مکانیزم Outbox در معماری ماژولار است و تضمین می‌کند
     که انتشار رویدادها به صورت قابل اعتماد، مقاوم در برابر خطا و قابل مانیتورینگ انجام شود.
     پیاده‌سازی آن در لایه Infrastructure خواهد بود (مثلاً OutboxService مبتنی بر EF Core).
    */

    public interface IOutboxService<TDbContext>
         where TDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        Task AddEventsAsync(IEnumerable<IDomainEvent> domainEvents); // 📌 افزودن رویدادها به Outbox
        Task<IEnumerable<OutboxMessage>> GetPendingMessagesAsync(int batchSize = 100); // 📌 دریافت پیام‌های منتشرنشده
        Task MarkAsProcessingAsync(Guid messageId); // 📌 علامت‌گذاری به عنوان در حال پردازش
        Task MarkAsCompletedAsync(Guid messageId); // 📌 علامت‌گذاری به عنوان پردازش موفق
        Task MarkAsFailedAsync(Guid messageId, Exception ex); // 📌 علامت‌گذاری به عنوان پردازش ناموفق
        Task CleanupProcessedMessagesAsync(DateTime olderThan); // 📌 پاک‌سازی پیام‌های قدیمی پردازش‌شده
    }
}

و همینطور این ماژول:
?   Cach.Application
?   ?   Cach.Application.csproj
?   ?   DependencyInjection
?   ?   ?   ServiceCollectionExtensions.cs
?   ?   Interface
?   ?   ?   ICacheInternalService.cs
?   ?   Models
?   ?   ?   CacheSettings.cs
?   Cach.Domain
?   ?   Cach.Domain.csproj
?   Cach.Infrastructure
?   ?   Cach.Infrastructure.csproj
?   ?   HelpMe_Redis.txt
?   ?   DependencyInjection
?   ?   ?   ApplicationBuilderExtensions.cs
?   ?   ?   HealthCheckExtensions.cs
?   ?   ?   ServiceCollectionExtensions.cs
?   ?   HealthChecks
?   ?   ?   CacheHealthCheck.cs
?   ?   Services
?   ?   ?   MemoryCacheService.cs
?   ?   ?   RedisCacheService.cs
?   Cach.Presentation
?   ?   Cach.Presentation.csproj
?   ?   DependencyInjection
?   ?   ?   ServiceCollectionExtensions.cs
این هم اینترفیسیه که در اختیارمون میازه:
namespace Core.Application.Abstractions.Caching.PublicService
{
    /*
     📌 ICacheService
     ----------------
     این اینترفیس قرارداد سرویس کش (Caching) در لایه Application است.
     هدف آن فراهم کردن یک API عمومی برای مدیریت داده‌های کش شده است،
     بدون وابستگی به جزئیات پیاده‌سازی (مثلاً MemoryCache، Redis، یا Distributed Cache).

     ✅ نکات کلیدی:
     - متدها:
       1. GetAsync<T>(string key)
          → دریافت داده از کش بر اساس کلید.
          → اگر داده وجود نداشته باشد، مقدار null برمی‌گرداند.

       2. SetAsync<T>(string key, T value, TimeSpan? expiration = null)
          → ذخیره داده در کش با کلید مشخص.
          → امکان تعیین زمان انقضا (Expiration) وجود دارد.

       3. RemoveAsync(string key)
          → حذف داده از کش بر اساس کلید.

       4. ExistsAsync(string key)
          → بررسی وجود داده در کش.

       5. RemoveByPatternAsync(string pattern)
          → حذف داده‌ها بر اساس الگوی کلید (Pattern Matching).
          → برای سناریوهایی مثل invalidation بعد از عملیات نوشتن استفاده می‌شود.

     🛠 جریان کار:
     1. سرویس‌های Query داده‌ها را از Repository می‌خوانند.
     2. داده‌ها در کش ذخیره می‌شوند تا درخواست‌های بعدی سریع‌تر پاسخ داده شوند.
     3. سرویس‌های Command بعد از تغییر داده‌ها، کش مرتبط را پاک می‌کنند (Invalidate).
     4. اینترفیس ICacheService تضمین می‌کند که لایه Application فقط قرارداد را بشناسد،
        و پیاده‌سازی در لایه Infrastructure انجام شود.

     📌 نتیجه:
     این اینترفیس پایه‌ی مکانیزم کش در معماری ماژولار است و امکان جایگزینی
     پیاده‌سازی‌های مختلف (MemoryCache، Redis، NCache، ...) را بدون تغییر در لایه Application فراهم می‌کند.
    */

    public interface ICachePublicService
    {
        Task<T?> GetAsync<T>(string key); // 📌 دریافت داده از کش
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null); // 📌 ذخیره داده در کش با زمان انقضا
        Task RemoveAsync(string key); // 📌 حذف داده از کش
        Task<bool> ExistsAsync(string key); // 📌 بررسی وجود داده در کش
        Task RemoveByPatternAsync(string pattern); // 📌 حذف داده‌ها بر اساس الگو (برای invalidation)
    }
}
