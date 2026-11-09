//src/modules/Trader/models/TradingRun.ts
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
  executionMode: number;                
  executionModeName: string;  
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

export interface TradingRunAccountModel {
  accountId: string;
  allocationPercent: number;      // (0, 100]
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
  executionMode: number;             
  executionModeName: string; 
  settings: TradingRunSettingsDto;
  symbols: TradingRunSymbolDto[];
  accounts: TradingRunAccountModel[];   
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
  executionMode: number; 
  settings: TradingRunSettingsModel;
  symbols: TradingRunSymbolModel[];
  accounts: TradingRunAccountModel[]; 
}

export interface UpdateTradingRunCommand extends CreateTradingRunCommand {
  id: string;
}

export interface StopTradingRunCommand {
  id: string;
  behavior: number; // StopRunBehavior
}