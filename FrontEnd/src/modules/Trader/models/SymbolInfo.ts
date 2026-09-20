export interface SymbolInfo {
  symbolIsin: string;
  highAllowedPrice: number | null;
  lowAllowedPrice: number | null;
  lastTradedPrice: number | null;
  closingPrice: number | null;
  firstTradedPrice: number | null;
  tradeDate: string | null;
  /** زمان fetch (ms) — برای cache TTL */
  fetchedAt: number;
}

/** پاسخ خام API (فقط فیلدهایی که استفاده میکنیم + بقیه nادیده) */
export interface RawSymbolInfoResponse {
  symbolISIN: string;
  highAllowedPrice: number | null;
  lowAllowedPrice: number | null;
  lastTradedPrice: number | null;
  closingPrice: number | null;
  firstTradedPrice: number | null;
  tradeDate: string | null;
  [key: string]: unknown;
}