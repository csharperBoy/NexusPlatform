// src/modules/HR/pages/employmentContact/EmploymentContactManagementPage.tsx

import React from "react";
import { GenericCrudPage } from "@/core/components/crud/components/GenericCrudPage";
import { GenericColumnDef, GenericCrudApi } from "@/core/components/crud/types";
import { employmentContactApi } from "../../api/EmploymentContactApi";
import { EmploymentContactInfoView } from "../../models/EmploymentContactInfoView";
import { UpdateEmploymentContactCommand } from "../../models/EmploymentContactCommand";

// ۱. تعریف ستون‌های جدول
const columns: GenericColumnDef<EmploymentContactInfoView>[] = [
  {
    key: "employmentCode",
    label: "کد پرسنلی",
    editable: false,
  },
  {
    key: "fullName",
    label: "نام و نام خانوادگی",
    editable: false,
    // استفاده از هر دو پارامتر (مقدار سلول و کل آبجکت ردیف) و اعمال Optional Chaining (?)
    render: (value, entity) => {
      // اگر GenericCrudPage ردیف را به عنوان آرگومان اول پاس می‌دهد، entity در واقع همان value خواهد بود
      const row = entity || value; 
      
      if (!row) return "-";
      
      return `${row?.firstName || ""} ${row?.lastName || ""}`.trim() || "-";
    },
  },
  {
    key: "nationalCode",
    label: "کد ملی",
    editable: false,
  },
  {
    key: "employmentContactPhone",
    label: "تلفن داخلی",
    type: "taginput",
    editable: true,
  },
  {
    key: "employmentContactMobile",
    label: "موبایل سازمانی",
    type: "taginput",
    editable: true,
  },
];

// ۲. آداپتور API برای تطبیق متدها
const crudApiAdapter: GenericCrudApi<
  EmploymentContactInfoView,
  void,
  UpdateEmploymentContactCommand
> = {
  getList: employmentContactApi.GetList,
  batchUpdate: employmentContactApi.batchUpdate,
  create: async () => Promise.reject("امکان ایجاد کارمند جدید در این صفحه وجود ندارد."),
  delete: async () => Promise.reject("امکان حذف کارمند در این صفحه وجود ندارد.")
};

export const EmploymentContactManagementPage: React.FC = () => {
  return (
    <GenericCrudPage<
      EmploymentContactInfoView,
      void,
      UpdateEmploymentContactCommand
    >
      title="مدیریت اطلاعات تماس کارمندان"
      columns={columns}
      crudOptions={{
        api: crudApiAdapter,
        columns: columns,
        // تبدیل مدل UI به Command مورد نیاز جهت ارسال به سرور
        mapToUpdateCommand: (entity) => ({
          id: entity.id,
          employmentContactPhone: entity.employmentContactPhone ?? [],
          employmentContactMobile: entity.employmentContactMobile ?? [],
        }),
        pageFeatures:{
          enableAdd:false

        },
        tableFeatures: {
          enableDelete: false,
          enableSearch: true,          
          enableColumnFilter: true, // جستجوی مجزای ستونی
          enableExcelImport: true,  // فعال‌سازی دکمه بارگذاری از اکسل
        },
      }}
    />
  );
};

export default EmploymentContactManagementPage;