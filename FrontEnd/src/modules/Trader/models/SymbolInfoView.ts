export interface SymbolInfoView {
  id: string;
  symbolName: string;
  symbolIsin: string;
  price: number;
  quantity: number;
  side: number;
  validityType: number;
  commission: number;
  orderModelType: number;
  orderFrom: number;
}

export interface MarketSymbolInfoView {
  symbolIsin: string;
  highAllowedPrice?: number | null;
  lowAllowedPrice?: number | null;
  lastTradedPrice?: number | null;
  closingPrice?: number | null;
  tradeDate?: string | null;
  fetchedAt?: number;
}