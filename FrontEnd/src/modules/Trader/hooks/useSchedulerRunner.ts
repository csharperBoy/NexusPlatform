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

/* ══════════════════════════════════════════════
   ثابت‌ها
   ══════════════════════════════════════════════ */
const TICK_MS = 1000;
const MAX_LATE_MS = 5 * 1000;
const PRE_SYNC_MS = 5000;
const GROUP_TOLERANCE_MS = 5;
/** اگه از آخرین سینک کمتر از این مدت گذشته باشه، سینک دوباره انجام نمیشه */
const SYNC_COOLDOWN_MS = 60_000;

/* ══════════════════════════════════════════════
   state مشترک (in-memory)
   ══════════════════════════════════════════════ */
interface PreparedOrder {
  orderId: string;
  accountId: string;
  accountName: string;
  token: string;
  symbol: Symbol;
  price: number;
  quantity: number;
  target: number;
}

const preparedByPlan = new Map<string, PreparedOrder[]>();
const runningPlans = new Set<string>();

/** آخرین باری که سینک متراکم انجام شد (ms) */
let lastSyncAt = 0;

function clearAllState() {
  preparedByPlan.clear();
  runningPlans.clear();
  lastSyncAt = 0;
}

/* ══════════════════════════════════════════════
   ساخت payload برای همه‌ی سفارش‌های یه پلن
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
      errors.push(`نماد ${symbol.symbolName}: زمان نامعتبر (${o.time})`);
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

/* ══════════════════════════════════════════════
   اجرای کامل یه پلن
   ══════════════════════════════════════════════ */
async function runPlan(
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
      store.setPlanMessage(
        plan.id,
        `لاگین تا ${((loginAt - now) / 1000).toFixed(1)}s`,
      );
      await new Promise((r) => setTimeout(r, loginAt - now));
    }

    if (Date.now() - loginAt > MAX_LATE_MS) {
      log(`⏭ از زمان لاگین (${plan.autoLoginAt}) بیش از حد گذشته`, "err");
      store.setPlanState(plan.id, { lastLoginDate: today });
    } else {
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
        .accounts.filter(
          (a) => getTokenStatus(a.token).state === "valid",
        ).length;

      log(
        `✅ ${validCount}/${accounts.length} حساب معتبر`,
        validCount > 0 ? "ok" : "err",
      );

      if (validCount === 0) {
        store.setPlanMessage(plan.id, "هیچ حسابی لاگین نشد");
        return;
      }

      store.setPlanState(plan.id, { lastLoginDate: today });
    }

    planState = store.getPlanState(plan.id);
  }

  /* ─── ۲. رفرش ─── */
  if (planState.lastRefreshDate !== today) {
    const refreshAt = parseTargetTime(plan.autoRefreshAt, today);
    if (refreshAt === null) {
      store.setPlanMessage(plan.id, "زمان رفرش نامعتبره");
      return;
    }

    const now = Date.now();
    if (now < refreshAt) {
      store.setPlanMessage(
        plan.id,
        `رفرش تا ${((refreshAt - now) / 1000).toFixed(1)}s`,
      );
      await new Promise((r) => setTimeout(r, refreshAt - now));
    }

    if (Date.now() - refreshAt > MAX_LATE_MS) {
      log(`⏭ از زمان رفرش (${plan.autoRefreshAt}) بیش از حد گذشته`, "err");
      store.setPlanState(plan.id, { lastRefreshDate: today });
    } else {
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

      const built = await rebuildPrepared(plan, today);
      log(
        `📦 ${built.length}/${plan.orders.length} سفارش آماده شلیک`,
        built.length > 0 ? "ok" : "err",
      );

      store.setPlanState(plan.id, { lastRefreshDate: today });
    }

    planState = store.getPlanState(plan.id);
  }

  /* ─── ۳. آماده‌سازی ─── */
  let prepared = preparedByPlan.get(plan.id);
  if (!prepared || prepared.length === 0) {
    log("📦 prepared خالی بود — بازسازی می‌کنم...", "info");
    prepared = await rebuildPrepared(plan, today);
  }
  if (prepared.length === 0) {
    store.setPlanMessage(plan.id, "هیچ سفارشی آماده نیست");
    return;
  }

  const pending = prepared
    .filter((p) => !planState.firedOrderIds.includes(p.orderId))
    .sort((a, b) => a.target - b.target);

  if (pending.length === 0) {
    store.setPlanMessage(plan.id, "تمام — همه سفارش‌ها ارسال شد");
    return;
  }

  /* ─── ۴. گروه‌بندی ─── */
  const groups: { target: number; items: PreparedOrder[] }[] = [];
  for (const p of pending) {
    const last = groups[groups.length - 1];
    if (last && Math.abs(p.target - last.target) <= GROUP_TOLERANCE_MS) {
      last.items.push(p);
    } else {
      groups.push({ target: p.target, items: [p] });
    }
  }

  log(`🎯 ${groups.length} گروه زمانی`, "info");

  /* ─── ۵. حلقه‌ی fire ─── */
  for (let gi = 0; gi < groups.length; gi++) {
    const group = groups[gi];
    const now = Date.now();

    if (now - group.target > MAX_LATE_MS) {
      for (const p of group.items) store.markOrderFired(plan.id, p.orderId);
      log(
        `⏭ گروه ${gi + 1} کنسل — از ${logTimestamp(new Date(group.target))} گذشته`,
        "err",
      );
      continue;
    }

    let diff = clock.info.diff || 0;
    let clientFire = group.target - diff;
    let remainToFire = clientFire - now;

    /* ✅ سینک ۵ ثانیه قبل — ولی فقط اگه از سینک قبلی به‌اندازه‌ی کافی گذشته باشه */
    if (remainToFire > 0 && remainToFire < PRE_SYNC_MS) {
      const sinceLastSync = Date.now() - lastSyncAt;

      if (lastSyncAt > 0 && sinceLastSync < SYNC_COOLDOWN_MS) {
        /* سینک تازه انجام شده — استفاده مجدد */
        log(
          `⏭ سینک لازم نیست — ${Math.round(sinceLastSync / 1000)}s پیش سینک شد (diff=${Math.round(diff)}ms)`,
          "info",
        );
      } else {
        log(
          `⏰ سینک متراکم (گروه ${gi + 1}/${groups.length} — ${group.items.length} سفارش)`,
          "info",
        );
        const freshDiff = await clock.sync(5);
        lastSyncAt = Date.now();
        if (typeof freshDiff === "number" && Number.isFinite(freshDiff)) {
          diff = freshDiff;
          log(`✅ diff تازه: ${Math.round(freshDiff)}ms`, "info");
        } else {
          log(`⚠️ سینک ناموفق — diff قدیمی (${Math.round(diff)}ms)`, "err");
        }
        clientFire = group.target - diff;
        remainToFire = clientFire - Date.now();
      }
    }

    if (remainToFire > 0) {
      store.setPlanMessage(
        plan.id,
        `گروه ${gi + 1}/${groups.length} — شلیک ${group.items.length} تا ${(remainToFire / 1000).toFixed(1)}s`,
      );
      await preciseWait(clientFire);
    } else if (remainToFire < -MAX_LATE_MS) {
      for (const p of group.items) store.markOrderFired(plan.id, p.orderId);
      log(
        `⏭ گروه ${gi + 1} کنسل — بعد از سینک دیر شد (${Math.round(-remainToFire)}ms)`,
        "err",
      );
      continue;
    }

    const fireAt = logTimestamp();
    store.setPlanMessage(
      plan.id,
      `🔥 شلیک ${group.items.length} سفارش در ${fireAt}`,
    );

    for (const p of group.items) {
      store.markOrderFired(plan.id, p.orderId);
    }

    for (const p of group.items) {
      fireOrder({
        token: p.token,
        symbol: p.symbol,
        price: p.price,
        quantity: p.quantity,
        accountName: p.accountName,
        onLog: log,
      }).catch((e) =>
        log(
          `💥 ${p.symbol.symbolName} — ${e instanceof Error ? e.message : "خطا"}`,
          "err",
        ),
      );
    }
  }

  store.setPlanMessage(plan.id, "تمام — همه سفارش‌ها ارسال شد");
}

