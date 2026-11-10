import React from "react";
import { GenericCrudPage } from "@/core/components/crud/components/GenericCrudPage";
import { GenericColumnDef } from "@/core/components/crud/types";
import { priceSourceCrudApi } from "../../hooks/usePriceSourceManagement";
import type {
  PriceSourceView,
  CreatePriceSourceCommand,
  UpdatePriceSourceCommand,
} from "../../models";

const columns: GenericColumnDef<PriceSourceView>[] = [
  { key: "code", label: "کد", type: "text", dir: "ltr", required: true },
  { key: "name", label: "نام", type: "text", required: true },
  { key: "fromAsset", label: "از", type: "text", dir: "ltr", required: true },
  { key: "toAsset", label: "به", type: "text", dir: "ltr", required: true },
  {
    key: "configJson",
    label: "پیکربندی (JSON)",
    type: "text",
    dir: "ltr",
    required: true,
    render: (_v, item) => (
      <code className="font-mono text-[11px] text-gray-400">
        {item.configJson}
      </code>
    ),
  },
  {
    key: "isActive",
    label: "وضعیت",
    editable: false,
    render: (_v, item) =>
      item.isActive ? (
        <span className="rounded bg-emerald-900/40 px-2 py-0.5 text-xs text-emerald-300">
          فعال
        </span>
      ) : (
        <span className="rounded bg-red-900/40 px-2 py-0.5 text-xs text-red-300">
          غیرفعال
        </span>
      ),
  },
];

export const PriceSourcesManagementPage: React.FC = () => {
  return (
    <div>
      <div className="mb-4 rounded-lg border border-blue-700 bg-blue-900/20 p-3 text-sm text-blue-200">
        ℹ️ ConfigJson نمونه برای TGJU (دلار):
        <code className="mx-2 rounded bg-slate-800 px-2 py-0.5 font-mono text-xs text-emerald-300">
          {'{"indicator":"price_dollar_rl"}'}
        </code>
        <br />
        <span className="text-xs text-blue-300">
          حذف = غیرفعال‌سازی. منبع غیرفعال در ساخت Job جدید قابل انتخاب نیست.
        </span>
      </div>

      <GenericCrudPage<
        PriceSourceView,
        CreatePriceSourceCommand,
        UpdatePriceSourceCommand
      >
        title="منابع قیمت"
        columns={columns}
        crudOptions={{
          api: priceSourceCrudApi,
          columns,
          mapToUpdateCommand: (item) => ({
            id: item.id,
            name: item.name,
            configJson: item.configJson,
            fromAsset: item.fromAsset,
            toAsset: item.toAsset,
          }),
          mapToCreateCommand: (formData) => ({
            code: formData.code,
            name: formData.name,
            type: 1, // PriceSourceType.Tgju — Phase B1 only provider
            configJson: formData.configJson,
            fromAsset: formData.fromAsset,
            toAsset: formData.toAsset,
          }),
          tableFeatures: {
            enableSearch: true,
            enableDelete: true,
            enableColumnFilter: false,
          },
          pageFeatures: {
            enableAdd: true,
          },
        }}
      />
    </div>
  );
};