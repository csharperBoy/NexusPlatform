import React from "react";
import { BaseEntity, GenericCrudApi, GenericColumnDef } from "@/core/components/crud/types";
import { GenericCrudPage } from "@/core/components/crud/components/GenericCrudPage";
import { SelectionListDto } from "@/core/models/SelectionListDto";

// DTOها و Typeها
export interface EmploymentDto extends BaseEntity {
  id: string;
  title: string;
  code: string;
  departmentId: string;
  employmentTypeId: string;
}

export interface CreateEmploymentCmd {
  title: string;
  code: string;
  departmentId: string;
  employmentTypeId: string;
}

export interface UpdateEmploymentCmd {
  id: string;
  title: string;
  code: string;
  departmentId: string;
  employmentTypeId: string;
}

// سرویس API با نام‌گذاری‌های جدید
const employmentApi: GenericCrudApi<EmploymentDto, CreateEmploymentCmd, UpdateEmploymentCmd> = {
  GetSelectionList: async () => [/* ... */],
  GetList: async () => [
    {
      id: "1",
      title: "کارشناس ارشد نرم‌افزار",
      code: "EMP-101",
      departmentId: "d1",
      employmentTypeId: "t1",
    },
  ],
  create: async (cmd) => console.log("Create:", cmd),
  update: async (id, cmd) => console.log("Update:", id, cmd),
  batchUpdate: async (cmds) => console.log("Batch Update:", cmds),
  delete: async (id) => console.log("Delete:", id),
};

// API سرویس‌های جانبی برای Dropdownها
const departmentApi = {
  GetSelectionList: async (): Promise<SelectionListDto[]> => [
    { value: "d1", label: "فناوری اطلاعات", display: "فناوری اطلاعات" },
    { value: "d2", label: "منابع انسانی", display: "منابع انسانی" },
  ],
};

const employmentTypeApi = {
  GetSelectionList: async (): Promise<SelectionListDto[]> => [
    { value: "t1", label: "تمام وقت", display: "تمام وقت" },
    { value: "t2", label: "پاره وقت", display: "پاره وقت" },
  ],
};

export const EmploymentPage = () => {
  // تعریف ستون‌های جدول
  const columns: GenericColumnDef<EmploymentDto>[] = [
    {
      key: "code",
      title: "کد پرسنلی",
      width: "150px",
      type: "text",
      editable: true,
    },
    {
      key: "title",
      title: "عنوان شغل",
      type: "text",
      editable: true,
    },
    {
      key: "departmentId",
      title: "دپارتمان",
      type: "select",
      optionsKey: "departmentId",
      editable: true,
    },
    {
      key: "employmentTypeId",
      title: "نوع همکاری",
      type: "select",
      optionsKey: "employmentTypeId",
      editable: true,
    },
  ];

  return (
    <GenericCrudPage<EmploymentDto, CreateEmploymentCmd, UpdateEmploymentCmd>
      title="مدیریت مشاغل و استخدام"
      columns={columns}
      crudOptions={{
        api: employmentApi,
        selectionApis: {
          departmentId: departmentApi.GetSelectionList,
          employmentTypeId: employmentTypeApi.GetSelectionList,
        },
        mapToUpdateCommand: (entity) => ({
          id: entity.id,
          title: entity.title,
          code: entity.code,
          departmentId: entity.departmentId,
          employmentTypeId: entity.employmentTypeId,
        }),
        features: {
          enableAdd: true,
          enableDelete: true,
          enableBatchSave: true,
          enableGlobalSearch: true,
          enableExcelImport: true,
          excelMapper: (row) => ({
            title: row["عنوان شغل"] || "",
            code: String(row["کد پرسنلی"] || ""),
            departmentId: "d1",
            employmentTypeId: "t1",
          }),
        },
      }}
    />
  );
};