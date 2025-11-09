using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Application.DTOs
{
    /*
     📌 SampleApiResponse
     --------------------
     این کلاس یک DTO برای خروجی API است.
     
     ✅ نکات کلیدی:
     - خروجی عملیات در قالب این DTO برگردانده می‌شود.
     - اینجا فقط یک property نمونه داریم (property1) که نشان‌دهنده نتیجه یا داده خروجی است.
     - استفاده از record باعث می‌شود خروجی سبک و خوانا باشد.

     🛠 کاربرد:
     بعد از اجرای Command یا Query، نتیجه در قالب این DTO به کلاینت یا لایه بالاتر برگردانده می‌شود.
     مثال: اگر عملیات موفق بود، پیام یا داده مرتبط در property1 قرار می‌گیرد.
    */
    public record SampleApiResponse(
     
     string property1
 );
}
