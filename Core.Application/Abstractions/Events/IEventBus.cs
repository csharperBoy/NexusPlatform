using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
