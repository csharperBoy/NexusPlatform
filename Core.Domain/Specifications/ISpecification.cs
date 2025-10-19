using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Specifications
{
    public interface ISpecification<T>
    {
        // شرط اصلی
        Expression<Func<T, bool>> Criteria { get; }

        // Includes ساده (برای مسیرهای مستقیم)
        List<Expression<Func<T, object>>> Includes { get; }

        // Includes پیچیده (با ThenInclude)
        List<Func<IQueryable<T>, IIncludableQueryable<T, object>>> IncludeFunctions { get; }

        // Includes رشته‌ای (برای موارد خاص)
        List<string> IncludeStrings { get; }

        // مرتب‌سازی
        Expression<Func<T, object>> OrderBy { get; }
        Expression<Func<T, object>> OrderByDescending { get; }

        // مرتب‌سازی زنجیره‌ای (ThenOrderBy)
        List<(Expression<Func<T, object>> KeySelector, bool IsDescending)> ThenOrderBy { get; }

        // صفحه‌بندی
        int Take { get; }
        int Skip { get; }
        bool IsPagingEnabled { get; }
    }
}
