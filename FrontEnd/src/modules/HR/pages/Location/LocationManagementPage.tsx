// src/modules/HR/pages/Location/LocationManagementPage.tsx
import React from "react";
import {
  GenericTreeCrudPage,
  TreeColumnDef,
  UseGenericTreeCrudOptions,
} from "@/core/components/treeCrud";
import { locationApi } from "../../api/LocationApi";
import { LocationInfoView } from "../../models/LocationInfoView";
import {
  CreateLocationCommand,
  UpdateLocationCommand,
} from "../../models/LocationCommand";

// ─── ستون‌ها ───
const columns: TreeColumnDef<LocationInfoView>[] = [
  {
    key: "title",
    label: "عنوان مکان",
    type: "text",
    required: true,
    editable: true,
    excelHeaders: ["عنوان", "عنوان مکان", "title", "نام", "مکان"],
  },
];

// ─── تنظیمات CRUD ───
const crudOptions: UseGenericTreeCrudOptions<
  LocationInfoView,
  CreateLocationCommand,
  UpdateLocationCommand
> = {
  api: locationApi,

  // 👈 حفظ استراتژی آفلاین قبلی
  apiOptions: {
    offlineStrategy: "queueOffline",
  },

  columns,

  // ─── مپینگ ───
  mapToUpdateCommand: (entity): UpdateLocationCommand => ({
    id: entity.id,
    title: entity.title || null,
    parentId: entity.parentId ?? null,     // ← ترجمه‌ی parentId به Command
  }),

  mapToCreateCommand: (formData, parentId): CreateLocationCommand => ({
    title: formData.title || "",
    parentId,                              // ← از هوک میاد (null = ریشه)
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
    enableDelete: true,
    enableDragDrop: true,          // ← جابه‌جایی با درگ
    enableInlineAddChild: true,    // ← + داخل هر سطر
    enableMultiSelect: true,
    enableExpandCollapseAll: true,
    enableStatusColumn: true,
  },
  pageFeatures: {
    enableAddAsRoot: true,         // ← دکمه افزودن ریشه در هدر
  },
};

export const LocationManagementPage: React.FC = () => {
  return (
    <GenericTreeCrudPage<
      LocationInfoView,
      CreateLocationCommand,
      UpdateLocationCommand
    >
      title="مدیریت ساختار مکان‌ها"
      columns={columns}
      crudOptions={crudOptions}
    />
  );
};

export default LocationManagementPage;