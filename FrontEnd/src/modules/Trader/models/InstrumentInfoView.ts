export interface InstrumentInfoView {
  id: string;
  isin: string;
  symbolName: string;
  displayName: string;
  type: number;         // InstrumentType
  typeName: string;
  market: number;       // MarketType
  marketName: string;
  boardCode?: string | null;
  isActive: boolean;
  createdAt: string;    // ISO
}