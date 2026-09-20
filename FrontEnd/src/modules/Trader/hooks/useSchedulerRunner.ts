import { useEffect, useRef } from "react";
import { useScheduleStore } from "../stores/useScheduleStore";
import { useAccountsStore, getTokenStatus } from "../stores/useAccountsStore";
import { useSymbolInfoStore } from "../stores/useSymbolInfoStore";
import { useSymbolsStore } from "../stores/useSymbolsStore";
import { useLoginStore } from "../stores/useLoginStore";
import { fireOrder } from "../utils/fireOrder";
import {
  parseTargetTime,
  preciseWait,
  todayDateKey,
  logTimestamp,
} from "../utils";
import type { SchedulePlan, Symbol } from "../models";
import type { UseServerClockResult } from "./useServerClock";

const TICK_MS = 1000;
const MAX_LATE_MS = 5 * 1000;
/** پنجره‌ی سینک قبل از شلیک (ms) */
const PRE_SYNC_MS = 3000;
/** دو سفارش با اختلاف کمتر از این، هم‌گروه حساب می‌شن */
const GROUP_TOLERANCE_MS = 5;

/* ══════════════════════════════════════════════
   state مشترک بین tick ها (in-memory)
   ══════════════════════════════════════════════ */
interface PreparedOrder {
  orderId: string;
  accountId: string;
  accountName: string;
  token: string;
  symbol: Symbol;
  price: number;
  quantity: number;
  /** timestamp دقیق هدف (ms) */
  target: number;
}

/** key: planId → لیست سفارش‌های آماده */
const preparedByPlan = new Map<string, PreparedOrder[]>();

/** targetهایی که برای اون‌ها clock sync انجام شده */
const syncedTargets = new Set<number>();

function resetPreparedState() {
  preparedByPlan.clear();
  syncedTargets.clear();
}
/* ══════════════════════════════════════════════
   ساخت payload برای همه‌ی سفارش‌های یه پلن
   (بدون fetch — فقط از cache symbol-info)
   ══════════════════════════════════════════════ */
