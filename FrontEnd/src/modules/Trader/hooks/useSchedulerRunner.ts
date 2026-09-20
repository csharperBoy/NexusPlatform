import { useEffect, useRef } from "react";
import { useScheduleStore } from "../stores/useScheduleStore";
import { useAccountsStore, getTokenStatus } from "../stores/useAccountsStore";
import { useSymbolInfoStore } from "../stores/useSymbolInfoStore";
import { useSymbolsStore } from "../stores/useSymbolsStore";
import { useLoginStore } from "../stores/useLoginStore";
import { fireOrder } from "../utils/fireOrder";
import { parseTargetTime, preciseWait, todayDateKey } from "../utils";
import type { SchedulePlan, ScheduledOrder } from "../models";
import type { UseServerClockResult } from "./useServerClock";

const TICK_MS = 1000;
const MAX_LATE_MS = 5 * 1000;
const PRE_CLOCK_SYNC_MS = 5000;
/** پنجره‌ی همزمانی برای شلیک سفارش‌هایی که زمانشون رسیده */
const FIRE_TOLERANCE_MS = 5;

interface Options {
  clock: UseServerClockResult;
}

export function useSchedulerRunner({ clock }: Options): void {
  const runningRef = useRef(false);
  const clockRef = useRef(clock);
  clockRef.current = clock;

  useEffect(() => {
    const tick = async () => {
      if (runningRef.current) return;
      runningRef.current = true;

      try {
        const s = useScheduleStore.getState();

        if (!s.enabled) {
          if (s.runtime.status !== "idle") s.setStatus("idle", "");
          return;
        }

        const today = todayDateKey();
        if (s.runtime.currentDate !== today) {
          useScheduleStore.getState().resetForDay(today);
          return;
        }

        const todayPlans = s.plans.filter(
          (p) => p.enabled && p.date === today && p.orders.length > 0,
        );

        if (todayPlans.length === 0) {
          const futureCount = s.plans.filter(
            (p) => p.enabled && p.date > today && p.orders.length > 0,
          ).length;
          useScheduleStore
            .getState()
            .setStatus(
              "idle",
              futureCount > 0
                ? `${futureCount} برنامه آینده در انتظار`
                : "هیچ برنامه فعالی برای امروز نیست",
            );
          return;
        }

        for (const plan of todayPlans) {
          await stepPlan(plan, today, clockRef.current);
        }
      } finally {
        runningRef.current = false;
      }
    };

    const id = setInterval(tick, TICK_MS);
    tick();

    const onVis = () => {
      if (document.visibilityState === "visible") tick();
    };
    document.addEventListener("visibilitychange", onVis);

    return () => {
      clearInterval(id);
      document.removeEventListener("visibilitychange", onVis);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);
}

/* ══════════════════════════════════════════════ */
async function stepPlan(
  plan: SchedulePlan,
  today: string,
  clock: UseServerClockResult,
): Promise<void> {
  const store = useScheduleStore.getState();
  const log = (msg: string, type: "ok" | "err" | "info" | "send" = "info") =>
    store.appendPlanLog(plan.id, msg, type);

  let planState = store.getPlanState(plan.id);

  /* ─── ۱. لاگین ─── */
  if (planState.lastLoginDate !== today) {
    const loginAt = parseTargetTime(plan.autoLoginAt, today);
    if (loginAt === null) {
      store.setPlanMessage(plan.id, "زمان لاگین نامعتبره");
      return;
    }

    const now = Date.now();
    if (now < loginAt) {
      const remain = Math.floor((loginAt - now) / 1000);
      store.setPlanMessage(plan.id, `لاگین تا ${remain} ثانیه دیگر`);
      return;
    }
    if (now - loginAt > MAX_LATE_MS) {
      log(`⏭ از زمان لاگین (${plan.autoLoginAt}) گذشته — کنسل`, "err");
      store.setPlanState(plan.id, { lastLoginDate: today });
      return;
    }

    store.setPlanMessage(plan.id, "در حال لاگین...");
    log(`🔄 لاگین همه حساب‌ها`, "info");

    const accounts = useAccountsStore.getState().accounts;
    const needsLogin = accounts.filter(
      (a) =>
        a.username?.trim() &&
        a.password?.trim() &&
        getTokenStatus(a.token).state !== "valid",
    );

    if (needsLogin.length > 0) {
      const login = useLoginStore.getState().loginAccount;
      for (const acc of needsLogin) {
        await login(acc.id, { silent: true });
      }
    }

    const validCount = useAccountsStore
      .getState()
      .accounts.filter((a) => getTokenStatus(a.token).state === "valid").length;

    log(
      `✅ ${validCount}/${accounts.length} حساب معتبر`,
      validCount > 0 ? "ok" : "err",
    );

    if (validCount === 0) {
      store.setPlanMessage(plan.id, "هیچ حسابی لاگین نشد");
      return;
    }

    store.setPlanState(plan.id, { lastLoginDate: today });
    store.setPlanMessage(plan.id, "لاگین شد");
    return;
  }

  /* ─── ۲. رفرش قیمت ─── */
  if (planState.lastRefreshDate !== today) {
    const refreshAt = parseTargetTime(plan.autoRefreshAt, today);
    if (refreshAt === null) {
      store.setPlanMessage(plan.id, "زمان رفرش نامعتبره");
      return;
    }

    const now = Date.now();
    if (now < refreshAt) {
      const remain = Math.floor((refreshAt - now) / 1000);
      store.setPlanMessage(plan.id, `رفرش تا ${remain} ثانیه دیگر`);
      return;
    }
    if (now - refreshAt > MAX_LATE_MS) {
      log(`⏭ از زمان رفرش (${plan.autoRefreshAt}) گذشته — کنسل`, "err");
      store.setPlanState(plan.id, { lastRefreshDate: today });
      return;
    }

    store.setPlanMessage(plan.id, "در حال رفرش...");
    log(`🔄 رفرش قیمت نمادها`, "info");

    const validToken =
      useAccountsStore
        .getState()
        .accounts.find((a) => a.token?.trim())?.token ?? "";
    if (!validToken) {
      store.setPlanMessage(plan.id, "توکن معتبری نیست");
      return;
    }

    const uniqueIsins = Array.from(
      new Set(plan.orders.map((o) => o.symbolIsin).filter((x) => !!x)),
    );
    const ensureInfo = useSymbolInfoStore.getState().ensureInfo;
    let ok = 0;
    for (const isin of uniqueIsins) {
      const info = await ensureInfo(isin, validToken, true);
      if (info) ok++;
    }

    log(
      `✅ ${ok}/${uniqueIsins.length} نماد بروزرسانی شد`,
      ok > 0 ? "ok" : "err",
    );

    store.setPlanState(plan.id, { lastRefreshDate: today });
    store.setPlanMessage(plan.id, "قیمت‌ها آماده");
    return;
  }

  /* ─── ۳. شلیک ─── */
  planState = store.getPlanState(plan.id);

  const pending = plan.orders
    .filter(
      (o) =>
        o.accountId && o.symbolIsin && !planState.firedOrderIds.includes(o.id),
    )
    .map((o) => ({ o, t: parseTargetTime(o.time, today) }))
    .filter((x): x is { o: ScheduledOrder; t: number } => x.t !== null)
    .sort((a, b) => a.t - b.t);

  if (pending.length === 0) {
    store.setPlanMessage(
      plan.id,
      plan.orders.length > 0 ? "تمام — همه ارسال شد" : "سفارشی ندارد",
    );
    return;
  }

  const first = pending[0];
  const now1 = Date.now();
  const lateBy = now1 - first.t;

  /* خیلی دیر شده → کنسل همه */
  if (lateBy > MAX_LATE_MS) {
    for (const { o } of pending) {
      store.markOrderFired(plan.id, o.id);
    }
    log(`⏭ کنسل — از ${first.o.time} بیش از حد گذشته`, "err");
    return;
  }

  /* محاسبه‌ی زمان شلیک به وقت کلاینت */
  const diff = clock.info.diff || 0;
  const clientFire = first.t - diff;
  const nowClient = Date.now();
  const remainToFire = clientFire - nowClient;

  /* هنوز تا شلیک وقت هست */
  if (remainToFire > FIRE_TOLERANCE_MS) {
    /* نزدیک شلیک → سینک دقیقاً یک بار برای هر order */
    if (
      remainToFire < PRE_CLOCK_SYNC_MS &&
      planState.clockSyncedForOrderId !== first.o.id
    ) {
      log(`⏰ سینک متراکم قبل از شلیک...`, "info");
      await clock.sync(5);
      store.setPlanState(plan.id, { clockSyncedForOrderId: first.o.id });
      return; // tick بعدی، clock تازه
    }

    /* خیلی نزدیک → انتظار دقیق */
    if (remainToFire < 2000) {
      /* بعد از sync، دوباره محاسبه کن که now قدیمی نباشه */
      const freshClientFire = first.t - (clock.info.diff || 0);
      if (freshClientFire > Date.now()) {
        await preciseWait(freshClientFire);
      }
      /* بعد از preciseWait، با همون tick ادامه بده و شلیک کن */
    } else {
      store.setPlanMessage(
        plan.id,
        `شلیک بعدی تا ${(remainToFire / 1000).toFixed(1)} ثانیه`,
      );
      return;
    }
  }

  /* الان موقع شلیکه — همه‌ی order‌هایی که زمانشون رسیده رو با هم شلیک کن */
  const nowFire = Date.now();
  const toFire = pending.filter((x) => {
    const cf = x.t - diff;
    return nowFire >= cf - FIRE_TOLERANCE_MS;
  });

  if (toFire.length === 0) {
    /* این حالت نباید پیش بیاد، ولی محض اطمینان */
    store.setPlanMessage(plan.id, "در انتظار...");
    return;
  }

  /* علامت بزن که همه شلیک شدن */
  for (const { o } of toFire) {
    store.markOrderFired(plan.id, o.id);
  }

  store.setPlanMessage(plan.id, `🔥 شلیک ${toFire.length} سفارش`);

  /* همه رو همزمان (بدون await) بفرست */
  await Promise.all(
    toFire.map((x) => fireScheduledOrder(plan, x.o, clock)),
  );
}

/* ══════════════════════════════════════════════ */
async function fireScheduledOrder(
  plan: SchedulePlan,
  o: ScheduledOrder,
  _clock: UseServerClockResult,
): Promise<void> {
  const store = useScheduleStore.getState();
  const log = (msg: string, type: "ok" | "err" | "info" | "send" = "info") =>
    store.appendPlanLog(plan.id, msg, type);

  const account = useAccountsStore.getState().getAccount(o.accountId);
  if (!account) return log(`❌ حساب پیدا نشد`, "err");
  if (!account.token?.trim())
    return log(`❌ [${account.name}] توکن خالیه`, "err");

  const symbol = useSymbolsStore.getState().getSymbol(o.symbolIsin);
  if (!symbol) return log(`❌ نماد پیدا نشد`, "err");

  const info = useSymbolInfoStore.getState().cache[o.symbolIsin];
  if (!info)
    return log(`❌ اطلاعات ${symbol.symbolName} لود نشده`, "err");

  const price = o.side === 0 ? info.highAllowedPrice : info.lowAllowedPrice;
  if (!price || price <= 0)
    return log(`❌ ${symbol.symbolName}: قیمت مجاز موجود نیست`, "err");

  let quantity: number;
  if (o.mode === "quantity") {
    quantity = Number(o.quantity);
  } else {
    const totalValue = Number(o.totalValue);
    quantity = Math.floor(totalValue / (price * (1 + symbol.commission)));
  }

  if (!Number.isFinite(quantity) || quantity <= 0)
    return log(`❌ ${symbol.symbolName}: تعداد نامعتبر`, "err");

  log(
    `📋 ${symbol.symbolName} — قیمت ${price} × ${quantity}`,
    "info",
  );

  await fireOrder({
    token: account.token,
    symbol: { ...symbol, side: o.side },
    price,
    quantity,
    accountName: account.name,
    onLog: log,
  });
}