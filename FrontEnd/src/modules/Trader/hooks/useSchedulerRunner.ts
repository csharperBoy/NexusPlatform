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
import { serverClockRef } from "./useServerClock";
import type { SchedulePlan, Symbol } from "../models";
import type { UseServerClockResult } from "./useServerClock";

/* ══════════════════════════════════════════════
   ثابت‌ها
   ══════════════════════════════════════════════ */
const TICK_MS = 1000;
const MAX_LATE_MS = 5 * 1000;
const GROUP_TOLERANCE_MS = 5;
/** پنجره‌ی presync: از این مدت قبل از شلیک، presync شروع میشه */
const PRESYNC_AT_MS = 2500;
/** حداقل فاصله بین دو presync متوالی */
const PRESYNC_COOLDOWN_MS = 3000;

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
/** timestamp آخرین presync (module-level، مشترک بین همه‌ی پلن‌ها) */
let lastPresyncAt = 0;

function clearAllState() {
  preparedByPlan.clear();
  runningPlans.clear();
  lastPresyncAt = 0;
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

  /* ─── ۲. رفرش + ساخت payload ─── */
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

  /* ─── ۵. حلقه‌ی fire — با presync غیربلاک ─── */
  for (let gi = 0; gi < groups.length; gi++) {
    const group = groups[gi];
    const now = Date.now();

    /* خیلی دیر شده → کنسل گروه */
    if (now - group.target > MAX_LATE_MS) {
      for (const p of group.items) store.markOrderFired(plan.id, p.orderId);
      log(
        `⏭ گروه ${gi + 1} کنسل — از ${logTimestamp(new Date(group.target))} گذشته`,
        "err",
      );
      continue;
    }

    /* ✅ snapshot از diff فعلی — هر بار از ref تازه خونده میشه */
    const diff = serverClockRef.diff;
    const clientFire = group.target - diff;
    const remainToFire = clientFire - Date.now();

    /* ═══ presync غیربلاک ۲.۵ ثانیه قبل ═══ */
    /* اگه نزدیک شلیکیم و از آخرین presync بیش از ۳ ثانیه گذشته، یه presync
       جدید شروع کن — ولی منتظرش نمون. مقدار جدید برای گروه‌های بعدی استفاده میشه */
    if (remainToFire > 0 && remainToFire < PRESYNC_AT_MS) {
      const sinceLast = Date.now() - lastPresyncAt;
      if (sinceLast > PRESYNC_COOLDOWN_MS) {
        lastPresyncAt = Date.now();
        log(
          `⏰ presync غیربلاک (قبل از گروه ${gi + 1} — برای گروه‌های بعدی)`,
          "info",
        );
        /* fire-and-forget — بدون await */
        void clock.sync(3).catch(() => {
          /* خطا رو نادیده بگیر — مقدار قبلی همچنان معتبره */
        });
      }
    }

    /* ═══ preciseWait با snapshot ═══ */
    if (clientFire > Date.now()) {
      store.setPlanMessage(
        plan.id,
        `گروه ${gi + 1}/${groups.length} — ${group.items.length} سفارش تا ${((clientFire - Date.now()) / 1000).toFixed(2)}s (diff=${Math.round(diff)}ms)`,
      );
      await preciseWait(clientFire);
    }

    if (clientFire < Date.now() - MAX_LATE_MS) {
      for (const p of group.items) store.markOrderFired(plan.id, p.orderId);
      log(
        `⏭ گروه ${gi + 1} کنسل — بیش از حد دیر شد`,
        "err",
      );
      continue;
    }

    /* ═══ fire فوری — قبل از هر log/setState ═══ */
    const sendWallMs = Date.now();
    const fireLogStr = logTimestamp(new Date(sendWallMs));

    for (const p of group.items) {
      void fireOrder({
        token: p.token,
        symbol: p.symbol,
        price: p.price,
        quantity: p.quantity,
        accountName: p.accountName,
        onLog: log,
      });
    }

    /* حالا log و state (fetch در پروازه) */
    log(
      `🔥 گروه ${gi + 1}/${groups.length}: ${group.items.length} سفارش در ${fireLogStr}`,
      "send",
    );
    for (const p of group.items) {
      store.markOrderFired(plan.id, p.orderId);
    }
    store.setPlanMessage(
      plan.id,
      `🔥 شلیک ${group.items.length} سفارش در ${fireLogStr}`,
    );
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