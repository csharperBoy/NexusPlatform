// src/modules/HR/pages/locationContact/LocationContactManagementPage.tsx

import React from "react";
import { GenericCrudPage } from "@/core/components/crud/components/GenericCrudPage";
import { GenericColumnDef, GenericCrudApi } from "@/core/components/crud/types";
import { locationContactApi } from "../../api/LocationContactApi";
import { LocationContactInfoView } from "../../models/LocationContactInfoView";
import { UpdateLocationContactCommand } from "../../models/LocationContactCommand";

// ۱. تعریف ستون‌های جدول
const columns: GenericColumnDef<LocationContactInfoView>[] = [
  {
    key: "title",
    label: "عنوان واحد",
    editable: false, // عنوان فقط خواندنی است
  },
  {
    key: "orgPhone",
    label: "شماره‌های تلفن ثابت",
    type: "taginput",
    editable: true,
  },
  {
    key: "orgMobile",
    label: "شماره‌های همراه",
    type: "taginput",
    editable: true,
  },
];

// ۲. آداپتور API برای تطبیق متدهای اختصاصی با GenericCrudApi
const crudApiAdapter: GenericCrudApi<
  LocationContactInfoView,
  void,
  UpdateLocationContactCommand
> = {
  getList: locationContactApi.GetList,
  batchUpdate: locationContactApi.batchUpdate,
  create: async () => Promise.reject("امکان ایجاد واحد جدید در این صفحه وجود ندارد."),
  delete: async () => Promise.reject("امکان حذف واحد در این صفحه وجود ندارد."),
};

export const LocationContactManagementPage: React.FC = () => {
  return (
    <GenericCrudPage<LocationContactInfoView, void, UpdateLocationContactCommand>
      title="مدیریت شماره‌های تماس واحدها"
      columns={columns}
      crudOptions={{
        api: crudApiAdapter,
        columns: columns,
        // تبدیل مدل UI به Command مورد نیاز برای API (نگاشت orgPhone به officePhone)
        mapToUpdateCommand: (entity) => ({
          id: entity.id,
          officePhone: entity.orgPhone ?? [],
          orgMobile: entity.orgMobile ?? [],
        }),
          pageFeatures:{
          enableAdd:false

        },
        tableFeatures: {
          enableDelete: false,
          enableSearch: true,
          enableColumnFilter: false,
        },
      }}
    />
  );
};