namespace Core.Shared.Results
{
    /*
     📌 Result & Result<T>
     ---------------------
     این دو record وظیفه‌ی ارائه‌ی یک مدل استاندارد برای بازگرداندن نتیجه‌ی عملیات را بر عهده دارند.
     هدف آن ساده‌سازی مدیریت موفقیت/شکست عملیات و جلوگیری از استفاده‌ی پراکنده از Exceptionها یا مقادیر خاص است.

     ✅ نکات کلیدی:
     - Result:
       • نوع پایه برای نتایج بدون داده.
       • ویژگی‌ها:
         - Succeeded → نشان‌دهنده‌ی موفقیت یا شکست عملیات.
         - Error → پیام خطا در صورت شکست.
       • متدهای کمکی:
         - Ok() → ایجاد نتیجه‌ی موفق.
         - Fail(error) → ایجاد نتیجه‌ی ناموفق با پیام خطا.

     - Result<T>:
       • نوع Generic برای نتایج همراه با داده.
       • ارث‌بری از Result.
       • ویژگی‌ها:
         - Data → داده‌ی بازگردانده‌شده در صورت موفقیت.
         - Error → پیام خطا در صورت شکست.
       • متدهای کمکی:
         - Ok(data) → ایجاد نتیجه‌ی موفق همراه با داده.
         - Fail(error) → ایجاد نتیجه‌ی ناموفق همراه با پیام خطا.

     🛠 جریان کار:
     1. سرویس‌ها یا متدها به جای پرتاب Exception یا بازگرداندن null، از Result استفاده می‌کنند.
     2. مصرف‌کننده می‌تواند بررسی کند:
        if (result.Succeeded) { ... } else { ... }
     3. در صورت موفقیت، داده در Result<T>.Data موجود است.
     4. در صورت شکست، پیام خطا در Result.Error یا Result<T>.Error موجود است.

     📌 نتیجه:
     این کلاس پایه‌ی مکانیزم **Operation Result Pattern** در معماری ماژولار است
     و تضمین می‌کند که مدیریت موفقیت/شکست عملیات به صورت استاندارد، خوانا و قابل تست انجام شود.
    */
    #region For Single Commands

    public record Result(bool Succeeded, string? Error = null)
    {
        public static Result Ok() => new(true);
        public static Result Fail(string error) => new(false, error);
    }

    public record Result<T>(bool Succeeded, T? Data = default, string? Error = null) : Result(Succeeded, Error)
    {
        public static Result<T> Ok(T data) => new(true, data);
        public static Result<T> Fail(string error) => new(false, default, error);
    }

    #endregion
    #region For Batch Commands

    public record BatchResult<T>(
     bool Succeeded,
     List<string>? SuccessMessages = null,
     List<string>? Errors = null,
     T? Data = default)
    {
        // ساخت یک نتیجهٔ موفق با داده و پیام‌های اختیاری
        public static BatchResult<T> Ok(T data, List<string>? successMessages = null)
            => new(true, successMessages ?? new(), null, data);

        // ساخت یک نتیجهٔ ناموفق با لیست خطاها (و دادهٔ اختیاری)
        public static BatchResult<T> Fail(List<string> errors, T? data = default, List<string>? successMessages = null)
            => new(false, successMessages, errors, data);

        // متد کمکی برای خطای تکی
        public static BatchResult<T> Fail(string error, T? data = default)
            => new(false, null, new List<string> { error }, data);
    }

    // همچنین برای مواقعی که داده‌ای نداریم، می‌توان یک نوع غیرجنریک تعریف کرد:
    public record BatchResult : BatchResult<object?>
    {
        public BatchResult(bool succeeded, List<string>? successMessages = null, List<string>? errors = null)
            : base(succeeded, successMessages, errors, null) { }

        public static BatchResult Ok(List<string>? successMessages = null)
            => new(true, successMessages);

        public static BatchResult Fail(List<string> errors, List<string>? successMessages = null)
            => new(false, successMessages, errors);

        public static BatchResult Fail(string error)
            => new(false, null, new List<string> { error });
    }

    #endregion
}