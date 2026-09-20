import { EASYTRADER_BASE, fetchJson } from "./client";
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