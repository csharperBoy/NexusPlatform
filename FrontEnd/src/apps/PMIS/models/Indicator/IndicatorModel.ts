
export interface IndicatorModel {
  id?: string;
  code: string;
  title: string;
  dateTimeFrom: Date;
  dateTimeTo: Date | null;
  fkLkpTypeId: number;
  fkLkpManualityId: number;
  fkLkpPeriodId: number;
  fkLkpMeasureId: number;
  fkLkpDesirabilityId: number;
  fkLkpFormId: number;
  formula?: string;
  description?: string;
}