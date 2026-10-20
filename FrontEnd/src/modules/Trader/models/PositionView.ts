export enum PositionState {
  Open = 0,
  PartiallyClosed = 1,
  Closed = 2,
}

export const POSITION_STATE_LABELS: Record<PositionState, string> = {
  [PositionState.Open]: "باز",
  [PositionState.PartiallyClosed]: "بخشی بسته",
  [PositionState.Closed]: "بسته",
};

export const POSITION_STATE_COLORS: Record<PositionState, string> = {
  [PositionState.Open]: "bg-emerald-900/40 text-emerald-300",
  [PositionState.PartiallyClosed]: "bg-amber-900/40 text-amber-300",
  [PositionState.Closed]: "bg-slate-700 text-slate-300",
};

export interface PositionView {
  id: string;
  runId: string;
  traderAccountId: string;
  isin: string;
  state: number;
  stateName: string;
  quantity: number;
  averageEntryPrice: number;
  realizedPnL: number;
  unrealizedPnL: number | null;
  stopLossPrice: number | null;
  takeProfitPrice: number | null;
  openedAt: string;
  closedAt: string | null;
}