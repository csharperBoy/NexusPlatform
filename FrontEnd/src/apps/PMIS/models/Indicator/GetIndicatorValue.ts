// src/models/Indicator/GetIndicatorValue.ts
export interface GetIndicatorValueRequest {
  fromDateTime?: Date;
  toDateTime?: Date;
  fkIndicatorId?: string[];         // شاخص‌ها (guid یا string)
  fkLkpShiftId?: number[];          // شناسه شیفت‌ها
  fkLkpValueTypeId?: number[];      // شناسه نوع مقدار
  fkLkpPeriodId?: number[];         // شناسه دوره
  fkFormId?: number[];              // شناسه فرم
}


export interface GetIndicatorValueResponse {
  id: string; // Guid => string
  fkIndicatorId: string;
  fkLkpValueTypeId: number;
  fkLkpShiftId: number;
  dateTime: Date; 
  value: number;
  valueCumulative: number;
  description?: string;
   hasChanges?: boolean; 
}
