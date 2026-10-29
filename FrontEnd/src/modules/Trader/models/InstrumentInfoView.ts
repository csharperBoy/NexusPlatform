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

  // NEW — Commission + Order Limits
  buyCommissionRate: number;      // 0.0037 = 0.37%
  sellCommissionRate: number;
  minOrderQuantity: number;
  maxOrderQuantity: number;
  minOrderValueRial?: number | null;

  isActive: boolean;
  createdAt: string;
}