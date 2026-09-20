export interface Account {
  id: string;
  name: string;
  /** کد ملی */
  username: string;
  /** رمز عبور (در localStorage obfuscate میشه) */
  password: string;
  /** توکن Bearer EasyTrader (بدون کلمه Bearer) */
  token: string;
}

export type TokenState = "empty" | "invalid" | "expired" | "valid";

export interface TokenStatus {
  state: TokenState;
  /** timestamp انقضا (ms) */
  exp?: number;
  /** ms تا انقضا */
  remainMs?: number;
}