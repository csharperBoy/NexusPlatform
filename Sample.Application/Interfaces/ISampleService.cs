using Core.Shared.Results;
using Sample.Application.DTOs;
using Sample.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Sample.Application.Interfaces
{
    /*
     📌 ISampleService
     -----------------
     اینترفیس سرویس اصلی ماژول Sample.
     این سرویس مخصوص عملیات **نوشتن** یا تغییر وضعیت سیستم است (برخلاف ISampleQueryService که فقط خواندن انجام می‌دهد).

     ✅ نکات کلیدی:
     - این سرویس مسئول ایجاد Sample جدید و مدیریت تغییرات مرتبط است.
     - بعد از ایجاد Sample، رویداد دامنه (Domain Event) تولید می‌شود تا سایر بخش‌ها بتوانند واکنش نشان دهند.
     - این سرویس همچنین مسئول پاک‌سازی کش مرتبط بعد از عملیات نوشتن است تا داده‌های قدیمی در کش باقی نمانند.

     🛠 متدها:
     1. SampleApiMethodAsync
        - یک Sample جدید ایجاد می‌کند.
        - رویداد دامنه تولید می‌شود و در Outbox ذخیره می‌گردد.
        - نتیجه در قالب Result<SampleApiResponse> برگردانده می‌شود.

     2. SampleApiMethodWithCacheAsync
        - مشابه متد قبلی است اما علاوه بر ایجاد Sample، کش مرتبط با داده‌ها را پاک‌سازی می‌کند.
        - این کار تضمین می‌کند که داده‌های جدید در Queryها نمایش داده شوند.

     📌 نتیجه:
     این اینترفیس نشان می‌دهد که سرویس‌های Command/Write باید مسئول تغییر وضعیت سیستم باشند
     و در صورت نیاز کش را مدیریت کنند.
    */

    public interface ISampleService
    {
        Task<Result<SampleApiResponse>> SampleApiMethodAsync(SampleApiRequest request);

        Task<Result<SampleApiResponse>> SampleApiMethodWithCacheAsync(SampleApiRequest request);
    }
}
