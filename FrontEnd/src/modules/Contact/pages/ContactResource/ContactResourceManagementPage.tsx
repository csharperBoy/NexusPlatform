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
  {
    key: "isPrimary",
    label: "اصلی",
    type: "boolean",
    excelHeaders: ["اصلی", "isprimary", "primary"],
  },
  {
    key: "sortOrder",
    label: "ترتیب",
    type: "number",
    excelHeaders: ["ترتیب", "sortorder", "order"],
  },
  {
    key: "relationType",
    label: "نوع ارتباط",
    type: "select",
    staticOptions: relationTypeOptions,
    excelHeaders: ["نوع ارتباط", "relationtype", "relation"],
  },
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
  mapToUpdateCommand: (item): UpdateContactResourceCommand => ({
    id: item.id,
    value: item.value,
    label: item.label ?? null,
    ContactType: item.contactType,
    IsPrimary: item.isPrimary,
    SortOrder: item.sortOrder ?? null,
    RelationType: item.relationType ?? null,
    parentId: item.parentId ?? null,
  }),

  mapToCreateCommand: (formData, parentId): CreateContactResourceCommand => ({
    value: formData.value || "",
    label: formData.label || null,
    ContactType: formData.contactType ?? ContactTypeEnum.Phone,
    IsPrimary: formData.isPrimary ?? false,
    SortOrder: formData.sortOrder ?? null,
    RelationType: formData.relationType ?? null,
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
    enableExcelImport: true,
    enableExcelExport: true,
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