// src/modules/HR/pages/Post/PostManagementPage.tsx
import React from "react";

import { postApi } from "../../api/PostApi";
import { locationApi } from "../../api/LocationApi";
import { employmentApi } from "../../api/EmploymentApi";
import { PostInfoDto } from "../../models/postInfoDto";
import {
  CreatePostCommand,
  UpdatePostCommand,
} from "../../models/postCommand";
import {
  GenericTreeCrudPage,
  TreeColumnDef,
  UseGenericTreeCrudOptions,
  GenericTreeCrudApi,
} from "@/core/components/treeCrud";

// ═══════════════════════════════════════════════════════════════════
//  آداپتور: تطبیق postApi با اینترفیس GenericTreeCrudApi
// ═══════════════════════════════════════════════════════════════════
const treeApi: GenericTreeCrudApi<
  PostInfoDto,
  CreatePostCommand,
  UpdatePostCommand
> = {
  getList: async () => {
    const data = await postApi.getList();
    // PostInfoView → PostInfoDto  (locations رو به [] نرمال می‌کنیم)
    return (data as unknown as PostInfoDto[]).map((p) => ({
      ...p,
      locations: (p as any).locations ?? [],
    }));
  },

  create: (cmd) => postApi.create(cmd),

  batchUpdate: (cmds) => postApi.batchUpdate(cmds),

  delete: (id) => postApi.delete(String(id)),
};

// ─── ستون‌ها ───
const columns: TreeColumnDef<PostInfoDto>[] = [
  {
    key: "fkJobTitleId",
    label: "عنوان شغل",
    type: "select",
    selectionKey: "jobTitles",
    required: true,
    excelHeaders: ["عنوان شغل", "jobtitle"],
  },
  {
    key: "fkOrganizationUnitId",
    label: "واحد سازمانی",
    type: "select",
    selectionKey: "orgUnits",
    excelHeaders: ["واحد سازمانی", "واحد"],
  },
  {
    key: "employmentId",
    label: "شاغل فعلی",
    type: "select",
    selectionKey: "employments",
    excelHeaders: ["شاغل", "شاغل فعلی"],
  },
  {
    key: "fkJobLevelId",
    label: "سطح شغلی",
    type: "select",
    selectionKey: "jobLevels",
    excelHeaders: ["سطح شغلی", "سطح"],
  },
  {
    key: "locations",
    label: "محل استقرار",
    type: "multi-select",
    selectionKey: "locations",
    excelHeaders: ["محل استقرار", "مکان", "location"],
  },
];

// ─── تنظیمات CRUD ───
const crudOptions: UseGenericTreeCrudOptions<
  PostInfoDto,
  CreatePostCommand,
  UpdatePostCommand
> = {
  api: treeApi,                 // ← آداپتور
  columns,

  selectionApis: {
    locations: locationApi.getSelectionList,
    employments: employmentApi.getSelectionList,
    jobTitles: postApi.GetJobTitleSelectionList,
    orgUnits: postApi.GetOrganizationUnitSelectionList,
    jobLevels: postApi.GetJobLevelSelectionList,
    grades: postApi.GetGradeSelectionList,
  },

  excelMatchKey: "employmentCode",

  mapToUpdateCommand: (post) => ({
    id: post.id,
    code: post.postCode,
    organizationUnitId: post.fkOrganizationUnitId,
    jobTitleId: post.fkJobTitleId,
    jobLevelId: post.fkJobLevelId,
    gradeId: post.fkGradeId,
    costCenterId: post.fkCostCenterId,
    reportsToPostId: post.parentId,
    officePhone: post.officePhone,
    orgEmail: post.orgEmail,
    orgMobile: post.orgMobile,
    assignType: post.assignmentsAssigneeType,
    isActive: true,
    employmentId: post.employmentId,
    locationsId: post.locations?.map((l) => l.id) || [],
  }),

  // ✅ مپ صریح به جای spread
  mapToCreateCommand: (formData, parentId): CreatePostCommand => ({
    code: formData.postCode || "",
    organizationUnitId: formData.fkOrganizationUnitId ?? null,
    jobTitleId: formData.fkJobTitleId ?? null,
    jobLevelId: formData.fkJobLevelId ?? null,
    gradeId: formData.fkGradeId ?? null,
    costCenterId: formData.fkCostCenterId ?? null,
    reportsToPostId: parentId,
    officePhone: formData.officePhone || "",
    orgEmail: formData.orgEmail || "",
    orgMobile: formData.orgMobile || "",
    assignType: formData.assignmentsAssigneeType ?? null,
    isActive: true,
    employmentId: formData.employmentId ?? null   
  }),

  getDisplayTitle: (post) => `پست ${post.postCode || post.id}`,

  tableFeatures: {
    enableExcelImport: false,
    enableDragDrop: true,
    enableInlineAddChild: false,
    enableMultiSelect: true,
    enableColumnFilter: true,
    enableStatusColumn: true,
    enableDelete: true,
  },
  pageFeatures: {
    enableAddAsRoot: false,
  },
};

export default function PostManagementPage() {
  return (
    <GenericTreeCrudPage<PostInfoDto, CreatePostCommand, UpdatePostCommand>
      title="مدیریت ساختار چارت سازمانی"
      columns={columns}
      crudOptions={crudOptions}
    />
  );
}