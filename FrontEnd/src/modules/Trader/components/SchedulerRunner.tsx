import { useAccountsStore } from "../stores";
import { useServerClock, useSchedulerRunner } from "../hooks";

/**
 * کامپوننت بدون UI که موتور زمان‌بند رو توی Layout mount می‌کنه.
 * یه instance از useServerClock می‌سازه (بر اساس اولین توکن معتبر) و
 * به runner پاس می‌ده.
 */
export function SchedulerRunner() {
  const firstValidToken = useAccountsStore(
    (s) => s.accounts.find((a) => a.token?.trim())?.token ?? "",
  );

  const clock = useServerClock(firstValidToken);
  useSchedulerRunner({ clock });

  return null;
}