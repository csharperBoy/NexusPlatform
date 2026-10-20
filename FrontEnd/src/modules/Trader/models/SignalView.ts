export enum SignalAction {
  Hold = 0,
  Buy = 1,
  Sell = 2,
}

export const SIGNAL_ACTION_LABELS: Record<SignalAction, string> = {
  [SignalAction.Hold]: "خنثی",
  [SignalAction.Buy]: "خرید",
  [SignalAction.Sell]: "فروش",
};

export const SIGNAL_ACTION_COLORS: Record<SignalAction, string> = {
  [SignalAction.Hold]: "bg-slate-700 text-slate-300",
  [SignalAction.Buy]: "bg-emerald-900/40 text-emerald-300",
  [SignalAction.Sell]: "bg-red-900/40 text-red-300",
};

export interface SignalView {
  id: string;
  runId: string;
  isin: string;
  action: number;
  actionName: string;
  strength: number;
  strategyKey: string;
  producedAt: string;
  consumedAt: string | null;
}