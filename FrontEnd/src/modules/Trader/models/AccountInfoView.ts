export type TokenStatus = "empty" | "invalid" | "expired" | "valid";

export interface AccountInfoView {
  id: string;
  name: string;
  username: string;
  tokenStatus: TokenStatus;
  /** ISO date */
  tokenExp?: string | null;
}