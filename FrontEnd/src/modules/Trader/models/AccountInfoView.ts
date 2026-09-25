export type TokenStatus = "empty" | "invalid" | "expired" | "valid";

export interface AccountInfoView {
  id: string;
   broker: number;  
  name: string;
  username: string;
  
  sessionStatus: "empty" | "valid" | "expired" | "invalid";  // ← rename
  sessionExp?: number | null;        // ← rename از tokenExp
}