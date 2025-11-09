using Core.Domain.Specifications;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Abstractions
{
    /*
     📌 ISpecificationRepository<TEntity, TKey>
     ------------------------------------------
     این اینترفیس قرارداد Repository مبتنی بر **Specification Pattern** است.
     هدف آن جداسازی منطق Query از سرویس‌ها و فراهم کردن یک API عمومی برای
     اجرای کوئری‌های پیچیده بر اساس Specification می‌باشد.

     ✅ نکات کلیدی:
     - Generic Interface:
       → TEntity : class → موجودیت دامنه.
       → TKey : IEquatable<TKey> → کلید اصلی موجودیت (مثلاً Guid یا int).
     - استفاده از ISpecification<TEntity>:
       → Specification یک آبجکت است که شرایط Query را تعریف می‌کند.
       → این کار باعث می‌شود منطق Query قابل استفاده مجدد و تست‌پذیر باشد.

     - متدها:
       1. GetBySpecAsync(ISpecification<TEntity> specification)
          → دریافت یک موجودیت بر اساس Specification.
          → اگر داده‌ای پیدا نشود، مقدار null برمی‌گرداند.

       2. ListBySpecAsync(ISpecification<TEntity> specification)
          → دریافت لیست موجودیت‌ها بر اساس Specification.

       3. FindBySpecAsync(ISpecification<TEntity> specification)
          → دریافت لیست موجودیت‌ها همراه با تعداد کل (برای صفحه‌بندی).
          → خروجی: (Items, TotalCount).

       4. CountBySpecAsync(ISpecification<TEntity> specification)
          → شمارش موجودیت‌ها بر اساس Specification.

     🛠 جریان کار:
     1. سرویس‌های Query یا Handlerها یک Specification تعریف می‌کنند
        (مثلاً SampleGetSpec برای فیلتر کردن موجودیت‌ها بر اساس property1).
     2. این Specification به ISpecificationRepository داده می‌شود.
     3. Repository کوئری را اجرا کرده و داده‌ها را برمی‌گرداند.
     4. سرویس‌ها می‌توانند داده‌ها را صفحه‌بندی یا پردازش کنند.

     📌 نتیجه:
     این اینترفیس پایه‌ی مکانیزم **Specification Pattern** در معماری ماژولار است
     و باعث می‌شود منطق Query از سرویس‌ها جدا شود، قابل استفاده مجدد باشد،
     و تست‌پذیری بالاتری داشته باشد.
    */

    public interface ISpecificationRepository<TEntity, TKey>
        where TEntity : class
        where TKey : IEquatable<TKey>
    {
        Task<TEntity?> GetBySpecAsync(ISpecification<TEntity> specification); // 📌 دریافت یک موجودیت بر اساس Specification
        Task<IEnumerable<TEntity>> ListBySpecAsync(ISpecification<TEntity> specification); // 📌 دریافت لیست موجودیت‌ها
        Task<(IEnumerable<TEntity> Items, int TotalCount)> FindBySpecAsync(ISpecification<TEntity> specification); // 📌 لیست + تعداد کل (برای Paging)
        Task<int> CountBySpecAsync(ISpecification<TEntity> specification); // 📌 شمارش موجودیت‌ها
    }
}
