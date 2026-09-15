//src/modules/HR/pages/Location/LocationManagementPage.tsx
import React from "react";
import { GenericCrudPage } from "@/core/components/crud/components/GenericCrudPage";
import { GenericColumnDef, UseGenericCrudOptions } from "@/core/components/crud/types";
import { locationApi } from "../../api/LocationApi";
import { LocationInfoView } from "../../models/LocationInfoView";
import { CreateLocationCommand, UpdateLocationCommand } from "../../models/LocationCommand";

// ۱. تعریف ستون‌ها مطابق با GenericColumnDef
const columns: GenericColumnDef<LocationInfoView>[] = [
  {
    key: "title",
    label: "عنوان",
    type: "text",
    required: true,
    editable: true,
  },
];

// ۲. تنظیمات CRUD همگام با استراتژی جدید آفلاین
const crudOptions: UseGenericCrudOptions<
  LocationInfoView,
  CreateLocationCommand,
  UpdateLocationCommand
> = {
  api: locationApi,
  // 👈 فعال‌سازی استراتژی ذخیره در صف آفلاین و کش‌سازی برای این صفحه
  apiOptions: {
    offlineStrategy: "queueOffline"
  },
  columns: columns,
  mapToUpdateCommand: (entity) => ({
    id: entity.id,
    title: entity.title || null,
  }),

  mapToCreateCommand: (formData) => ({
    title: formData.title || "",
  }),

  pageFeatures: {
    enableAdd: true,
  },

  tableFeatures: {
    enableSearch: true,
    enableColumnFilter: true,
    enableExcelImport: true,
    enableExcelExport: true,
    enableDelete: true,
  },
};

export const LocationManagementPage: React.FC = () => {
  return (
    <GenericCrudPage<LocationInfoView, CreateLocationCommand, UpdateLocationCommand>
      title="مدیریت اطلاعات ارتباطی مکان‌ها"
      columns={columns}
      crudOptions={crudOptions}
    />
  );
};

export default LocationManagementPage;