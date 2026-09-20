import { useCallback, useRef, useState } from "react";
import { parseTargetTime, preciseWait, logTimestamp } from "../utils";
import { buildOrderPayload } from "../utils/buildOrderPayload";
import { sendOrder } from "../api";
import type { Symbol, OrderResponse } from "../models";
import type { UseServerClockResult } from "./useServerClock";

const PRESYNC_AT_MS = 2000;
const MIN_INTERVAL_PER_ACCOUNT_SYMBOL = 300;
const RETRYABLE_CODES = [11000]; // فقط GroupStateNotAllowError
const RETRY_DELAYS = [400, 600, 900, 1300];

export interface ShotgunTask {
  /** ID حساب (برای نمایش) */
  accountId: string;
  /** نام حساب (برای لاگ) */
  accountName: string;
  /** توکن Bearer این حساب */
  token: string;
  /** نماد (شامل isin, name, commission, ...) */
  symbol: Symbol;
  price: number;
  quantity: number;
  /** "HH:MM:SS.mmm" */
  time: string;
}

export type SchedulerLogType = "info" | "ok" | "err" | "send";

export interface SchedulerLogFn {
  (msg: string, type?: SchedulerLogType): void;
}

export interface UseShotgunSchedulerOptions {
  clock: UseServerClockResult;
  onLog: SchedulerLogFn;
}

export interface UseShotgunSchedulerResult {
  status: "idle" | "armed" | "sending";
  remaining: number | null;
  nextLabel: string;
  arm: (tasks: ShotgunTask[]) => Promise<void>;
  cancel: () => void;
}

interface PreparedTask extends ShotgunTask {
  target: number;
}

interface FirePlanItem extends PreparedTask {
  clientFireMs: number;
}

