export interface SaveIndicatorBulkChangeRequest {
  saveList: SaveIndicatorChangeRequest[]; // فقط اطلاعات لازم برای ذخیره
  deleteList: string[];                   // فقط آیدی‌های لازم برای حذف
}
export interface SaveIndicatorBulkChangeResponse {
  isSuccess: boolean;    // فقط اطلاعات لازم برای افزودن
  }

export interface SaveIndicatorChangeRequest { 
    id?: string | null;
    fkLkpValueTypeId: number;
    fkLkpShiftId: number;
    fkLkpPeriodId: number;
    fkLkpMeasureId: number;
    fkLkpDesirabilityId: number;
    fkLkpFormId: number;
    fkLkpManualityId: number;
    code: string;
    title: string;
    formula: string;
    description:  string | null;
    dateTimeFrom: Date;
    dateTimeTo: Date | null;
}
export interface SaveIndicatorChangeResponse {
  isSuccess: boolean;
  id?: string | null; // چون Guid در C# به string در TypeScript تبدیل می‌شه
}
