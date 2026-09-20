import { useEffect, useRef } from "react";
import { useScheduleStore } from "../stores/useScheduleStore";
import { useAccountsStore } from "../stores/useAccountsStore";
import { useSymbolInfoStore } from "../stores/useSymbolInfoStore";
import { useSymbolsStore } from "../stores/useSymbolsStore";
import { useLogStore } from "../stores/useLogStore";
import { useLoginStore } from "../stores/useLoginStore";
import { fireOrder } from "../utils/fireOrder";
import { parseTargetTime, preciseWait, todayDateKey } from "../utils";
import type { SchedulePlan, ScheduledOrder } from "../models";
import type { UseServerClockResult } from "./useServerClock";

const TICK_MS = 1000;
const MAX_LATE_MS = 5 * 1000;
const PRE_CLOCK_SYNC_MS = 5000;

interface Options {
  clock: UseServerClockResult;
}

export function useSchedulerRunner({ clock }: Options): void {
  const runningRef = useRef(false);
  const clockRef = useRef(clock);
  clockRef.current = clock; // latest-ref pattern

  useEffect(() => {
    const tick = async () => {
      if (runningRef.current) return;
      runningRef.current = true;

      try {
        const s = useScheduleStore.getState();
        if (!s.enabled) {
          if (s.runtime.status !== "idle") {
            s.setRuntime({ status: "idle", message: "" });
          }
          return;
        }

        const today = todayDateKey();
        const now = Date.now();

        /* ─── روز عوض شده؟ reset ─── */
        if (s.runtime.currentDate !== today) {
          s.setRuntime({
            currentDate: today,
            lastLoginDate: null,
            lastRefreshDate: null,
            firedKeys: [],
            status: "waiting-login",
            message: "",
          });
          return;
        }

        /* ─── پلن‌های امروز ─── */
        const todayPlans = s.plans.filter(
          (p) => p.enabled && p.date === today && p.orders.length > 0,
        );

        if (todayPlans.length === 0) {
          const futureCount = s.plans.filter(
            (p) => p.enabled && p.date > today && p.orders.length > 0,
          ).length;

          s.setRuntime({
            status: "idle",
            message:
              futureCount > 0
                ? `${futureCount} برنامه آینده در انتظار`
                : "هیچ برنامه فعالی برای امروز نیست",
          });
          return;
        }

        /* ═══ مرحله ۱: لاگین ═══ */
        if (s.runtime.lastLoginDate !== today) {
          const loginAt = parseTargetTime(s.autoLoginAt, today);
          if (loginAt === null) {
            s.setRuntime({ status: "error", message: "زمان لاگین نامعتبره" });
            return;
          }

          if (now < loginAt) {
            const remain = Math.floor((loginAt - now) / 1000);
            s.setRuntime({
              status: "waiting-login",
              message: `لاگین تا ${remain} ثانیه دیگر`,
            });
            return;
          }

          if (now - loginAt > MAX_LATE_MS) {
            s.setRuntime({
              status: "cancelled",
              message: `از زمان لاگین (${s.autoLoginAt}) بیش از حد گذشته`,
            });
            return;
          }

          s.setRuntime({
            status: "logging-in",
            message: "در حال لاگین همه حساب‌ها...",
          });
          useLogStore.getState().append("🔄 لاگین خودکار همه حساب‌ها", "info");

          const accounts = useAccountsStore.getState().accounts;
          const results = await useLoginStore
            .getState()
            .loginAll({ silent: false });
          const ok = results.filter((r) => r.ok).length;

          useLogStore
            .getState()
            .append(
              `✅ ${ok}/${accounts.length} حساب لاگین شد`,
              ok > 0 ? "ok" : "err",
            );

          if (ok === 0) {
            s.setRuntime({
              status: "error",
              message: "هیچ حسابی لاگین نشد",
            });
            return;
          }

          s.setRuntime({
            status: "waiting-refresh",
            lastLoginDate: today,
            lastLoginAt: Date.now(),
            message: "لاگین انجام شد",
          });
          return;
        }

        /* ═══ مرحله ۲: رفرش قیمت‌ها ═══ */
        if (s.runtime.lastRefreshDate !== today) {
          const refreshAt = parseTargetTime(s.autoRefreshAt, today);
          if (refreshAt === null) {
            s.setRuntime({ status: "error", message: "زمان رفرش نامعتبره" });
            return;
          }

          if (now < refreshAt) {
            const remain = Math.floor((refreshAt - now) / 1000);
            s.setRuntime({
              status: "waiting-refresh",
              message: `رفرش قیمت‌ها تا ${remain} ثانیه دیگر`,
            });
            return;
          }

          if (now - refreshAt > MAX_LATE_MS) {
            s.setRuntime({
              status: "cancelled",
              message: `از زمان رفرش (${s.autoRefreshAt}) بیش از حد گذشته`,
            });
            return;
          }

          s.setRuntime({
            status: "refreshing",
            message: "در حال رفرش قیمت‌ها...",
          });
          useLogStore.getState().append("🔄 رفرش قیمت نمادها", "info");

          const validToken =
            useAccountsStore
              .getState()
              .accounts.find((a) => a.token?.trim())?.token ?? "";

          if (!validToken) {
            s.setRuntime({
              status: "error",
              message: "توکن معتبری برای رفرش نیست",
            });
            return;
          }

          const uniqueIsins = Array.from(
            new Set(
              todayPlans
                .flatMap((p) => p.orders.map((o) => o.symbolIsin))
                .filter((x) => !!x),
            ),
          );

          const ensureInfo = useSymbolInfoStore.getState().ensureInfo;
          let ok = 0;
          for (const isin of uniqueIsins) {
            const info = await ensureInfo(isin, validToken, true);
            if (info) ok++;
          }

          useLogStore
            .getState()
            .append(
              `✅ ${ok}/${uniqueIsins.length} نماد بروزرسانی شد`,
              ok > 0 ? "ok" : "err",
            );

          s.setRuntime({
            status: "waiting-fire",
            lastRefreshDate: today,
            lastRefreshAt: Date.now(),
            message: "قیمت‌ها آماده است",
          });
          return;
        }

        /* ═══ مرحله ۳: شلیک ═══ */
        type Pending = {
          plan: SchedulePlan;
          order: ScheduledOrder;
          target: number;
        };
        const pending: Pending[] = [];

        for (const p of todayPlans) {
          for (const o of p.orders) {
            if (!o.accountId || !o.symbolIsin) continue;
            if (s.runtime.firedKeys.includes(`${p.id}:${o.id}`)) continue;
            const t = parseTargetTime(o.time, p.date);
            if (t !== null) pending.push({ plan: p, order: o, target: t });
          }
        }

        if (pending.length === 0) {
          s.setRuntime({ status: "done", message: "همه سفارش‌ها ارسال شد" });
          return;
        }

        pending.sort((a, b) => a.target - b.target);
        const next = pending[0];
        const lateBy = now - next.target;

        /* دیر شده — کنسل */
        if (lateBy > MAX_LATE_MS) {
          useScheduleStore.getState().markFired(next.plan.id, next.order.id);
          useLogStore
            .getState()
            .append(
              `⏭ [${next.plan.name}] کنسل — از ${next.order.time} دیرتر`,
              "err",
            );
          return;
        }

        /* در انتظار */
        if (now < next.target) {
          const remainSec = (next.target - now) / 1000;
          s.setRuntime({
            status: "waiting-fire",
            message: `شلیک بعدی تا ${remainSec.toFixed(1)} ثانیه`,
          });

          /* نزدیک شلیک → سینک ساعت */
          if (next.target - now < PRE_CLOCK_SYNC_MS) {
            useLogStore.getState().append("⏰ سینک متراکم قبل از شلیک...", "info");
            await clockRef.current.sync(5);
          }

          /* خیلی نزدیک → انتظار دقیق */
          if (next.target - now < 2000) {
            const clientFire = next.target - clockRef.current.info.diff;
            if (clientFire > Date.now()) {
              await preciseWait(clientFire);
            }
          }
          return;
        }

        /* شلیک */
        s.setRuntime({
          status: "firing",
          message: `شلیک [${next.plan.name}] — ${next.order.time}`,
        });

        useScheduleStore.getState().markFired(next.plan.id, next.order.id);

        await fireScheduledOrder(next.plan, next.order);
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
  }, []);
}

/* ─── شلیک یک سفارش زمان‌بندی‌شده ─── */
async function fireScheduledOrder(
  plan: SchedulePlan,
  o: ScheduledOrder,
): Promise<void> {
  const log = useLogStore.getState().append;

  const account = useAccountsStore.getState().getAccount(o.accountId);
  if (!account) {
    log("❌ حساب پیدا نشد", "err");
    return;
  }
  if (!account.token?.trim()) {
    log(`❌ [${account.name}] توکن خالیه`, "err");
    return;
  }

  const symbol = useSymbolsStore.getState().getSymbol(o.symbolIsin);
  if (!symbol) {
    log("❌ نماد پیدا نشد", "err");
    return;
  }

  const info = useSymbolInfoStore.getState().cache[o.symbolIsin];
  if (!info) {
    log(`❌ اطلاعات [${symbol.symbolName}] لود نشده`, "err");
    return;
  }

  const price = o.side === 0 ? info.highAllowedPrice : info.lowAllowedPrice;
  if (!price || price <= 0) {
    log(
      `❌ [${account.name}] ${symbol.symbolName}: قیمت مجاز موجود نیست`,
      "err",
    );
    return;
  }

  let quantity: number;
  if (o.mode === "quantity") {
    quantity = Number(o.quantity);
  } else {
    const totalValue = Number(o.totalValue);
    quantity = Math.floor(totalValue / (price * (1 + symbol.commission)));
  }

  if (!Number.isFinite(quantity) || quantity <= 0) {
    log(
      `❌ [${account.name}] ${symbol.symbolName}: تعداد نامعتبر (${quantity})`,
      "err",
    );
    return;
  }

  log(
    `📋 [${plan.name}] ${symbol.symbolName} — قیمت ${price} × ${quantity}`,
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