// src/modules/HR/models/SyncModels.ts

export interface SyncResult {
  addedCount: number;
  updatedCount: number;
  deletedCount: number;
}

/**
 * هر آیتم تغییر شامل یک متن قابل نمایش و Command مربوطه
 */
export interface SyncPreviewItem {
  id: string;          // معادل Guid در بک‌اند
  summary: string;     // متن تولیدشده در بک‌اند
  command: any;        // خود Command (برای ارسال به Apply)
}

/**
 * باندل یکپارچه برای هر موجودیت
 */
export interface SyncCommandBundle {
  addCommands: SyncPreviewItem[];
  updateCommands: SyncPreviewItem[];
  deleteCommands: SyncPreviewItem[];
  warnings: string[];
}

export type SyncEntityKey = 'orgUnit' | 'jobLevel' | 'jobTitle' | 'employment' | 'post' | 'assignment';

export const entityLabels: Record<SyncEntityKey, string> = {
  orgUnit: 'واحد سازمانی',
  jobLevel: 'سطح شغلی',
  jobTitle: 'عنوان شغلی',
  employment: 'کارمند',
  post: 'پست',
  assignment: 'انتصابات'
};