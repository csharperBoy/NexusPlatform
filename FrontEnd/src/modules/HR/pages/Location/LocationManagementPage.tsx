import React from "react";
import { GenericCrudPage } from "@/core/components/crud/components/GenericCrudPage";
import { GenericColumnDef, UseGenericCrudOptions } from "@/core/components/crud/types";
import { locationApi } from "../../api/LocationApi";
import { LocationInfoView } from "../../models/LocationInfoView";
import { CreateLocationCommand, UpdateLocationCommand } from "../../models/LocationCommand";

// ۱. تعریف ستون‌ها مطابق با GenericColumnDef (تغییر title به label)
const columns: GenericColumnDef<LocationInfoView>[] = [
  {
    key: "title",
    label: "عنوان",
    type: "text",
    required: true,
    editable: true,
  },
];

// ۲. تنظیمات CRUD همگام با تایپ‌های جدید
const crudOptions: UseGenericCrudOptions<
  LocationInfoView,
  CreateLocationCommand,
  UpdateLocationCommand
> = {
  api: locationApi,
columns: columns,
  mapToUpdateCommand: (entity) => ({
    id: entity.id,
    title: entity.title || null,
  }),

  mapToCreateCommand: (formData) => ({
    title: formData.title || "",
  }),

  features: {
    enableSearch: true,
    enableColumnFilter: true,
    enableExcelImport: true,
    enableExcelExport: true,
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