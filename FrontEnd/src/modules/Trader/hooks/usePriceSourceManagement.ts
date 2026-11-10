import { priceSourceApi } from "../api/priceCollectionApi";
import type {
  PriceSourceView,
  CreatePriceSourceCommand,
  UpdatePriceSourceCommand,
} from "../models";
import { GenericCrudApi } from "@/core/components/crud/types";

export const priceSourceCrudApi: GenericCrudApi<
  PriceSourceView,
  CreatePriceSourceCommand,
  UpdatePriceSourceCommand
> = {
  getList: priceSourceApi.GetList,
  getSelectionList: async () =>
    (await priceSourceApi.GetList())
      .filter((s) => s.isActive)
      .map((s) => ({
        value: s.id,
        label: s.name,
        display: `${s.name} (${s.fromAsset}/${s.toAsset})`,
      })),
  create: priceSourceApi.create,
  batchUpdate: async (cmds) => {
    for (const cmd of cmds) await priceSourceApi.update(cmd);
    return true;
  },
  delete: priceSourceApi.delete,
};

export function usePriceSourceManagement() {
  return { priceSourceCrudApi };
}