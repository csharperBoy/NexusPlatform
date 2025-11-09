using Core.Application.Abstractions.Auditing;
using Core.Application.Abstractions.Security;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Core.Application.Behaviors
{
    /*
     📌 AuditBehavior<TRequest, TResponse>
     -------------------------------------
     این کلاس یک **Pipeline Behavior** در MediatR است که وظیفه‌اش ثبت عملیات‌های
     (Auditing) مربوط به هر Request/Command/Query می‌باشد.

     ✅ نکات کلیدی:
     - از IPipelineBehavior<TRequest, TResponse> ارث‌بری می‌کند:
       → این الگو به ما اجازه می‌دهد قبل یا بعد از اجرای Handler منطق اضافی اجرا کنیم.
       → در اینجا بعد از اجرای Handler، عملیات لاگ‌گذاری انجام می‌شود.

     - وابستگی‌ها:
       1. IAuditService → سرویس ثبت لاگ (در دیتابیس یا سیستم لاگ).
       2. ICurrentUserService → اطلاعات کاربر فعلی (برای ثبت UserId).

     - متد Handle:
       1. ابتدا next() فراخوانی می‌شود تا Handler اصلی اجرا شود.
       2. سپس AuditService فراخوانی می‌شود تا اطلاعات عملیات ثبت شود:
          - action → نام Request (مثلاً SampleApiCommand).
          - entityName → نوع Request.
          - entityId → شناسه موجودیت (اینجا به صورت Guid جدید تولید شده، ولی بهتر است از Request استخراج شود).
          - userId → شناسه کاربر فعلی.
          - changes → خود Request (برای ثبت جزئیات تغییرات).

     🛠 جریان کار:
     1. کاربر یک Command یا Query ارسال می‌کند.
     2. MediatR آن را به Handler مربوطه می‌فرستد.
     3. AuditBehavior بعد از اجرای Handler وارد عمل می‌شود.
     4. عملیات در AuditService ثبت می‌شود (مثلاً در جدول AuditLogs).
     5. پاسخ اصلی به کاربر برگردانده می‌شود.

     📌 نتیجه:
     این کلاس تضمین می‌کند که همه‌ی درخواست‌ها به صورت خودکار لاگ شوند،
     بدون اینکه نیاز باشد در هر Handler به صورت دستی کد لاگ‌گذاری نوشته شود.
     این کار باعث رعایت اصل **Cross-Cutting Concerns** و ساده‌سازی Handlerها می‌شود.
    */

    public class AuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IAuditService _auditService;
        private readonly ICurrentUserService _currentUser;

        public AuditBehavior(IAuditService auditService, ICurrentUserService currentUser)
        {
            _auditService = auditService;
            _currentUser = currentUser;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            // 📌 اجرای Handler اصلی
            var response = await next();

            // 📌 ثبت عملیات در سرویس Audit
            await _auditService.LogAsync(
                action: typeof(TRequest).Name,
                entityName: request.GetType().Name,
                entityId: Guid.NewGuid().ToString(), // بهتر است از request استخراج شود
                userId: _currentUser.UserId,
                changes: request);

            return response;
        }
    }
}
