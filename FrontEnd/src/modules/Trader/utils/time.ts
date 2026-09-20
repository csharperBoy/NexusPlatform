/** فرمت "9/18/2026, 10:30:10 AM" که EasyTrader میخواد */
export function formatEasyTraderDateTime(d: Date = new Date()): string {
  const h24 = d.getHours();
  const h12 = h24 % 12 || 12;
  const ampm = h24 < 12 ? "AM" : "PM";
  const mm = String(d.getMinutes()).padStart(2, "0");
  const ss = String(d.getSeconds()).padStart(2, "0");
  return `${d.getMonth() + 1}/${d.getDate()}/${d.getFullYear()}, ${h12}:${mm}:${ss} ${ampm}`;
}

/**
 * "HH:MM:SS.mmm" → timestamp
 * @param hms زمان
 * @param dateStr تاریخ به فرمت YYYY-MM-DD (اختیاری — پیش‌فرض: امروز)
 */
export function parseTargetTime(hms: string, dateStr?: string): number | null {
  const m = hms.trim().match(/^(\d{1,2}):(\d{2})(?::(\d{2})(?:\.(\d{1,3}))?)?$/);
  if (!m) return null;
  const [, h, mi, s = "0", ms = "0"] = m;

  let baseDate: Date;
  if (dateStr) {
    const dm = dateStr.match(/^(\d{4})-(\d{2})-(\d{2})$/);
    if (!dm) return null;
    baseDate = new Date(Number(dm[1]), Number(dm[2]) - 1, Number(dm[3]));
  } else {
    baseDate = new Date();
  }

  return new Date(
    baseDate.getFullYear(),
    baseDate.getMonth(),
    baseDate.getDate(),
    Number(h),
    Number(mi),
    Number(s),
    Number(ms.padEnd(3, "0")),
  ).getTime();
}

/** "YYYY-MM-DD" امروز به وقت محلی */
export function todayDateKey(): string {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;
}

/** "YYYY-MM-DD" فردا */
export function tomorrowDateKey(): string {
  const d = new Date();
  d.setDate(d.getDate() + 1);
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;
}

/** "HH:MM:SS.mmm" با میلی‌ثانیه برای لاگ */
export function logTimestamp(d: Date = new Date()): string {
  const pad = (n: number, len = 2) => String(n).padStart(len, "0");
  return (
    `${pad(d.getHours())}:${pad(d.getMinutes())}:${pad(d.getSeconds())}.` +
    `${pad(d.getMilliseconds(), 3)}`
  );
}

/** انتظار دقیق تا یک timestamp مشخص (به وقت کلاینت) */
export function preciseWait(
  targetMs: number,
  onTick?: (remainingMs: number) => void,
): Promise<void> {
  return new Promise((resolve) => {
    const tick = () => {
      const remaining = targetMs - Date.now();
      onTick?.(remaining);
      if (remaining <= 0) return resolve();
      if (remaining > 100) setTimeout(tick, remaining - 50);
      else if (remaining > 15) setTimeout(tick, 1);
      else {
        while (Date.now() < targetMs) {
          /* busy-wait */
        }
        resolve();
      }
    };
    tick();
  });
}