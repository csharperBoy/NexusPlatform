export interface TradingRunInfoView {
  id: string;
  name: string;
  state: number;
  stateName: string;
  strategyKey: string;
  primaryTimeFrame: number;
  primaryTimeFrameName: string;
  capitalAllocationMode: number;
  fixedCapital?: number | null;
  targetCurrency: string | null; 
  strengthMultiplier: number;    
  minDeltaPercent: number; 
  symbolCount: number;
  accountCount: number;
  startedAt?: string | null;
  stoppedAt?: string | null;
  createdAt: string;
}

export interface TradingRunSettingsDto {
  capitalAllocationMode: number;
  fixedCapitalAmount?: number | null;
  stopLossPercentage?: number | null;
  stopLossPrice?: number | null;
  takeProfitPercentage?: number | null;
  takeProfitPrice?: number | null;
  defaultOrderMode: number;
  onMarketClose: number;
  onStop: number;
}

export interface TradingRunSymbolDto {
  isin: string;
  allocationPercentage: number;
}

export interface TradingRunDetailView {
  id: string;
  name: string;
  state: number;
  stateName: string;
  strategyKey: string;
  strategyConfigJson: string;
  primaryTimeFrame: number;
  targetCurrency: string | null;  
  strengthMultiplier: number;   
  minDeltaPercent: number;
  settings: TradingRunSettingsDto;
  symbols: TradingRunSymbolDto[];
  accountIds: string[];
  startedAt?: string | null;
  stoppedAt?: string | null;
  createdAt: string;
  modifiedAt?: string | null;
}

/* ─── Commands ─── */
export interface TradingRunSettingsModel {
  capitalAllocationMode: number;
  fixedCapitalAmount?: number | null;
  stopLossPercentage?: number | null;
  stopLossPrice?: number | null;
  takeProfitPercentage?: number | null;
  takeProfitPrice?: number | null;
  defaultOrderMode: number;
  onMarketClose: number;
  onStop: number;
}

export interface TradingRunSymbolModel {
  isin: string;
  allocationPercentage: number;
}

export interface CreateTradingRunCommand {
  name: string;
  strategyKey: string;
  strategyConfigJson: string;
  primaryTimeFrame: number;
  targetCurrency: string | null;  
  strengthMultiplier: number;   
  minDeltaPercent: number;
  settings: TradingRunSettingsModel;
  symbols: TradingRunSymbolModel[];
  accountIds: string[];
}

export interface UpdateTradingRunCommand extends CreateTradingRunCommand {
  id: string;
}

export interface StopTradingRunCommand {
  id: string;
  behavior: number; // StopRunBehavior
}