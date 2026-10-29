import React from "react";
import { GenericCrudPage } from "@/core/components/crud/components/GenericCrudPage";
import type { GenericColumnDef } from "@/core/components/crud/types";
import { instrumentCrudApi } from "../../hooks/useInstrumentManagement";
import { tradingScheduleApi } from "../../api/tradingScheduleApi";
import type {
  InstrumentInfoView,
  CreateInstrumentCommand,
  UpdateInstrumentCommand,
} from "../../models";
import type { SelectionListDto } from "@/core/models/SelectionListDto";

/* ─── Static Options ─── */
const instrumentTypeOptions: SelectionListDto[] = [
  { value: "0", label: "سهام", display: "سهام" },
  { value: "2", label: "آتی", display: "آتی" },
  { value: "3", label: "اوراق", display: "اوراق" },
  { value: "4", label: "صندوق", display: "صندوق" },
  { value: "5", label: "ارز", display: "ارز" },
  { value: "6", label: "رمزارز", display: "رمزارز" },
];

const marketTypeOptions: SelectionListDto[] = [
  { value: "0", label: "بورس تهران", display: "بورس تهران" },
  { value: "2", label: "بورس آتی", display: "بورس آتی" },
  { value: "10", label: "فارکس", display: "فارکس" },
  { value: "20", label: "کریپتو", display: "کریپتو" },
];

/* ─── Helpers ─── */
const formatCommission = (rate: number): string =>
  `${(rate * 100).toFixed(3)}%`;

/* ─── Columns ─── */
const columns: GenericColumnDef<InstrumentInfoView>[] = [
  {
    key: "isin",
    label: "ISIN",
    type: "text",
    dir: "ltr",
    required: true,
    editable: false,
    editableInAdd: true,
  },
  {
    key: "symbolName",
    label: "نام نماد",
    type: "text",
    required: true,
  },
  {
    key: "displayName",
    label: "نام نمایشی",
    type: "text",
    required: true,
  },
  {
    key: "type",
    label: "نوع",
    type: "select",
    required: true,
    editable: false,
    editableInAdd: true,
    staticOptions: instrumentTypeOptions,
    valueType: "number",
    render: (_v, item) => item.typeName,
  },
  {
    key: "market",
    label: "بازار",
    type: "select",
    required: true,
    editable: false,
    editableInAdd: true,
    staticOptions: marketTypeOptions,
    valueType: "number",
    render: (_v, item) => item.marketName,
  },
  {
    key: "boardCode",
    label: "کد تابلو",
    type: "text",
    dir: "ltr",
  },
  {
    key: "tradingScheduleId",
    label: "ساعت معاملاتی",
    type: "select",
    selectionKey: "tradingSchedule",
    required: false,
    render: (_v, item) =>
      item.tradingScheduleName ? item.tradingScheduleName : "—",
  },

  /* ─── NEW: Commission + Order Limits ─── */
  {
    key: "buyCommissionRate",
    label: "کمیسیون خرید (%)",
    type: "number",
    required: true,
    valueType: "number",
    dir: "ltr",
    render: (v: number) => formatCommission(v),
  },
  {
    key: "sellCommissionRate",
    label: "کمیسیون فروش (%)",
    type: "number",
    required: true,
    valueType: "number",
    dir: "ltr",
    render: (v: number) => formatCommission(v),
  },
  {
    key: "minOrderQuantity",
    label: "حداقل تعداد",
    type: "number",
    required: true,
    valueType: "number",
    dir: "ltr",
  },
  {
    key: "maxOrderQuantity",
    label: "حداکثر تعداد",
    type: "number",
    required: true,
    valueType: "number",
    dir: "ltr",
    render: (v: number) => v?.toLocaleString?.("fa-IR") ?? v,
  },
  {
    key: "minOrderValueRial",
    label: "حداقل ارزش (ریال)",
    type: "number",
    required: false,
    valueType: "number",
    dir: "ltr",
    render: (v: number | null) =>
      v != null ? v.toLocaleString("fa-IR") : "—",
  },

  {
    key: "isActive",
    label: "وضعیت",
    editable: false,
    render: (v: boolean) => (
      <span
        className={`inline-flex rounded-full px-2 py-1 text-xs font-medium ${
          v
            ? "bg-emerald-900/40 text-emerald-300"
            : "bg-red-900/40 text-red-300"
        }`}
      >
        {v ? "فعال" : "غیرفعال"}
      </span>
    ),
  },
];

/* ─── Page ─── */
export const InstrumentsManagementPage: React.FC = () => {
  return (
    <div>
      <GenericCrudPage<
        InstrumentInfoView,
        CreateInstrumentCommand,
        UpdateInstrumentCommand
      >
        title="مدیریت نمادها"
        columns={columns}
        crudOptions={{
          api: instrumentCrudApi,
          columns,
          selectionApis: {
            tradingSchedule: tradingScheduleApi.GetSelectionList,
          },

          mapToCreateCommand: (formData) => ({
            isin: String(formData.isin ?? "").trim(),
            symbolName: String(formData.symbolName ?? "").trim(),
            displayName: String(formData.displayName ?? "").trim(),
            type: Number(formData.type ?? 0),
            market: Number(formData.market ?? 0),
            boardCode: formData.boardCode
              ? String(formData.boardCode).trim()
              : null,
            tradingScheduleId: formData.tradingScheduleId
              ? String(formData.tradingScheduleId)
              : null,

            // Commission: ورودی به درصد است (0.37) → تبدیل به نسبت (0.0037)
            buyCommissionRate: Number(formData.buyCommissionRate ?? 0) / 100,
            sellCommissionRate: Number(formData.sellCommissionRate ?? 0) / 100,
            minOrderQuantity: Number(formData.minOrderQuantity ?? 1),
            maxOrderQuantity: Number(formData.maxOrderQuantity ?? 100000),
            minOrderValueRial:
              formData.minOrderValueRial == null ||
              formData.minOrderValueRial === ""
                ? null
                : Number(formData.minOrderValueRial),

            optionContract: null,
          }),

          mapToUpdateCommand: (item) => ({
            id: item.id,
            symbolName: item.symbolName,
            displayName: item.displayName,
            boardCode: item.boardCode ?? null,
            tradingScheduleId: item.tradingScheduleId ?? null,
            buyCommissionRate: item.buyCommissionRate,
            sellCommissionRate: item.sellCommissionRate,
            minOrderQuantity: item.minOrderQuantity,
            maxOrderQuantity: item.maxOrderQuantity,
            minOrderValueRial: item.minOrderValueRial ?? null,
          }),

          tableFeatures: {
            enableSearch: true,
            enableDelete: true,
            enableColumnFilter: true,
          },
          pageFeatures: {
            enableAdd: true,
          },
        }}
      />

      <div className="mx-6 mb-6 rounded-lg border border-amber-700 bg-amber-900/20 p-3 text-xs text-amber-300">
        ⚠️ نمادهای <strong>اختیار معامله</strong> در این صفحه پشتیبانی
        نمی‌شوند. برای ثبت آن‌ها از فرم سفارشی استفاده خواهد شد (Phase 2).
      </div>
    </div>
  );
};