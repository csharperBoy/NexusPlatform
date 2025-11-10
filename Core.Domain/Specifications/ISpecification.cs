using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
namespace Core.Domain.Specifications
{
    /*
     📌 ISpecification<T>
     --------------------
     این اینترفیس قرارداد پایه برای پیاده‌سازی **Specification Pattern** در معماری DDD است.
     هدف آن استانداردسازی ساختار Specificationها برای تعریف Queryهای پیچیده و قابل ترکیب
     بدون وابستگی مستقیم به دیتابیس یا EF Core می‌باشد.

     ✅ نکات کلیدی:
     - Criteria → شرط اصلی فیلتر (Expression<Func<T, bool>>).
       • مشخص می‌کند که چه رکوردهایی باید انتخاب شوند.
       • مثال: product => product.Price > 100

     - Includes → لیست Navigation Properties برای eager loading.
     - IncludeFunctions → توابعی برای Includeهای پیچیده (با ThenInclude).
     - IncludeStrings → لیست Include به صورت رشته‌ای (برای سناریوهای خاص).

     - OrderBy / OrderByDescending → مرتب‌سازی اصلی.
     - ThenOrderBy → مرتب‌سازی‌های چند سطحی (چندین کلید مرتب‌سازی).

     - Paging:
       • Take → تعداد رکوردهایی که باید خوانده شوند.
       • Skip → تعداد رکوردهایی که باید رد شوند.
       • IsPagingEnabled → فعال بودن Paging.

     🛠 جریان کار:
     1. یک Specification جدید ساخته می‌شود (مثلاً ProductsByCategorySpec).
     2. Criteria و Includes و OrderBy در آن تعریف می‌شوند.
     3. Repository این Specification را دریافت کرده و Query نهایی را می‌سازد.
     4. EF Core یا ORM دیگر Query را اجرا می‌کند و نتایج برمی‌گرداند.

     📌 نتیجه:
     این اینترفیس پایه‌ی مکانیزم **Specification Pattern** در معماری DDD است
     و تضمین می‌کند که Queryهای پیچیده به صورت ماژولار، قابل ترکیب و تست‌پذیر باشند.
    */

    public interface ISpecification<T>
    {
        // 📌 شرط اصلی فیلتر
        Expression<Func<T, bool>> Criteria { get; }

        // 📌 Includes برای eager loading
        List<Expression<Func<T, object>>> Includes { get; }
        List<Func<IQueryable<T>, IIncludableQueryable<T, object>>> IncludeFunctions { get; }
        List<string> IncludeStrings { get; }

        // 📌 مرتب‌سازی
        Expression<Func<T, object>>? OrderBy { get; }
        Expression<Func<T, object>>? OrderByDescending { get; }
        List<(Expression<Func<T, object>> KeySelector, bool IsDescending)> ThenOrderBy { get; }

        // 📌 Paging
        int Take { get; }
        int Skip { get; }
        bool IsPagingEnabled { get; }
    }
}
