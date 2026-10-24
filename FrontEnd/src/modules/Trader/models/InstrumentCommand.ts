export interface OptionContractModel {
  optionType: number;
  strikePrice: number;
  expirationDate: string;
  underlyingIsin: string;
}

export interface CreateInstrumentCommand {
  isin: string;
  symbolName: string;
  displayName: string;
  type: number;
  market: number;
  boardCode?: string | null;
  tradingScheduleId?: string | null;
  optionContract?: OptionContractModel | null;
}

export interface UpdateInstrumentCommand {
  id: string;
  symbolName: string;
  displayName: string;
  boardCode?: string | null;
  tradingScheduleId?: string | null;
}