// src/modules/HR/utils/syncOrchestrator.ts
import { SyncApi } from "../api/SyncApi";
import { SyncEntityKey, entityLabels } from "../models/SyncModels";

export interface SyncStepStatus {
  key: SyncEntityKey;
  label: string;
  status: 'idle' | 'loading' | 'success' | 'error';
  message?: string;
}

// ترتیب متوالی و اجباری اجرای همگام‌سازی‌ها
export const SYNC_EXECUTION_ORDER: SyncEntityKey[] = [
  'orgUnit',
  'jobLevel',
  'jobTitle',
  'employment',
  'post',
  'assignment'
];

/**
 * همگام‌سازی کلی ترتیبی (Auto Preview -> Auto Apply)
 */
export const runFullSyncSequentially = async (
  onProgress: (key: SyncEntityKey, status: 'loading' | 'success' | 'error', message?: string) => void
) => {
  const steps: { key: SyncEntityKey; preview: Function; apply: Function }[] = [
    { key: 'orgUnit', preview: SyncApi.SyncOrganizationUnitPreview, apply: SyncApi.ApplyOrganizationUnit },
    { key: 'jobLevel', preview: SyncApi.SyncJobLevelPreview, apply: SyncApi.ApplyJobLevel },
    { key: 'jobTitle', preview: SyncApi.SyncJobTitlePreview, apply: SyncApi.ApplyJobTitle },
    { key: 'employment', preview: SyncApi.SyncEmploymentsPreview, apply: SyncApi.ApplyEmployments },
    { key: 'post', preview: SyncApi.SyncPostPreview, apply: SyncApi.ApplyPost },
    { key: 'assignment', preview: SyncApi.SyncAssignmentsPreview, apply: SyncApi.ApplyAssignments },
  ];

  for (const step of steps) {
    onProgress(step.key, 'loading', `در حال گرفتن پیش‌نمایش ${entityLabels[step.key]}...`);

    const previewRes = await step.preview();
    if (!previewRes.isSuccess || !previewRes.data) {
      onProgress(step.key, 'error', `خطا در دریافت پیش‌نمایش ${entityLabels[step.key]}`);
      break; // متوقف کردن فرآیند در صورت بروز خطا در مراحل پایه
    }

    onProgress(step.key, 'loading', `در حال اعمال تغییرات ${entityLabels[step.key]}...`);
    const applyRes = await step.apply(previewRes.data);

    if (applyRes.isSuccess) {
      const { addedCount, updatedCount, deletedCount } = applyRes.data;
      onProgress(step.key, 'success', `تکمیل شد (افزوده: ${addedCount}، ویرایش: ${updatedCount}، حذف: ${deletedCount})`);
    } else {
      onProgress(step.key, 'error', `خطا در اعمال تغییرات ${entityLabels[step.key]}`);
      break;
    }
  }
};