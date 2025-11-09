using Core.Domain.Specifications;
using Sample.Domain.Entities;

namespace Sample.Domain.Specifications
{
    /*
     📌 SampleGetSpec
     ----------------
     این کلاس یک Specification برای موجودیت SampleEntity است.
     Specificationها در معماری DDD برای تعریف شرایط Query استفاده می‌شوند
     تا منطق فیلتر، مرتب‌سازی و صفحه‌بندی به صورت قابل‌استفاده مجدد تعریف شود.

     ✅ نکات کلیدی:
     - شرط اصلی: property1 باید برابر مقدار ورودی باشد.
     - شامل‌سازی (Include): می‌توانیم Navigation Propertyها را بارگذاری کنیم (مثلاً User).
     - مرتب‌سازی (OrderBy / OrderByDescending / ThenOrderBy): ترتیب نتایج را مشخص می‌کنیم.
     - صفحه‌بندی (ApplyPaging): تعداد رکوردها و شروع صفحه را مشخص می‌کنیم.

     📌 نتیجه:
     این Specification نشان می‌دهد چطور می‌توانیم یک Query پیچیده را به صورت
     یک کلاس مستقل تعریف کنیم تا در سرویس‌های Query یا Repository استفاده شود.
    */

    public class SampleGetSpec : BaseSpecification<SampleEntity>
    {
        public SampleGetSpec(string property1, int page = 1, int pageSize = 10)
            : base(r => r.property1 == property1)
        {
            // 📌 شامل‌سازی Navigation Property (مثال: User)
            //AddInclude(r => r.User);

            // 📌 مرتب‌سازی صعودی بر اساس property1
            ApplyOrderBy(r => r.property1);

            // 📌 مرتب‌سازی نزولی بر اساس property1 (در صورت نیاز)
            // ApplyOrderByDescending(r => r.property1);

            // 📌 مرتب‌سازی ثانویه بر اساس Id
            ApplyThenOrderBy(r => r.Id, isDescending: false);

            // 📌 صفحه‌بندی: محاسبه skip و take
            var skip = (page - 1) * pageSize;
            ApplyPaging(skip, pageSize);
        }
    }
}
