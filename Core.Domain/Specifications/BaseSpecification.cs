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
     📌 BaseSpecification<T>
     ------------------------
     این کلاس یک **Base Class** برای پیاده‌سازی الگوی Specification در معماری DDD است.
     هدف آن فراهم کردن یک اسکلت استاندارد برای تعریف Queryهای پیچیده و قابل ترکیب
     بدون وابستگی مستقیم به EF یا دیتابیس می‌باشد.

     ✅ نکات کلیدی:
     - Criteria → شرط اصلی فیلتر (Expression<Func<T, bool>>).
       • پیش‌فرض: همیشه true (یعنی بدون فیلتر).
       • در سازنده می‌توان شرط اختصاصی تعریف کرد.

     - Includes → لیست Navigation Properties برای eager loading.
     - IncludeFunctions → توابعی برای Includeهای پیچیده (با ThenInclude).
     - IncludeStrings → لیست Include به صورت رشته‌ای (برای سناریوهای خاص).

     - OrderBy / OrderByDescending → مرتب‌سازی اصلی.
     - ThenOrderBy → مرتب‌سازی‌های اضافی (چند سطحی).

     - Paging:
       • Skip → تعداد رکوردهایی که باید رد شوند.
       • Take → تعداد رکوردهایی که باید خوانده شوند.
       • IsPagingEnabled → فعال بودن Paging.

     - متدهای محافظت‌شده (Protected):
       • AddInclude → افزودن Include به لیست.
       • ApplyPaging → فعال‌سازی Paging.
       • ApplyOrderBy / ApplyOrderByDescending → مرتب‌سازی اصلی.
       • ApplyThenOrderBy → مرتب‌سازی‌های چند سطحی.

     🛠 جریان کار:
     1. یک Specification جدید ساخته می‌شود (مثلاً ProductByCategorySpec).
     2. در سازنده‌ی آن Criteria و Includes و OrderBy تعریف می‌شوند.
     3. Repository این Specification را دریافت کرده و Query نهایی را می‌سازد.
     4. EF Core یا ORM دیگر Query را اجرا می‌کند و نتایج برمی‌گرداند.

     📌 نتیجه:
     این کلاس پایه‌ی مکانیزم **Specification Pattern** در معماری DDD است
     و تضمین می‌کند که Queryهای پیچیده به صورت ماژولار، قابل ترکیب و تست‌پذیر باشند.
    */

    public abstract class BaseSpecification<T> : ISpecification<T>
    {
        protected BaseSpecification(Expression<Func<T, bool>> criteria)
        {
            Criteria = criteria;
        }

        protected BaseSpecification() { }

        // 📌 شرط اصلی فیلتر
        public Expression<Func<T, bool>> Criteria { get; } = _ => true;

        // 📌 Includes برای eager loading
        public List<Expression<Func<T, object>>> Includes { get; } = new();
        public List<Func<IQueryable<T>, IIncludableQueryable<T, object>>> IncludeFunctions { get; } = new();
        public List<string> IncludeStrings { get; } = new();

        // 📌 مرتب‌سازی
        public Expression<Func<T, object>>? OrderBy { get; private set; }
        public Expression<Func<T, object>>? OrderByDescending { get; private set; }
        public List<(Expression<Func<T, object>> KeySelector, bool IsDescending)> ThenOrderBy { get; } = new();

        // 📌 Paging
        public int Take { get; private set; }
        public int Skip { get; private set; }
        public bool IsPagingEnabled { get; private set; } = false;

        // 📌 متدهای کمکی برای ساخت Specification
        protected virtual void AddInclude(Expression<Func<T, object>> includeExpression) => Includes.Add(includeExpression);
        protected virtual void AddInclude(Func<IQueryable<T>, IIncludableQueryable<T, object>> includeFunction) => IncludeFunctions.Add(includeFunction);
        protected virtual void AddInclude(string includeString) => IncludeStrings.Add(includeString);

        protected virtual void ApplyPaging(int skip, int take)
        {
            Skip = skip;
            Take = take;
            IsPagingEnabled = true;
        }

        protected virtual void ApplyOrderBy(Expression<Func<T, object>> orderByExpression) => OrderBy = orderByExpression;
        protected virtual void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression) => OrderByDescending = orderByDescendingExpression;
        protected virtual void ApplyThenOrderBy(Expression<Func<T, object>> thenOrderByExpression, bool isDescending = false) => ThenOrderBy.Add((thenOrderByExpression, isDescending));
    }
}