export function useShotgunScheduler({
  clock,
  onLog,
}: UseShotgunSchedulerOptions): UseShotgunSchedulerResult {
  const [status, setStatus] = useState<"idle" | "armed" | "sending">("idle");
  const [remaining, setRemaining] = useState<number | null>(null);
  const [nextLabel, setNextLabel] = useState("");
  const cancelRef = useRef(false);

  /* ─── ارسال یک سفارش با retry ─── */
  const fireOne = useCallback(
    async (
      task: ShotgunTask,
      attempt = 1,
    ): Promise<OrderResponse | null> => {
      const { token, symbol, price, quantity, accountName } = task;
      const payload = buildOrderPayload(symbol, price, quantity);
      const t0 = performance.now();
      const sendWallMs = Date.now();

      onLog(
        `🚀 fetch(${attempt}) در ${logTimestamp(new Date(sendWallMs))} — [${accountName}] ${symbol.symbolName} ${price} × ${quantity}`,
        "send",
      );

      try {
        const data = await sendOrder(token, payload);
        const dt = (performance.now() - t0).toFixed(1);
        const recvWallMs = Date.now();

        if (data.isSuccessful) {
          onLog(
            `✅ [${accountName}] ${symbol.symbolName} در ${logTimestamp(new Date(recvWallMs))} — ${dt}ms`,
            "ok",
          );
          return data;
        }

        /* خطا از سرور */
        const code = data.omsError?.[0]?.code;
        const msg = data.message ?? "خطا";
        onLog(
          `❌ [${accountName}] ${symbol.symbolName} در ${logTimestamp(new Date(recvWallMs))} — ${code} — ${msg}`,
          "err",
        );

        /* retry فقط برای خطاهای موقتی */
        if (
          code !== undefined &&
          RETRYABLE_CODES.includes(code) &&
          attempt <= RETRY_DELAYS.length &&
          !cancelRef.current
        ) {
          const delay = RETRY_DELAYS[attempt - 1];
          onLog(
            `🔁 [${accountName}] ${symbol.symbolName}: تلاش ${attempt + 1} در ${delay}ms`,
            "info",
          );
          await new Promise((r) => setTimeout(r, delay));
          if (cancelRef.current) return null;
          return fireOne(task, attempt + 1);
        }

        return data;
      } catch (e) {
        const dt = (performance.now() - t0).toFixed(1);
        const msg = e instanceof Error ? e.message : "خطای نامشخص";
        onLog(`💥 [${accountName}] ${symbol.symbolName} — ${msg} (${dt}ms)`, "err");
        return null;
      }
    },
    [onLog],
  );

  /* ─── اعتبارسنجی و آماده‌سازی ─── */
  const validate = useCallback(
    (tasks: ShotgunTask[]): { ok: true; prepared: PreparedTask[] } | { ok: false; error: string } => {
      if (tasks.length === 0) return { ok: false, error: "سفارشی نداری" };

      const prepared: PreparedTask[] = [];

      for (const t of tasks) {
        if (!t.token?.trim())
          return { ok: false, error: `توکن [${t.accountName}] خالیه` };
        if (!t.symbol?.symbolIsin)
          return { ok: false, error: `نماد [${t.accountName}] نامعتبر` };
        const target = parseTargetTime(t.time);
        if (target === null)
          return {
            ok: false,
            error: `فرمت زمان اشتباه برای [${t.accountName}] ${t.symbol.symbolName}: "${t.time}"`,
          };
        if (!Number.isFinite(t.price) || t.price <= 0)
          return {
            ok: false,
            error: `قیمت نامعتبر برای [${t.accountName}] ${t.symbol.symbolName}`,
          };
        if (!Number.isFinite(t.quantity) || t.quantity <= 0)
          return {
            ok: false,
            error: `حجم نامعتبر برای [${t.accountName}] ${t.symbol.symbolName}`,
          };
        prepared.push({ ...t, target });
      }

      /* ─── چک فاصله ۳۰۰ms بین سفارش‌های یک حساب روی یه نماد ─── */
      const lastPerKey = new Map<string, number>();
      const sorted = [...prepared].sort((a, b) => a.target - b.target);
      for (const t of sorted) {
        const key = `${t.accountId}:${t.symbol.symbolIsin}`;
        const prev = lastPerKey.get(key);
        if (prev !== undefined && t.target - prev < MIN_INTERVAL_PER_ACCOUNT_SYMBOL) {
          return {
            ok: false,
            error: `[${t.accountName}] ${t.symbol.symbolName}: دو سفارش با فاصله ${t.target - prev}ms (حداقل ${MIN_INTERVAL_PER_ACCOUNT_SYMBOL}ms لازمه)`,
          };
        }
        lastPerKey.set(key, t.target);
      }

      return { ok: true, prepared };
    },
    [],
  );

  /* ─── منطق اصلی ─── */
  const arm = useCallback(
    async (tasks: ShotgunTask[]) => {
      const v = validate(tasks);
      if (!v.ok) {
        onLog(v.error, "err");
        return;
      }

      const prepared = v.prepared;
      const earliest = Math.min(...prepared.map((t) => t.target));
      const now0 = Date.now();

      if (earliest - now0 <= 0) {
        onLog("زمان اولین سفارش گذشته است", "err");
        return;
      }
      if (earliest - now0 > 60 * 60 * 1000) {
        onLog("زمان بیشتر از یک ساعت بعد است", "err");
        return;
      }

      cancelRef.current = false;
      setStatus("armed");

      /* ═══ گام ۱: presync در T-2s ═══ */
      const presyncAt = earliest - PRESYNC_AT_MS;
      const wait1 = presyncAt - Date.now();

      if (wait1 > 0) {
        onLog(
          `⏰ انتظار تا T-2s (${(wait1 / 1000).toFixed(2)}s) برای سینک متراکم...`,
          "info",
        );
        await preciseWait(presyncAt, (r) => setRemaining(r));
        if (cancelRef.current) {
          setStatus("idle");
          setRemaining(null);
          onLog("🛑 لغو شد", "err");
          return;
        }
      }

      /* ═══ گام ۲: سینک متراکم ═══ */
      onLog("⏰ سینک متراکم قبل از ارسال...", "info");
      const freshDiff = await clock.sync(5);
      const effectiveDiff = Number.isFinite(freshDiff)
        ? (freshDiff as number)
        : clock.info.diff;

      if (cancelRef.current) {
        setStatus("idle");
        setRemaining(null);
        onLog("🛑 لغو شد", "err");
        return;
      }

      /* ═══ گام ۳: محاسبه‌ی شلیک ═══ */
      const firePlan: FirePlanItem[] = prepared
        .map((t) => ({ ...t, clientFireMs: t.target - effectiveDiff }))
        .sort((a, b) => a.clientFireMs - b.clientFireMs);

      onLog(
        `⏱ مسلح — ${firePlan.length} سفارش | diff=${Math.round(effectiveDiff)}ms | اولین شلیک: ${logTimestamp(new Date(firePlan[0].clientFireMs))}`,
        "info",
      );

      /* ═══ گام ۴: حلقه‌ی شلیک ═══ */
      for (let i = 0; i < firePlan.length; i++) {
        if (cancelRef.current) {
          onLog("🛑 لغو شد", "err");
          break;
        }

        const t = firePlan[i];
        const delta = t.clientFireMs - Date.now();

        setNextLabel(
          `${i + 1}/${firePlan.length} — [${t.accountName}] ${t.symbol.symbolName}`,
        );

        onLog(
          `⏱ ${i + 1}/${firePlan.length} — [${t.accountName}] ${t.symbol.symbolName} | target=${logTimestamp(new Date(t.target))} | شلیک=${logTimestamp(new Date(t.clientFireMs))} | Δ=${delta > 0 ? "+" : ""}${Math.round(delta)}ms`,
          "info",
        );

        if (delta > 0) {
          await preciseWait(t.clientFireMs, (r) => setRemaining(r));
        } else if (delta < -30) {
          onLog(
            `⚠️ [${t.accountName}] ${t.symbol.symbolName}: ${Math.round(-delta)}ms دیر شده — فوری شلیک`,
            "err",
          );
        }

        if (cancelRef.current) {
          onLog("🛑 لغو شد", "err");
          break;
        }

        onLog(
          `🔥 شلیک ${i + 1}/${firePlan.length} — [${t.accountName}] ${t.symbol.symbolName}`,
          "send",
        );

        /* بدون await — موازی */
        void fireOne(t);
      }

      setStatus("idle");
      setRemaining(null);
      setNextLabel("");
    },
    [clock, fireOne, onLog, validate],
  );

  const cancel = useCallback(() => {
    cancelRef.current = true;
    onLog("🛑 درخواست لغو شد", "err");
  }, [onLog]);

  return { status, remaining, nextLabel, arm, cancel };
}