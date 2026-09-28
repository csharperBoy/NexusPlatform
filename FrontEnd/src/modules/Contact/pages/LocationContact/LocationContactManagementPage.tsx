// src/modules/HR/pages/locationContact/LocationContactManagementPage.tsx

import React from "react";
import { GenericCrudPage } from "@/core/components/crud/components/GenericCrudPage";
import { GenericColumnDef, GenericCrudApi } from "@/core/components/crud/types";
import { locationContactApi } from "../../api/LocationContactApi";
import { LocationContactInfoView } from "../../models/LocationContactInfoView";
import { UpdateLocationContactCommand } from "../../models/LocationContactCommand";
import { GenericTreeCrudApi, GenericTreeCrudPage, TreeColumnDef, UseGenericTreeCrudOptions } from "@/core/components/treeCrud";

// ─── ستون‌ها ───
const columns: TreeColumnDef<LocationContactInfoView>[] = [
  {
    key: "title",
    label: "عنوان مکان",
    type: "text",
    required: true,
    editable: false,
    excelHeaders: ["عنوان", "عنوان مکان", "title", "نام", "مکان"],
  },
  {
    key: "officePhone",
    label: "تلفن‌های داخلی",
    type: "taginput",
    excelHeaders: [
      "تلفن داخلی",
      "تلفن‌های داخلی",
      "officephone",
      "internalphone",
    ],
  },
  {
    key: "orgMobile",
    label: "موبایل‌های سازمانی",
    type: "taginput",
    excelHeaders: [
      "موبایل سازمانی",
      "موبایل‌های سازمانی",
      "orgmobile",
      "mobile",
    ],
  },
];

// ═══════════════════════════════════════════════════════════════════
//  آداپتور: تطبیق postContactApi با اینترفیس GenericTreeCrudApi
//  نکته: چون این فرم افزودن نداره، create فقط یه no-op هست.
// ═══════════════════════════════════════════════════════════════════
const treeApi: GenericTreeCrudApi<
  LocationContactInfoView,
  never,                     // ← هیچ CreateCommand نداریم
  UpdateLocationContactCommand
> = {
  getList: async () => {
    const data = await locationContactApi.getList();
    // نرمال‌سازی null → []
    return (data as LocationContactInfoView[]).map((p) => ({
      ...p,
      officePhone: p.orgPhone ?? [],
      orgMobile: p.orgMobile ?? [],
    }));
  },

  // اگر UI هیچ‌وقت create صدا نزنه، این هیچ‌وقت اجرا نمی‌شه
  create: async () => {
    throw new Error("Create is not supported for PostContact entity");
  },

  batchUpdate: (cmds) => locationContactApi.batchUpdate(cmds),

  delete: async () => {
    throw new Error("Delete is not supported for PostContact entity");
  },
};
// ─── تنظیمات CRUD ───
const crudOptions: UseGenericTreeCrudOptions<
  LocationContactInfoView,
   never,
  UpdateLocationContactCommand
> = {
  api: treeApi,

  // 👈 حفظ استراتژی آفلاین قبلی
  apiOptions: {
    offlineStrategy: "queueOffline",
  },

  columns,

  // ─── مپینگ ───
  
    mapToUpdateCommand: (post): UpdateLocationContactCommand => ({
      id: post.id,
      officePhone: post.orgPhone ?? [],
      orgMobile: post.orgMobile ?? [],
    }),

  // ─── عنوان نمایشی برای مودال حذف ───
  getDisplayTitle: (loc) => loc.title || `مکان #${loc.id}`,

  // ─── مچ اکسل بر اساس عنوان ───
  excelMatchKey: "title",

  // ─── قابلیت‌ها ───
  tableFeatures: {
    enableSearch: true,
    enableColumnFilter: true,
    enableExcelImport: true,
    enableExcelExport: true,
    enableDelete: false,
    enableDragDrop: false,          // ← جابه‌جایی با درگ
    enableInlineAddChild: false,    // ← + داخل هر سطر
    enableMultiSelect: false,
    enableExpandCollapseAll: true,
    enableStatusColumn: true,
  },
  pageFeatures: {
    enableAddAsRoot: false,         // ← دکمه افزودن ریشه در هدر
  },
};

export const LocationContactManagementPage: React.FC = () => {
   return (
      <GenericTreeCrudPage<
        LocationContactInfoView,
        never,
        UpdateLocationContactCommand
      >
        title="مدیریت اطلاعات تماس مکان‌ها"
        columns={columns}
        crudOptions={crudOptions}
      />
    );
};