async function rebuildPrepared(
  plan: SchedulePlan,
  today: string,
): Promise<PreparedOrder[]> {
  const store = useScheduleStore.getState();
  const log = (msg: string, type: "ok" | "err" | "info" | "send" = "info") =>
    store.appendPlanLog(plan.id, msg, type);

  const prepared: PreparedOrder[] = [];
  const errors: string[] = [];

  for (const o of plan.orders) {
    const account = useAccountsStore.getState().getAccount(o.accountId);
    if (!account || !account.token?.trim()) {
      errors.push(`حساب خالی/بدون توکن`);
      continue;
    }
    const symbol = useSymbolsStore.getState().getSymbol(o.symbolIsin);
    if (!symbol) {
      errors.push(`نماد ${o.symbolIsin}: پیدا نشد`);
      continue;
    }
    const info = useSymbolInfoStore.getState().cache[o.symbolIsin];
    if (!info) {
      errors.push(`نماد ${symbol.symbolName}: اطلاعات لود نشده`);
      continue;
    }
    const price = o.side === 0 ? info.highAllowedPrice : info.lowAllowedPrice;
    if (!price || price <= 0) {
      errors.push(`نماد ${symbol.symbolName}: قیمت مجاز موجود نیست`);
      continue;
    }
    let quantity: number;
    if (o.mode === "quantity") {
      quantity = Number(o.quantity);
    } else {
      quantity = Math.floor(
        Number(o.totalValue) / (price * (1 + symbol.commission)),
      );
    }
    if (!Number.isFinite(quantity) || quantity <= 0) {
      errors.push(`نماد ${symbol.symbolName}: تعداد نامعتبر`);
      continue;
    }
    const target = parseTargetTime(o.time, today);
    if (target === null) {
      errors.push(`نماد ${symbol.symbolName}: زمان نامعتبر`);
      continue;
    }
    prepared.push({
      orderId: o.id,
      accountId: account.id,
      accountName: account.name,
      token: account.token,
      symbol: { ...symbol, side: o.side },
      price,
      quantity,
      target,
    });
  }

  preparedByPlan.set(plan.id, prepared);
  for (const err of errors) log(`⚠️ ${err}`, "err");
  return prepared;
}
/* ══════════════════════════════════════════════ */
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

        /* ─── روز عوض شده؟ همه‌چیز رو ریست کن ─── */
        if (s.runtime.currentDate !== today) {
          resetPreparedState();
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
/* ══════════════════════════════════════════════
   هر پلن، یه قدم جلو میره
   ══════════════════════════════════════════════ */
async function stepPlan(
  plan: SchedulePlan,
  today: string,
  clock: UseServerClockResult,
): Promise<void> {
  const store = useScheduleStore.getState();
  const log = (msg: string, type: "ok" | "err" | "info" | "send" = "info") =>
    store.appendPlanLog(plan.id, msg, type);

  let planState = store.getPlanState(plan.id);

  /* ═══ ۱. لاگین ═══ */
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

  /* ═══ ۲. رفرش + ساخت payload همه سفارش‌ها ═══ */
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

    store.setPlanMessage(plan.id, "در حال رفرش قیمت‌ها...");
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

    /* ─── ساخت Payload ─── */
    const built = await rebuildPrepared(plan, today);
    log(
      `📦 ${built.length}/${plan.orders.length} سفارش آماده شلیک`,
      built.length > 0 ? "ok" : "err",
    );

    store.setPlanState(plan.id, { lastRefreshDate: today });
    store.setPlanMessage(plan.id, `${built.length} سفارش آماده`);
    return;
  }

  /* ═══ ۳. شلیک ═══ */
  /* ✅ safety net: اگه prepared خالی بود (مثلاً بعد از reload)، همین‌جا بازسازی کن */
  let prepared = preparedByPlan.get(plan.id);
  if (!prepared || prepared.length === 0) {
    const hasPending = plan.orders.some(
      (o) => !planState.firedOrderIds.includes(o.id),
    );
    if (!hasPending) {
      store.setPlanMessage(plan.id, "تمام — همه سفارش‌ها ارسال شد");
      return;
    }
    log("📦 prepared خالی بود — بازسازی می‌کنم...", "info");
    const rebuilt = await rebuildPrepared(plan, today);
    if (rebuilt.length === 0) {
      store.setPlanMessage(plan.id, "بازسازی ناموفق");
      return;
    }
    prepared = rebuilt;
  }

  /* از اینجا prepared قطعاً آرایه‌ی غیرخالیه */
  planState = store.getPlanState(plan.id);
  const pending = prepared
    .filter((p) => !planState.firedOrderIds.includes(p.orderId))
    .sort((a, b) => a.target - b.target);

  if (pending.length === 0) {
    store.setPlanMessage(plan.id, "تمام — همه سفارش‌ها ارسال شد");
    return;
  }

  /* ─── گروه‌بندی بر اساس زمان هدف ─── */
  const groups: { target: number; items: PreparedOrder[] }[] = [];
  for (const p of pending) {
    const last = groups[groups.length - 1];
    if (last && Math.abs(p.target - last.target) <= GROUP_TOLERANCE_MS) {
      last.items.push(p);
    } else {
      groups.push({ target: p.target, items: [p] });
    }
  }

  const group = groups[0];
  if (!group) {
    store.setPlanMessage(plan.id, "گروهی برای شلیک نیست");
    return;
  }

  const now = Date.now();

  /* خیلی دیر شده → کنسل کل گروه */
  if (now - group.target > MAX_LATE_MS) {
    for (const p of group.items) store.markOrderFired(plan.id, p.orderId);
    log(
      `⏭ گروه کنسل — از ${logTimestamp(new Date(group.target))} بیش از حد گذشته`,
      "err",
    );
    return;
  }

  const diff = clock.info.diff || 0;
  const clientFire = group.target - diff;
  const remainToFire = clientFire - now;

  /* ─── در انتظار شلیک ─── */
  if (remainToFire > 5) {
    /* سینک یک‌باره ۳ ثانیه قبل از هر گروه */
    if (remainToFire < PRE_SYNC_MS && !syncedTargets.has(group.target)) {
      log(
        `⏰ سینک متراکم (${group.items.length} سفارش — target=${logTimestamp(new Date(group.target))})`,
        "info",
      );
      await clock.sync(5);
      syncedTargets.add(group.target);
      return; // tick بعدی با diff تازه
    }

    store.setPlanMessage(
      plan.id,
      `شلیک ${group.items.length} سفارش — تا ${(remainToFire / 1000).toFixed(1)}s`,
    );

    /* نزدیک شلیک → preciseWait */
    if (remainToFire < 2000) {
      const freshClientFire = group.target - (clock.info.diff || 0);
      if (freshClientFire > Date.now()) {
        await preciseWait(freshClientFire);
      }
      /* بعد از preciseWait، ادامه بده تا شلیک شه */
    } else {
      return;
    }
  }

  /* ─── شلیک همزمان کل گروه ─── */
  const fireAt = logTimestamp(new Date());
  store.setPlanMessage(
    plan.id,
    `🔥 شلیک ${group.items.length} سفارش در ${fireAt}`,
  );

  for (const p of group.items) {
    store.markOrderFired(plan.id, p.orderId);
  }

  await Promise.all(
    group.items.map((p) =>
      fireOrder({
        token: p.token,
        symbol: p.symbol,
        price: p.price,
        quantity: p.quantity,
        accountName: p.accountName,
        onLog: log,
      }),
    ),
  );
}