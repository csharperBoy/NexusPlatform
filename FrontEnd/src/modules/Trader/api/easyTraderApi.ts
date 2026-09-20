import { ApiError, EASYTRADER_BASE, fetchJson } from "./client";
import type { OrderPayload, OrderResponse, RawSymbolInfoResponse, ServerClockSample, SymbolInfo } from "../models";

/* ═══════ ارسال سفارش ═══════ */
export async function sendOrder(
  token: string,
  payload: OrderPayload,
): Promise<OrderResponse> {
  const t0 = performance.now();
  const data = await fetchJson<OrderResponse>(
    `${EASYTRADER_BASE}/core/api/v2/order`,
    {
      method: "POST",
      token,
      body: JSON.stringify(payload),
      credentials: "include",
    },
  );
  /* data شامل زمان‌بندی نیست، اون رو در caller حساب میکنیم */
  void t0;
  return data;
}

/* ═══════ اطلاعات نماد ═══════ */
export async function fetchSymbolInfo(
  token: string,
  isin: string,
): Promise<SymbolInfo> {
  const raw = await fetchJson<RawSymbolInfoResponse>(
    `${EASYTRADER_BASE}/symbols/api/MarketData/symbol-info-data`,
    {
      method: "POST",
      token,
      body: JSON.stringify({ isin }),
      credentials: "include",
    },
  );

  return {
    symbolIsin: raw.symbolISIN ?? isin,
    highAllowedPrice: raw.highAllowedPrice ?? null,
    lowAllowedPrice: raw.lowAllowedPrice ?? null,
    lastTradedPrice: raw.lastTradedPrice ?? null,
    closingPrice: raw.closingPrice ?? null,
    firstTradedPrice: raw.firstTradedPrice ?? null,
    tradeDate: raw.tradeDate ?? null,
    fetchedAt: Date.now(),
  };
}

/* ═══════ فعال‌سازی توکن روی api-mts ═══════ */
/**
 * بعد از گرفتن توکن از OIDC، باید یک بار same-login بزنیم تا
 * سرور api-mts یه session براش بسازه. بدون این، همه درخواست‌ها 403 می‌شن.
 *
 * اگر 400 با پیام "already logged in" بگیری، یعنی قبلاً فعال شده — بازم موفق در نظر بگیر.
 */
export async function activateToken(token: string): Promise<void> {
  const body = {
    uuid: crypto.randomUUID(),
    appBuildNo: "53559",
    width: window.innerWidth || 452,
    height: window.innerHeight || 599,
    devicePlatform: "Desktop",
    platformInfo: navigator.userAgent,
  };

  try {
    await fetchJson<unknown>(`${EASYTRADER_BASE}/easy/api/account/same-login`, {
      method: "POST",
      token,
      body: JSON.stringify(body),
      credentials: "include",
    });
  } catch (e) {
    /* 400 با پیام "already logged in" = قبلاً فعال شده = OK */
    if (e instanceof ApiError && e.status === 400) {
      const b = e.body as { errors?: { ""?: string[] } } | undefined;
      const msg = b?.errors?.[""]?.[0] ?? "";
      if (msg.includes("already logged in")) return;
    }
    throw e;
  }
}
/* ═══════ سینک ساعت سرور ═══════ */
export async function fetchServerTime(
  token: string,
): Promise<Omit<ServerClockSample, "ts">> {
  const clientTs = Date.now();
  const t0 = performance.now();

  const data = await fetchJson<{ diff: number; serverTimestamp: number }>(
    `${EASYTRADER_BASE}/easy/api/account/server-time/${clientTs}`,
    { method: "GET", token, credentials: "include" },
  );

  const rtt = performance.now() - t0;
  const offset = data.serverTimestamp - (clientTs + rtt / 2);

  return {
    offset,
    rtt: Math.round(rtt),
    serverTimestamp: data.serverTimestamp,
    rawDiff: data.diff,
  };
}