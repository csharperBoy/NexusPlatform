// src/modules/Contact/pages/ContactResource/ContactResourceManagementPage.tsx
import React from "react";
import {
  GenericTreeCrudPage,
  TreeColumnDef,
  UseGenericTreeCrudOptions,
} from "@/core/components/treeCrud";
import { SelectionListDto } from "@/core/models/SelectionListDto";
import { ContactResourceApi } from "../../api/ContactResourceApi";
import { ContactResourceInfoView } from "../../models/ContactResourceInfoView";
import {
  ContactRelationTypeEnum,
  ContactTypeEnum,
  CreateContactResourceCommand,
  UpdateContactResourceCommand,
} from "../../models/ContactResourceCommand";

// ═══════════════════════════════════════════════════════════════════
//  نگاشت enumها به SelectionListDto
// ═══════════════════════════════════════════════════════════════════
const contactTypeLabels: Record<ContactTypeEnum, string> = {
  [ContactTypeEnum.Mobile]: "موبایل",
  [ContactTypeEnum.Phone]: "تلفن",
  [ContactTypeEnum.OfficePhone]: "تلفن داخلی",
  [ContactTypeEnum.OrganizationMobile]: "موبایل سازمانی",
  [ContactTypeEnum.Email]: "ایمیل",
  [ContactTypeEnum.Fax]: "فکس",
  [ContactTypeEnum.Website]: "وب‌سایت",
  [ContactTypeEnum.WhatsApp]: "واتس‌اپ",
  [ContactTypeEnum.Instagram]: "اینستاگرام",
  [ContactTypeEnum.Telegram]: "تلگرام",
  [ContactTypeEnum.LinkedIn]: "لینکدین",
  [ContactTypeEnum.Address]: "آدرس",
  [ContactTypeEnum.PostalCode]: "کد پستی",
  [ContactTypeEnum.Eitaa]: "ایتا",
  [ContactTypeEnum.x]: "X",
  [ContactTypeEnum.Other]: "سایر",
};

const contactTypeOptions: SelectionListDto[] = (
  Object.keys(contactTypeLabels) as unknown as ContactTypeEnum[]
).map((key) => {
  const numKey = Number(key) as ContactTypeEnum;
  const label = contactTypeLabels[numKey];
  return { value: String(numKey), label, display: label };
});

const contactRelationLabels: Record<ContactRelationTypeEnum, string> = {
  [ContactRelationTypeEnum.PublicPhoneNumber]: "شماره همگانی",
  [ContactRelationTypeEnum.ExtensionOf]: "داخلی متصل به خط اصلی",
  [ContactRelationTypeEnum.Alternative]: "کانال جایگزین",
};

const relationTypeOptions: SelectionListDto[] = (
  Object.keys(contactRelationLabels) as unknown as ContactRelationTypeEnum[]
).map((key) => {
  const numKey = Number(key) as ContactRelationTypeEnum;
  const label = contactRelationLabels[numKey];
  return { value: String(numKey), label, display: label };
});

// ═══════════════════════════════════════════════════════════════════
//  ستون‌ها
// ═══════════════════════════════════════════════════════════════════
const columns: TreeColumnDef<ContactResourceInfoView>[] = [
  {
    key: "contactType",
    label: "نوع",
    type: "select",
    required: true,
    staticOptions: contactTypeOptions,
    valueType: "number",        // ← NEW
    excelHeaders: ["نوع", "نوع تماس", "contacttype", "type"],
  },
  {
    key: "value",
    label: "مقدار",
    type: "text",
    required: true,
    dir: "ltr",
    className: "font-mono",
    excelHeaders: ["مقدار", "value"],
  },
  {
    key: "label",
    label: "برچسب",
    type: "text",
    excelHeaders: ["برچسب", "label", "عنوان"],
  },
  // {
  //   key: "isPrimary",
  //   label: "اصلی",
  //   type: "boolean",
  //   valueType: "boolean",       // ← NEW (برای اطمینان)
  //   excelHeaders: ["اصلی", "isprimary", "primary"],
  // },
  {
    key: "sortOrder",
    label: "ترتیب",
    type: "number",
    valueType: "number",        // ← NEW
    excelHeaders: ["ترتیب", "sortorder", "order"],
  },
  // {
  //   key: "relationType",
  //   label: "نوع ارتباط",
  //   type: "select",
  //   staticOptions: relationTypeOptions,
  //   valueType: "number",        // ← NEW
  //   excelHeaders: ["نوع ارتباط", "relationtype", "relation"],
  // },
];

// ═══════════════════════════════════════════════════════════════════
//  تنظیمات CRUD
// ═══════════════════════════════════════════════════════════════════
const crudOptions: UseGenericTreeCrudOptions<
  ContactResourceInfoView,
  CreateContactResourceCommand,
  UpdateContactResourceCommand
> = {
  api: ContactResourceApi,

  apiOptions: {
    offlineStrategy: "queueOffline",
  },

  columns,

  // ⭐ محدودیت: حداکثر دو سطح (ریشه + یک سطح فرزند، بدون نوه)
  maxDepth: 2,

  // ─── مپینگ ───
  
  // ─── Update: مقادیر مشتق ───
  mapToUpdateCommand: (item): UpdateContactResourceCommand => ({
    id: item.id,
    value: item.value,
    label: item.label ?? null,
    contactType: item.contactType,
    isPrimary: item.parentId == null,   // ← مشتق: ریشه true، فرزند false
    sortOrder: item.sortOrder ?? null,
    relationType: null,                  // ← همیشه null
    parentId: item.parentId ?? null,
  }),

  // ─── Create: مقادیر مشتق ───
  mapToCreateCommand: (formData, parentId): CreateContactResourceCommand => ({
    value: formData.value || "",
    label: formData.label || null,
    contactType: formData.contactType ?? ContactTypeEnum.Phone,
    isPrimary: parentId == null,         // ← مشتق از parentId
    sortOrder: formData.sortOrder ?? null,
    relationType: null,                  // ← همیشه null
    parentId,
  }),


  // ─── مقادیر پیش‌فرض رکورد جدید ───
  getCreateDefaults: () => ({
    contactType: ContactTypeEnum.Phone,
    isPrimary: false,
    value: "",
    label: "",
    sortOrder: null,
    relationType: null,
  }),

  // ─── عنوان نمایشی مودال حذف ───
  getDisplayTitle: (item) =>
    `${contactTypeLabels[item.contactType] || "تماس"}: ${item.value || item.id}`,

  // ─── مچ اکسل بر اساس مقدار ───
  excelMatchKey: "value",

  tableFeatures: {
    enableSearch: true,
    enableColumnFilter: true,
    enableExcelImport: false,
    enableExcelExport: false,
    enableDelete: true,
    enableDragDrop: true,
    enableInlineAddChild: true,
    enableMultiSelect: true,
    enableExpandCollapseAll: true,
    enableStatusColumn: true,
  },
  pageFeatures: {
    enableAddAsRoot: true,
  },
};

export default function ContactResourceManagementPage() {
  return (
    <GenericTreeCrudPage<
      ContactResourceInfoView,
      CreateContactResourceCommand,
      UpdateContactResourceCommand
    >
      title="مدیریت منابع تماس"
      columns={columns}
      crudOptions={crudOptions}
    />
  );
}