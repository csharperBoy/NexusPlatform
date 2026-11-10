//src/modules/Trader/models/TradingRun.ts
/* ═══════════════ View Models (Response) ═══════════════ */

export interface TradingRunInfoView {
  id: string;
  name: string;
  state: number;
  stateName: string;
  strategyKey: string;
  primaryTimeFrame: number;
  primaryTimeFrameName: string;
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

export interface TradingRunAccountDto {
  accountId: string;
  capitalMode: number;                     // 0=Fixed, 1=Dynamic
  fixedCapitalAmount?: number | null;
  fixedCapitalCurrency?: string | null;
  percentOfBuyPower?: number | null;
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
  accounts: TradingRunAccountDto[];        // ← was accountIds: string[]
  startedAt?: string | null;
  stoppedAt?: string | null;
  createdAt: string;
  modifiedAt?: string | null;
}

/* ═══════════════ Command Models (Request) ═══════════════ */

export interface TradingRunSettingsModel {
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

export interface TradingRunAccountModel {
  accountId: string;
  capitalMode: number;                    // 0=Fixed, 1=Dynamic
  fixedCapitalAmount?: number | null;
  percentOfBuyPower?: number | null;
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
  accounts: TradingRunAccountModel[];     // ← was accountIds: string[]
}

export interface UpdateTradingRunCommand extends CreateTradingRunCommand {
  id: string;
}

export interface StopTradingRunCommand {
  id: string;
  behavior: number;
}