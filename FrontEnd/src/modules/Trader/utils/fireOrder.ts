import { buildOrderPayload } from "./buildOrderPayload";
import { sendOrder } from "../api";
import { logTimestamp } from "./time";
import type { Symbol, OrderResponse } from "../models";
import type { LogType } from "../stores/useLogStore";

const RETRYABLE_CODES = [11000];
const RETRY_DELAYS = [400, 600, 900, 1300];

export interface FireOrderParams {
  token: string;
  symbol: Symbol;
  price: number;
  quantity: number;
  accountName: string;
  onLog: (msg: string, type?: LogType) => void;
  shouldCancel?: () => boolean;
}

export async function fireOrder(
  params: FireOrderParams,
  attempt = 1,
): Promise<OrderResponse | null> {
  const { token, symbol, price, quantity, accountName, onLog, shouldCancel } =
    params;
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

    const code = data.omsError?.[0]?.code;
    const msg = data.message ?? "خطا";
    onLog(
      `❌ [${accountName}] ${symbol.symbolName} در ${logTimestamp(new Date(recvWallMs))} — ${code} — ${msg}`,
      "err",
    );

    if (
      code !== undefined &&
      RETRYABLE_CODES.includes(code) &&
      attempt <= RETRY_DELAYS.length &&
      !shouldCancel?.()
    ) {
      const delay = RETRY_DELAYS[attempt - 1];
      onLog(
        `🔁 [${accountName}] ${symbol.symbolName}: تلاش ${attempt + 1} در ${delay}ms`,
        "info",
      );
      await new Promise((r) => setTimeout(r, delay));
      if (shouldCancel?.()) return null;
      return fireOrder(params, attempt + 1);
    }

    return data;
  } catch (e) {
    const dt = (performance.now() - t0).toFixed(1);
    const msg = e instanceof Error ? e.message : "خطای نامشخص";
    onLog(`💥 [${accountName}] ${symbol.symbolName} — ${msg} (${dt}ms)`, "err");
    return null;
  }
}