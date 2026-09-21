import { useCallback, useEffect, useRef, useState } from "react";
import { fetchServerTime } from "../api";
import { useServerClockStore } from "../stores/useServerClockStore";
import type { ServerClockSample, ServerClockInfo } from "../models";

const WINDOW_MS = 5 * 60 * 1000; // ۵ دقیقه
const MAX_SAMPLES = 20;
const SYNC_INTERVAL_MS = 15 * 1000; // هر ۱۵ ثانیه سینک پس‌زمینه
const TIMEOUT_MS = 800; // حداکثر زمان انتظار برای هر درخواست سینک

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
  const rtts = samplesRef.current.map((x) => x.rtt);

  return {
    medianOffset: median(offs),
    medianDiff: median(diffs),
    medianRtt: median(rtts),
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
 * سینک ساعت با سرور EasyTrader
 * - سینک خودکار غیربلاک‌کننده در پس‌زمینه
 * - ارسال موازی درخواست‌ها با AbortController Timeout (۸۰۰ms)
 * - بروزرسانی خودکار store سراسری useServerClockStore
 */
export function useServerClock(token: string): UseServerClockResult {
  const tokenRef = useRef(token);
  tokenRef.current = token;

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
      const err = "توکن خالی — اول توکن رو بذار";
      setInfo((p) => ({ ...p, lastError: err }));
      useServerClockStore.getState().setClockData({ lastError: err });
      return null;
    }
    if (busyRef.current) {
      /* اگه یک سینک دیگه‌ همزمان جاری باشه، دلیلی نداره منتظر بمونیم، آخرین مقدار موجود رو فوراً برگردون */
      return useServerClockStore.getState().offset;
    }

    busyRef.current = true;
    setInfo((p) => ({ ...p, busy: true, lastError: null }));
    useServerClockStore.getState().setClockData({ busy: true, lastError: null });

    let lastErr: string | null = null;
    let ok = 0;

    try {
      /* اجرای موازی درخواست‌ها با تایم‌اوت مشخص تا به هیچ وجه بلاک ایجاد نکند */
      const promises = Array.from({ length: count }).map(async () => {
        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), TIMEOUT_MS);
        try {
          const r = await fetchServerTime(tk, controller.signal);
          clearTimeout(timeoutId);
          return { ...r, ts: Date.now() };
        } catch (e) {
          clearTimeout(timeoutId);
          throw e;
        }
      });

      const results = await Promise.allSettled(promises);

      for (const res of results) {
        if (res.status === "fulfilled") {
          samplesRef.current.push(res.value);
          ok++;
        } else {
          lastErr =
            res.reason instanceof Error
              ? res.reason.message
              : "خطای تایم‌اوت یا شبکه";
        }
      }

      const { medianOffset, medianDiff, medianRtt, count: n, min, max } =
        computeStats(samplesRef);

      const nowIso = new Date().toISOString();

      if (mountedRef.current) {
        setOffset(medianOffset);
        setInfo((p) => ({
          ...p,
          samples: n,
          lastRtt: Math.round(medianRtt),
          lastSync: new Date(),
          offset: Math.round(medianOffset),
          diff: Math.round(medianDiff),
          min: Math.round(min),
          max: Math.round(max),
          lastError: ok > 0 ? null : lastErr,
        }));
      }

      /* آپدیت Store سراسری */
      useServerClockStore.getState().setClockData({
        offset: Math.round(medianOffset),
        rtt: Math.round(medianRtt),
        samplesCount: n,
        minOffset: Math.round(min),
        maxOffset: Math.round(max),
        lastSync: nowIso,
        busy: false,
        lastError: ok > 0 ? null : lastErr,
      });

      return ok === 0 ? null : medianOffset;
    } finally {
      busyRef.current = false;
      if (mountedRef.current) {
        setInfo((p) => ({ ...p, busy: false }));
      }
      useServerClockStore.getState().setClockData({ busy: false });
    }
  }, []);

  /* ─── سینک خودکار پس‌زمینه هر ۱۵ ثانیه ─── */
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
    const t = setTimeout(() => sync(2), 1000);
    return () => clearTimeout(t);
  }, [token, sync]);

  const reset = useCallback(() => {
    samplesRef.current = [];
    setOffset(0);
    useServerClockStore.getState().setClockData({
      offset: 0,
      rtt: 0,
      samplesCount: 0,
      minOffset: 0,
      maxOffset: 0,
      lastSync: null,
      lastError: null,
    });
  }, []);

  return { offset, info, sync, reset };
}
