// src/modules/HR/pages/location/LocationManagementPage.tsx

import React from "react";
import { GenericCrudPage } from "@/core/components/crud/components/GenericCrudPage";
import { GenericColumnDef, GenericCrudApi } from "@/core/components/crud/types";
import { locationApi } from "../../api/LocationApi";
import { LocationInfoView } from "../../models/LocationInfoView";
import { CreateLocationCommand, UpdateLocationCommand } from "../../models/LocationCommand";

// تعریف ستون‌های جدول مکان‌ها
const columns: GenericColumnDef<LocationInfoView>[] = [
  {
    key: "title",
    title: "عنوان",
    editable: true,
    type: "text",
    searchable: true,
  },
];

export const LocationManagementPage: React.FC = () => {
  return (
    <GenericCrudPage<LocationInfoView, CreateLocationCommand, UpdateLocationCommand>
      title="مدیریت اطلاعات ارتباطی مکان‌ها"
      columns={columns}
      crudOptions={{
        // کست کردن API جهت هماهنگی کامل امضای متد update با اینترفیس عمومی
        api: locationApi as unknown as GenericCrudApi<LocationInfoView, CreateLocationCommand, UpdateLocationCommand>,
        
        // نگاشت رکورد جدول به DTO ویرایش گروهی
        mapToUpdateCommand: (entity) => ({
          id: entity.id,
          title: entity.title || null,
        }),
        
        features: {
          enableAdd: true,
          enableDelete: true,
          enableBatchSave: true,
          enableGlobalSearch: true,
        },
      }}
    />
  );
};

export default LocationManagementPage;