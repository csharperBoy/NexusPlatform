export interface CurrencyRateView {
  id: string;
  fromCurrency: string;
  toCurrency: string;
  rate: number;
  effectiveAt: string;        // ISO with timezone
  source: string;
  createdAt: string;          // ISO with timezone
}