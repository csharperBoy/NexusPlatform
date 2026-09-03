import React from "react";
import { GenericCrudPage } from "@/core/components/crud/components/GenericCrudPage";
import { GenericColumnDef, UseGenericCrudOptions  } from "@/core/components/crud/types";
import { employmentApi } from "../../api/EmploymentApi";
import { locationApi } from "../../api/LocationApi";
import { EmploymentInfoView } from "../../models/EmploymentInfoView";
import {
  CreateEmploymentCommand,
  UpdateEmploymentCommand,
} from "../../models/EmploymentCommand";

// تایپ محلی برای پشتیبانی از locationsId در حالت فرانت‌اند
type EmployeeItem = EmploymentInfoView & { locationsId?: string[] };

// ۱. تعریف ستون‌های جدول
const employeeColumns: GenericColumnDef<EmployeeItem>[] = [
  {
    key: "employmentCode",
    label: "کد پرسنلی",
    type: "text",
    required: true,
    dir: "ltr",
    className: "font-mono font-medium",
  },
  {
    key: "nationalCode",
    label: "کد ملی",
    type: "text",
    required: true,
    dir: "ltr",
    className: "font-mono",
  },
  {
    key: "firstName",
    label: "نام",
    type: "text",
    required: true,
  },
  {
    key: "lastName",
    label: "نام خانوادگی",
    type: "text",
    required: true,
  },
  {
    key: "locationsId",
    label: "محل‌های استقرار",
    type: "multi-select",
    selectionKey: "locations",
  },
];

// ۲. تنظیمات هوک CRUD با نگاشت کامل مدل‌ها
const crudOptions: UseGenericCrudOptions<
  EmployeeItem,
  CreateEmploymentCommand,
  UpdateEmploymentCommand
> = {
  api: employmentApi,
columns: employeeColumns,
  selectionApis: {
    locations: locationApi.getSelectionList,
  },

  // کلید تطبیق در فایل اکسل
  excelMatchKey: "employmentCode",

  // استخراج IDهای محل استقرار از لیست locations برمی‌گردد از بک‌اند
  transformApiData: (data) =>
    data.map((emp) => ({
      ...emp,
      locationsId: emp.locationsId || emp.locations?.map((loc) => loc.id) || [],
    })),

  // نگاشت متغیرها به UpdateEmploymentCommand (با رعایت دقیق نام‌گذاری فیلدها)
  mapToUpdateCommand: (entity) => ({
    id: entity.id,
    EmploymentCode: entity.employmentCode,
    nationalCode: entity.nationalCode,
    FirstName: entity.firstName,
    LastName: entity.lastName,
    locationsId: entity.locationsId || [],
  }),

  // نگاشت متغیرها به CreateEmploymentCommand
  mapToCreateCommand: (formData) => ({
    id: "",
    EmploymentCode: formData.employmentCode,
    NationalCode: formData.nationalCode,
    FirstName: formData.firstName,
    LastName: formData.lastName,
    locationsId: formData.locationsId || [],
  }),
pageFeatures:{
enableAdd: true,
},
  tableFeatures: {
    enableExcelImport: true,
    enableExcelExport: true,
    enableSearch: true,
    enableColumnFilter: true,
    enableDelete:true,
  },
};

export default function EmploymentManagementPage() {
  return (
    <GenericCrudPage<EmployeeItem, CreateEmploymentCommand, UpdateEmploymentCommand>
      title="مدیریت کارکنان"
      columns={employeeColumns}
      crudOptions={crudOptions}
    />
  );
}