import { useCallback, useEffect, useRef, useState } from "react";
import { fetchServerTime } from "../api";
import type { ServerClockSample, ServerClockInfo } from "../models";

const WINDOW_MS = 5 * 60 * 1000; // ۵ دقیقه
const MAX_SAMPLES = 20;
const SYNC_INTERVAL_MS = 30 * 1000;

function median(arr: number[]): number {
  if (arr.length === 0) return 0;
  const s = [...arr].sort((a, b) => a - b);
  const m = Math.floor(s.length / 2);
  return s.length % 2 ? s[m] : (s[m - 1] + s[m]) / 2;
}

function computeStats(samplesRef: { current: ServerClockSample[] }) {
  const now = Date.now();
  samplesRef.current = samplesRef.current
    .filter((x) => now - x.ts < WINDOW_MS)
    .slice(-MAX_SAMPLES);

  const offs = samplesRef.current.map((x) => x.offset);
  const diffs = samplesRef.current.map((x) => x.rawDiff);

  return {
    medianOffset: median(offs),
    medianDiff: median(diffs),
    count: offs.length,
    min: offs.length ? Math.min(...offs) : 0,
    max: offs.length ? Math.max(...offs) : 0,
  };
}

export interface UseServerClockResult {
  offset: number;
  info: ServerClockInfo;
  sync: (count?: number) => Promise<number | null>;
  reset: () => void;
}

/**
 * ✅ access از هر جا بدون React — مقدار آخرین offset/diff محاسبه‌شده
 * این object همیشه live آپدیت میشه، هر جا import کنی می‌تونی بخونی
 */
export const serverClockRef = {
  diff: 0,          // diff نهایی (برای محاسبه‌ی شلیک)
  offset: 0,        // offset خالص
  oneWayLatency: 0, // rtt/2
  lastUpdatedAt: 0, // ms timestamp
};

/**
 * سینک ساعت با سرور EasyTrader
 * - سینک خودکار هر ۳۰ ثانیه (اگه توکن داشته باشیم)
 * - سینک اولیه بعد از ۱ ثانیه توقف تایپ (debounce)
 * - return مقدار `medianDiff` که برای جبران تاخیر استفاده میشه
 */
export function useServerClock(token: string): UseServerClockResult {
  const tokenRef = useRef(token);
  tokenRef.current = token; // latest-ref pattern

  const samplesRef = useRef<ServerClockSample[]>([]);
  const mountedRef = useRef(true);
  const busyRef = useRef(false);

  const [offset, setOffset] = useState(0);
  const [info, setInfo] = useState<ServerClockInfo>({
    samples: 0,
    lastRtt: 0,
    lastSync: null,
    offset: 0,
    diff: 0,
    min: 0,
    max: 0,
    lastError: null,
    busy: false,
  });

  const sync = useCallback(async (count = 1): Promise<number | null> => {
    const tk = tokenRef.current;
    if (!tk?.trim()) {
      setInfo((p) => ({ ...p, lastError: "توکن خالی — اول توکن رو بذار" }));
      return null;
    }
    if (busyRef.current) {
      setInfo((p) => ({ ...p, lastError: "یه سینک دیگه در جریانه" }));
      return null;
    }

    busyRef.current = true;
    setInfo((p) => ({ ...p, busy: true, lastError: null }));

    let lastErr: string | null = null;
    let ok = 0;

    try {
      for (let i = 0; i < count; i++) {
        try {
          const r = await fetchServerTime(tk);
          samplesRef.current.push({ ...r, ts: Date.now() });
          ok++;
          if (mountedRef.current) {
            setInfo((p) => ({
              ...p,
              lastRtt: Math.round(r.rtt),
              lastSync: new Date(),
            }));
          }
        } catch (e) {
          lastErr = e instanceof Error ? e.message : "خطای نامشخص";
        }
      }

      const { medianOffset, medianDiff, count: n, min, max } =
        computeStats(samplesRef);

            /* ✅ آپدیت ref سراسری — قابل خواندن از هر جا (حتی بدون React) */
      serverClockRef.diff = Math.round(medianDiff);
      serverClockRef.offset = Math.round(medianOffset);
      serverClockRef.oneWayLatency = 0; // اگه rtt رو جدا نگه نمیداری
      serverClockRef.lastUpdatedAt = Date.now();

      if (mountedRef.current) {
        setOffset(medianOffset);
        setInfo((p) => ({
          ...p,
          samples: n,
          offset: Math.round(medianOffset),
          diff: Math.round(medianDiff),
          min: Math.round(min),
          max: Math.round(max),
          lastError: ok > 0 ? null : lastErr,
        }));
      }

      return ok === 0 ? null : medianDiff;
    } finally {
      busyRef.current = false;
      if (mountedRef.current) {
        setInfo((p) => ({ ...p, busy: false }));
      }
    }
  }, []);

  /* ─── سینک خودکار پس‌زمینه هر ۳۰ ثانیه ─── */
  useEffect(() => {
    mountedRef.current = true;
    const id = setInterval(() => {
      if (tokenRef.current?.trim()) sync(1);
    }, SYNC_INTERVAL_MS);
    return () => {
      mountedRef.current = false;
      clearInterval(id);
    };
  }, [sync]);

  /* ─── سینک اولیه بعد از ۱ ثانیه توقف تایپ ─── */
  useEffect(() => {
    if (!token?.trim()) return;
    const t = setTimeout(() => sync(3), 1000);
    return () => clearTimeout(t);
  }, [token, sync]);

  const reset = useCallback(() => {
    samplesRef.current = [];
    setOffset(0);
  }, []);

  return { offset, info, sync, reset };
}