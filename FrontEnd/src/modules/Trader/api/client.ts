/* ─── Base URLs (از طریق Vite proxy) ─── */
export const AUTH_SERVER_BASE = "/authserver";
export const EASYTRADER_BASE = "/easytrader";

/* ─── Error type ─── */
export class ApiError extends Error {
  constructor(
    message: string,
    public status: number,
    public body?: unknown,
  ) {
    super(message);
    this.name = "ApiError";
  }
}

/* ─── fetch با پارس خودکار JSON ─── */
export async function fetchJson<T>(
  url: string,
  init: RequestInit & { token?: string } = {},
): Promise<T> {
  const { token, ...rest } = init;
  const headers = new Headers(rest.headers);
  if (!headers.has("accept")) headers.set("accept", "application/json");
  if (!headers.has("content-type") && rest.body)
    headers.set("content-type", "application/json");
  if (token) headers.set("authorization", `Bearer ${token.trim()}`);

  const res = await fetch(url, { ...rest, headers });
  const text = await res.text();
  let data: unknown;
  try {
    data = text ? JSON.parse(text) : null;
  } catch {
    data = text;
  }
  if (!res.ok) {
    const msg =
      (data as { message?: string } | null)?.message ??
      `HTTP ${res.status} ${res.statusText}`;
    throw new ApiError(msg, res.status, data);
  }
  return data as T;
}