// src/modules/HR/pages/PostContact/PostContactManagementPage.tsx
import React from "react";
import {
  GenericTreeCrudPage,
  TreeColumnDef,
  UseGenericTreeCrudOptions,
  GenericTreeCrudApi,
} from "@/core/components/treeCrud";
import { postContactApi } from "../../api/PostContactApi";
import { PostContactInfoView } from "../../models/postContactInfoView";
import { UpdatePostContactCommand } from "../../models/postContactCommand";

// ═══════════════════════════════════════════════════════════════════
//  آداپتور: تطبیق postContactApi با اینترفیس GenericTreeCrudApi
//  نکته: چون این فرم افزودن نداره، create فقط یه no-op هست.
// ═══════════════════════════════════════════════════════════════════
const treeApi: GenericTreeCrudApi<
  PostContactInfoView,
  never,                     // ← هیچ CreateCommand نداریم
  UpdatePostContactCommand
> = {
  getList: async () => {
    const data = await postContactApi.GetList();
    // نرمال‌سازی null → []
    return (data as PostContactInfoView[]).map((p) => ({
      ...p,
      officePhone: p.officePhone ?? [],
      orgMobile: p.orgMobile ?? [],
    }));
  },

  // اگر UI هیچ‌وقت create صدا نزنه، این هیچ‌وقت اجرا نمی‌شه
  create: async () => {
    throw new Error("Create is not supported for PostContact entity");
  },

  batchUpdate: (cmds) => postContactApi.batchUpdate(cmds),

  delete: async () => {
    throw new Error("Delete is not supported for PostContact entity");
  },
};

// ─── ستون‌ها ───
const columns: TreeColumnDef<PostContactInfoView>[] = [
  {
    key: "jobTitleName",
    label: "عنوان شغل",
    editable: false,
    className: "font-medium text-gray-800",
    // برای این‌که سرچ روی postCode هم کار کنه
    getFilterValue: (n) =>
      `${n.jobTitleName || ""} ${n.postCode || ""}`.trim(),
    render: (_, node) => (
      <span>
        {node.jobTitleName || "بدون عنوان شغل"}{" "}
        {node.postCode && (
          <span className="text-gray-500 text-xs font-mono font-normal">
            ({node.postCode})
          </span>
        )}
      </span>
    ),
  },
  {
    key: "organizationUnitsName",
    label: "واحد سازمانی",
    editable: false,
    className: "text-gray-600 text-xs",
    getFilterValue: (n) => n.organizationUnitsName || "",
  },
  {
    key: "occupant",                         // کلید مجازی
    label: "شاغل فعلی",
    editable: false,
    getFilterValue: (n) =>
      `${n.firstName || ""} ${n.lastName || ""} ${n.employmentCode || ""}`.trim(),
    render: (_, node) => {
      const name =
        node.firstName || node.lastName
          ? `${node.firstName || ""} ${node.lastName || ""}`.trim()
          : "-";
      return (
        <div className="flex flex-col text-xs">
          <span className="font-medium text-gray-700">{name}</span>
          {node.employmentCode && (
            <span className="text-[10px] text-gray-400 font-mono">
              کد: {node.employmentCode}
            </span>
          )}
        </div>
      );
    },
  },
  {
    key: "jobLevelTitle",
    label: "سطح شغلی",
    editable: false,
    className: "text-gray-500 text-xs",
    getFilterValue: (n) =>
      `${n.jobLevelTitle || ""} ${n.gradeTitle || ""}`.trim(),
    render: (_, node) => {
      if (!node.jobLevelTitle && !node.gradeTitle) return <span>-</span>;
      return (
        <span>
          {node.jobLevelTitle || ""}
          {node.gradeTitle ? ` (${node.gradeTitle})` : ""}
        </span>
      );
    },
  },
  {
    key: "officePhone",
    label: "تلفن‌های داخلی",
    type: "taginput",
    excelHeaders: [
      "تلفن داخلی",
      "تلفن‌های داخلی",
      "officephone",
      "internalphone",
    ],
  },
  {
    key: "orgMobile",
    label: "موبایل‌های سازمانی",
    type: "taginput",
    excelHeaders: [
      "موبایل سازمانی",
      "موبایل‌های سازمانی",
      "orgmobile",
      "mobile",
    ],
  },
];

// ─── تنظیمات CRUD ───
const crudOptions: UseGenericTreeCrudOptions<
  PostContactInfoView,
  never,
  UpdatePostContactCommand
> = {
  api: treeApi,
  columns,
  excelMatchKey: "postCode",       // ⚠️ اگه کد پرسنلی ملاک مچ‌شدنه، این رو عوض کن

  mapToUpdateCommand: (post): UpdatePostContactCommand => ({
    id: post.id,
    officePhone: post.officePhone ?? [],
    orgMobile: post.orgMobile ?? [],
  }),

  // هیچ‌وقت اجرا نمی‌شه، چون افزودن نداریم
  // mapToCreateCommand لازم نیست

  getDisplayTitle: (post) =>
    `${post.jobTitleName || "پست"} (${post.postCode || post.id})`,

  tableFeatures: {
    enableExcelImport: true,
    enableSearch: true,
    enableColumnFilter: true,
    enableStatusColumn: true,
    enableExpandCollapseAll: true,

    enableDragDrop: false,          // درگ‌اند‌دراپ خاموش
    enableInlineAddChild: false,    // افزودن فرزند خاموش
    enableDelete: false,            // حذف خاموش
    enableMultiSelect: false,       // انتخاب چندگانه خاموش
  },
  pageFeatures: {
    enableAddAsRoot: false,         // افزودن ریشه خاموش
  },
};

export default function PostContactManagementPage() {
  return (
    <GenericTreeCrudPage<
      PostContactInfoView,
      never,
      UpdatePostContactCommand
    >
      title="مدیریت اطلاعات تماس پست‌ها"
      columns={columns}
      crudOptions={crudOptions}
    />
  );
}