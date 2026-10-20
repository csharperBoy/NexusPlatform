/**
 * Mirrors Trader.Domain.Enums.InstrumentType
 */
export enum InstrumentType {
  Stock = 0,
  Option = 1,
  Future = 2,
  Bond = 3,
  Fund = 4,
  Currency = 5,
  Crypto = 6,
}

/**
 * Mirrors Trader.Domain.Enums.MarketType
 */
export enum MarketType {
  IranStock = 0,
  IranOption = 1,
  IranFuture = 2,
  Forex = 10,
  Crypto = 20,
}

export interface OptionTypeOption {
  value: number;
  label: string;
  display: string;
}

export const OPTION_TYPE_OPTIONS: OptionTypeOption[] = [
  { value: 0, label: "خرید (Call)", display: "خرید (Call)" },
  { value: 1, label: "فروش (Put)", display: "فروش (Put)" },
];