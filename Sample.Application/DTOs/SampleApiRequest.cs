using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Application.DTOs
{
    /*
     📌 SampleApiRequest
     -------------------
     این کلاس یک DTO (Data Transfer Object) برای ورودی‌های API است.
     
     ✅ نکات کلیدی:
     - DTO ها فقط داده را حمل می‌کنند و هیچ منطق تجاری ندارند.
     - اینجا دو property داریم (property1 و property2) که از سمت کلاینت یا لایه بالاتر به سرویس ارسال می‌شوند.
     - استفاده از record باعث می‌شود کلاس immutable باشد و برای انتقال داده مناسب‌تر است.

     🛠 کاربرد:
     وقتی کاربر یا سیستم بخواهد یک Sample جدید ایجاد کند، داده‌های ورودی در قالب این DTO به سرویس یا Handler ارسال می‌شوند.
    */
    public record SampleApiRequest(string property1, string property2);

}
