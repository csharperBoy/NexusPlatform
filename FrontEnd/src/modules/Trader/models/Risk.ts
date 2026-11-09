/* ═══════════════════ Enums ═══════════════════ */

export enum KillSwitchScope {
  Global = 0,
  Account = 1,
}

/* ═══════════════════ Views (Response) ═══════════════════ */

/**
 * وضعیت Kill Switch — Global + Per-Account.
 * Backend: KillSwitchStatusView.
 */
export interface KillSwitchStatusView {
  globalActive: boolean;
  activeAccountIds: string[];
}

/**
 * وضعیت Loss Streak یک (Run, Account).
 * Backend: LossStreakState.
 *
 * نکته: `isCircuitActive` در Backend یک method است (پارامتر DateTime)
 * پس در JSON نمی‌آید. Frontend از cooldownExpiresAt محاسبه می‌کند.
 */
export interface LossStreakState {
  runId: string;
  accountId: string;
  consecutiveLosses: number;
  lastLossAt: string | null;
  lastWinAt: string | null;
  circuitBrokenAt: string | null;
  cooldownExpiresAt: string | null;
}

/* ═══════════════════ Commands (Request) ═══════════════════ */

export interface ActivateKillSwitchCommand {
  scope: number;                    // 0=Global, 1=Account
  accountId?: string | null;
  reason?: string | null;
}

export interface DeactivateKillSwitchCommand {
  scope: number;
  accountId?: string | null;
}

export interface ResetCircuitBreakerCommand {
  runId?: string | null;
  accountId?: string | null;
  reason?: string | null;
}

/* ═══════════════════ Helpers ═══════════════════ */

/**
 * آیا circuit هنوز فعال است؟ (cooldown منقضی نشده)
 */
export function isCircuitActive(state: LossStreakState): boolean {
  if (!state.cooldownExpiresAt) return false;
  return new Date(state.cooldownExpiresAt) > new Date();
}

/**
 * نمایش خوانا برای cooldown باقی‌مانده.
 */
export function formatCooldown(expiresAt: string | null): string {
  if (!expiresAt) return "—";
  const diffMs = new Date(expiresAt).getTime() - Date.now();
  if (diffMs <= 0) return "منقضی";
  const minutes = Math.ceil(diffMs / 60000);
  if (minutes < 60) return `${minutes} دقیقه`;
  const hours = Math.floor(minutes / 60);
  const remMin = minutes % 60;
  return remMin > 0 ? `${hours} ساعت و ${remMin} دقیقه` : `${hours} ساعت`;
}