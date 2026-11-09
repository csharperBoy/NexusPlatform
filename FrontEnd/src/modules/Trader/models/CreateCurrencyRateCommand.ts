//src/modules/Trader/models/CreateCurrencyRateCommand.ts
export interface CreateCurrencyRateCommand {
  fromCurrency: string;
  toCurrency: string;
  rate: number;
  effectiveAt: string;        // ISO
  source?: string;            // default "Manual"
}