import { AUTH_SERVER_BASE, fetchJson, ApiError } from "./client";

interface LoginResponse {
  access_token: string;
  id_token?: string;
  expires_in: number;
  scope: string;
}

interface LoginErrorResponse {
  error?: string;
}

export async function loginToEasyTrader(
  username: string,
  password: string,
): Promise<string> {
  if (!username.trim() || !password) {
    throw new ApiError("نام کاربری یا رمز خالیه", 400);
  }
  try {
    const data = await fetchJson<LoginResponse>(`${AUTH_SERVER_BASE}/api/login`, {
      method: "POST",
      body: JSON.stringify({ username: username.trim(), password }),
    });
    if (!data.access_token) throw new ApiError("access_token دریافت نشد", 500);
    return data.access_token;
  } catch (e) {
    if (e instanceof ApiError) throw e;
    const msg =
      e instanceof Error ? e.message : "خطای نامشخص در لاگین";
    throw new ApiError(msg, 0);
  }
}

/** چک سلامت auth server */
export async function pingAuthServer(): Promise<boolean> {
  try {
    await fetchJson<{ ok: boolean }>(`${AUTH_SERVER_BASE}/api/health`);
    return true;
  } catch {
    return false;
  }
}

/* ─── type-only export ─── */
export type { LoginResponse, LoginErrorResponse };