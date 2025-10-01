export interface SaveIndicatorValueBulkChangeRequest {
  saveList: SaveIndicatorValueChangeRequest[]; // فقط اطلاعات لازم برای ذخیره
  deleteList: string[];                   // فقط آیدی‌های لازم برای حذف
}
export interface SaveIndicatorValueBulkChangeResponse {
  isSuccess: boolean;    // فقط اطلاعات لازم برای افزودن
  }

export interface SaveIndicatorValueChangeRequest {
 
  id?: string;
  fkIndicatorId: string;
  fkLkpValueTypeId: number;
  fkLkpShiftId: number;
  dateTime: Date; // یا Date اگر با آبجکت Date کار می‌کنی، ولی string برای JSON رایجه
  value: number;
  valueCumulative: number;
  description?: string | null;
}
export interface SaveIndicatorValueChangeResponse {
  isSuccess: boolean;
  id?: string | null; // چون Guid در C# به string در TypeScript تبدیل می‌شه
}
