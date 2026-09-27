import React from "react";
import { GenericCrudPage } from "@/core/components/crud/components/GenericCrudPage";
import { GenericColumnDef } from "@/core/components/crud/types";
import { symbolCrudApi } from "../../hooks/useSymbolManagement";
import type {
  SymbolInfoView,
  CreateSymbolCommand,
  UpdateSymbolCommand,
} from "../../models";

const columns: GenericColumnDef<SymbolInfoView>[] = [
  { key: "symbolName", label: "نام نماد", type: "text", required: true },
  {
    key: "symbolIsin",
    label: "ISIN",
    type: "text",
    dir: "ltr",
    required: true,
  },
  { key: "price", label: "قیمت", type: "number", required: true },
  { key: "quantity", label: "حجم", type: "number", required: true },
  {
    key: "commission",
    label: "کارمزد",
    type: "number",
    required: true,
  },
];

export const SymbolsManagementPage: React.FC = () => {
  return (
    <GenericCrudPage<
      SymbolInfoView,
      CreateSymbolCommand,
      UpdateSymbolCommand
    >
      title="مدیریت نمادها"
      columns={columns}
      crudOptions={{
        api: symbolCrudApi,
        columns,
        mapToUpdateCommand: (item) => ({
          id: item.id,
          symbolName: item.symbolName,
          symbolIsin: item.symbolIsin,
          price: Number(item.price),
          quantity: Number(item.quantity),
          side: item.side ?? 0,
          validityType: item.validityType ?? 0,
          commission: Number(item.commission) || 0.003712,
          orderModelType: item.orderModelType ?? 1,
          orderFrom: item.orderFrom ?? 34,
        }),
        mapToCreateCommand: (formData) => ({
          symbolName: formData.symbolName,
          symbolIsin: formData.symbolIsin,
          price: Number(formData.price) || 0,
          quantity: Number(formData.quantity) || 0,
          side: 0,
          validityType: 0,
          commission: Number(formData.commission) || 0.003712,
          orderModelType: 1,
          orderFrom: 34,
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
  );
};