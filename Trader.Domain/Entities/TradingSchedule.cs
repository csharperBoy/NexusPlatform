using Core.Domain.Common.EntityProperties;

namespace Trader.Domain.Entities;

/// <summary>
/// برنامه‌ی زمانی معاملاتی یک گروه از Instrumentها.
/// Reference Data — تغییر آن روی همه‌ی Instrumentهای متصل اثر می‌گذارد.
/// زمان‌ها به وقت تهران (Local Tehran) ذخیره می‌شوند.
/// </summary>
public class TradingSchedule : BaseEntity
{
    /// <summary>کد یکتا — مثلاً "TSE-REGULAR", "TSE-AYAR".</summary>
    public string Code { get; private set; } = default!;

    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }

    /// <summary>شروع پیش‌گشایش (مثلاً 08:45).</summary>
    public TimeOnly PreMarketStart { get; private set; }

    /// <summary>شروع جلسه‌ی اصلی (مثلاً 09:00).</summary>
    public TimeOnly SessionStart { get; private set; }

    /// <summary>پایان جلسه‌ی اصلی (مثلاً 12:30).</summary>
    public TimeOnly SessionEnd { get; private set; }

    /// <summary>شروع جلسه‌ی دوم (اختیاری — برای نمادهایی که دو جلسه دارند).</summary>
    public TimeOnly? AfternoonStart { get; private set; }

    /// <summary>پایان جلسه‌ی دوم (اختیاری).</summary>
    public TimeOnly? AfternoonEnd { get; private set; }

    /// <summary>پایان جلسه‌ی After-Hours (اختیاری — مثلاً عیار تا 18).</summary>
    public TimeOnly? AfterHoursEnd { get; private set; }

    public bool IsActive { get; private set; }

    private TradingSchedule() { } // EF

    public static TradingSchedule Create(
        string code,
        string name,
        string? description,
        TimeOnly preMarketStart,
        TimeOnly sessionStart,
        TimeOnly sessionEnd,
        TimeOnly? afternoonStart = null,
        TimeOnly? afternoonEnd = null,
        TimeOnly? afterHoursEnd = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required.", nameof(code));
        if (sessionEnd <= sessionStart)
            throw new InvalidOperationException(
                "SessionEnd must be after SessionStart.");

        return new TradingSchedule
        {
            Code = code.Trim(),
            Name = name.Trim(),
            Description = description?.Trim(),
            PreMarketStart = preMarketStart,
            SessionStart = sessionStart,
            SessionEnd = sessionEnd,
            AfternoonStart = afternoonStart,
            AfternoonEnd = afternoonEnd,
            AfterHoursEnd = afterHoursEnd,
            IsActive = true
        };
    }

    public void Update(
        string name,
        string? description,
        TimeOnly preMarketStart,
        TimeOnly sessionStart,
        TimeOnly sessionEnd,
        TimeOnly? afternoonStart,
        TimeOnly? afternoonEnd,
        TimeOnly? afterHoursEnd)
    {
        if (sessionEnd <= sessionStart)
            throw new InvalidOperationException("SessionEnd must be after SessionStart.");

        Name = name.Trim();
        Description = description?.Trim();
        PreMarketStart = preMarketStart;
        SessionStart = sessionStart;
        SessionEnd = sessionEnd;
        AfternoonStart = afternoonStart;
        AfternoonEnd = afternoonEnd;
        AfterHoursEnd = afterHoursEnd;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}