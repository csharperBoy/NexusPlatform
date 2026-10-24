export interface TradingScheduleInfoView {
  id: string;
  code: string;
  name: string;
  description?: string | null;
  preMarketStart: string;
  sessionStart: string;
  sessionEnd: string;
  afternoonStart?: string | null;
  afternoonEnd?: string | null;
  afterHoursEnd?: string | null;
  isActive: boolean;
}