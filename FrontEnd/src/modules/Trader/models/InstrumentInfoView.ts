export interface InstrumentInfoView {
  id: string;
  isin: string;
  symbolName: string;
  displayName: string;
  type: number;
  typeName: string;
  market: number;
  marketName: string;
  boardCode?: string | null;
  tradingScheduleId?: string | null;
  tradingScheduleCode?: string | null;
  tradingScheduleName?: string | null;
  isActive: boolean;
  createdAt: string;
}