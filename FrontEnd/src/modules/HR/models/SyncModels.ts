// src/modules/HR/models/SyncModels.ts
export interface SyncResult {
  addedCount: number;
  updatedCount: number;
  deletedCount: number;
  errors: string[];
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