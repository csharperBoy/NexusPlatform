// src/modules/HR/api/mockSyncApi.ts
import { BatchResult } from "@/core/models/apiResults";
import { SyncCommandBundle, SyncResult } from "../models/SyncModels";

// تابع کمکی برای ایجاد تاخیر شبکه
const delay = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

export const mockSyncApi = {
  // ===================== Preview Mocks =====================

  SyncOrganizationUnitPreview: async (): Promise<BatchResult<SyncCommandBundle>> => {
    await delay(1000);
    return {
      succeeded: true,
      errors: [],
      successMessages: [],
      data: {
        addCommands: [
          {
            id: "1001",
            summary: "افزودن واحد سازمانی ریشه جدید 'مدیریت فناوری اطلاعات'",
            command: { name: "مدیریت فناوری اطلاعات", parentId: null }
          },
          {
            id: "1002",
            summary: "افزودن واحد سازمانی 'دپارتمان توسعه نرم‌افزار'",
            command: { name: "دپارتمان توسعه نرم‌افزار", parentId: "1001" }
          }
        ],
        updateCommands: [
          {
            id: "1003",
            summary: "تغییر نام واحد سازمانی از 'ارتباطات' به 'روابط عمومی و بین‌الملل'",
            command: { id: "unit-55", name: "روابط عمومی و بین‌الملل", parentId: null }
          }
        ],
        deleteCommands: [],
        warnings: []
      }
    };
  },

  SyncJobLevelPreview: async (): Promise<BatchResult<SyncCommandBundle>> => {
    await delay(800);
    return {
      succeeded : true,
      errors: [],
      successMessages: [],
      data: {
        addCommands: [
          {
            id: "2001",
            summary: "افزودن سطح شغلی جدید 'ارشد / Senior'",
            command: { title: "ارشد / Senior", code: "JL-04" }
          }
        ],
        updateCommands: [],
        deleteCommands: [],
        warnings: ["سطح شغلی 'کارآموز' در سیستم مرجع غیرفعال شده است."]
      }
    };
  },

  SyncJobTitlePreview: async (): Promise<BatchResult<SyncCommandBundle>> => {
    await delay(900);
    return {
      succeeded: true,
      errors: [],
      successMessages: [],
      data: {
        addCommands: [
          {
            id: "3001",
            summary: "افزودن عنوان شغلی جدید 'توسعه‌دهنده ارشد Frontend'",
            command: { title: "توسعه‌دهنده ارشد Frontend" }
          },
          {
            id: "3002",
            summary: "افزودن عنوان شغلی جدید 'راهبر سیستم‌های DevOps'",
            command: { title: "راهبر سیستم‌های DevOps" }
          }
        ],
        updateCommands: [
          {
            id: "3003",
            summary: "تغییر عنوان شغلی از 'برنامه‌نویس' به 'مهندس نرم‌افزار'",
            command: { id: "jt-12", title: "مهندس نرم‌افزار" }
          }
        ],
        deleteCommands: [],
        warnings: []
      }
    };
  },

  SyncEmploymentsPreview: async (): Promise<BatchResult<SyncCommandBundle>> => {
    await delay(1200);
    return {
      succeeded: true,
      errors: [],
      successMessages: [],
      data: {
        addCommands: [
          {
            id: "4001",
            summary: "افزودن پرسنل جدید 'علی محمدی' (کد پرسنلی: 98012)",
            command: { personalCode: "98012", firstName: "علی", lastName: "محمدی" }
          }
        ],
        updateCommands: [
          {
            id: "4002",
            summary: "بروزرسانی شماره تماس و ایمیل 'مریم رضایی'",
            command: { personalCode: "95104", mobile: "09123456789" }
          }
        ],
        deleteCommands: [],
        warnings: []
      }
    };
  },

  SyncPostPreview: async (): Promise<BatchResult<SyncCommandBundle>> => {
    await delay(1100);
    return {
      succeeded: true,
      errors: [],
      successMessages: [],
      data: {
        addCommands: [
          {
            id: "5001",
            summary: "افزودن پست سازمانی 'سرپرست تیم UI/UX'",
            command: { postTitle: "سرپرست تیم UI/UX", orgUnitId: "unit-10" }
          }
        ],
        updateCommands: [],
        deleteCommands: [],
        warnings: []
      }
    };
  },

  SyncAssignmentsPreview: async (): Promise<BatchResult<SyncCommandBundle>> => {
    await delay(1000);
    return {
      succeeded: true,
      errors: [],
      successMessages: [],
      data: {
        addCommands: [
          {
            id: "6001",
            summary: "انتصاب 'علی محمدی' به پست 'سرپرست تیم UI/UX'",
            command: { personalCode: "98012", postId: "post-88" }
          }
        ],
        updateCommands: [],
        deleteCommands: [],
        warnings: []
      }
    };
  },

  // ===================== Apply Mocks =====================

  ApplyGeneric: async (bundle: SyncCommandBundle): Promise<BatchResult<SyncResult>> => {
    await delay(1500);
    return {
      succeeded: true,
      errors: [],
      successMessages: [
        `با موفقیت ${bundle.addCommands.length} مورد ایجاد شد.`,
        `با موفقیت ${bundle.updateCommands.length} مورد ویرایش شد.`,
        `با موفقیت ${bundle.deleteCommands.length} مورد حذف شد.`
      ],
      data: {
        addedCount: bundle.addCommands.length,
        updatedCount: bundle.updateCommands.length,
        deletedCount: bundle.deleteCommands.length
      }
    };
  }
};