/* ══════════════════════════════════════════════ */
interface Options {
  clock: UseServerClockResult;
}

export function useSchedulerRunner({ clock }: Options): void {
  const clockRef = useRef(clock);
  clockRef.current = clock;

  useEffect(() => {
    const tick = () => {
      const s = useScheduleStore.getState();

      if (!s.enabled) {
        if (s.runtime.status !== "idle") s.setStatus("idle", "");
        return;
      }

      const today = todayDateKey();

      if (s.runtime.currentDate !== today) {
        clearAllState();
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
        s.setStatus(
          "idle",
          futureCount > 0
            ? `${futureCount} برنامه آینده در انتظار`
            : "هیچ برنامه فعالی برای امروز نیست",
        );
        return;
      }

      for (const plan of todayPlans) {
        if (runningPlans.has(plan.id)) continue;

        const loginAt = parseTargetTime(plan.autoLoginAt, today);
        if (loginAt === null) continue;
        if (Date.now() < loginAt) continue;

        runningPlans.add(plan.id);
        s.setStatus("waiting", `اجرای «${plan.name}»`);

        runPlan(plan, today, clockRef.current)
          .catch((e) => {
            useScheduleStore
              .getState()
              .appendPlanLog(
                plan.id,
                `💥 خطای غیرمنتظره: ${e instanceof Error ? e.message : "خطا"}`,
                "err",
              );
          })
          .finally(() => {
            runningPlans.delete(plan.id);
          });
      }

      const stillRunning = todayPlans.filter((p) => runningPlans.has(p.id));
      if (stillRunning.length > 0) {
        s.setStatus("waiting", `${stillRunning.length} پلن در حال اجرا`);
      } else {
        const allDone = todayPlans.every((p) => {
          const st = s.getPlanState(p.id);
          return st.firedOrderIds.length >= p.orders.length;
        });
        if (allDone) {
          s.setStatus("done", "همه برنامه‌ها تمام");
        } else {
          s.setStatus("idle", "در انتظار زمان اجرا");
        }
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
export function invalidatePrepared(planId: string) {
  preparedByPlan.delete(planId);
}