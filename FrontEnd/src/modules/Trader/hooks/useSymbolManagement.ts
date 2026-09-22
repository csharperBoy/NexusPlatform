import { symbolApi } from "../api/symbolApi";
import type {
  SymbolInfoView,
  CreateSymbolCommand,
  UpdateSymbolCommand,
} from "../models";
import { GenericCrudApi } from "@/core/components/crud/types";

export const symbolCrudApi: GenericCrudApi<
  SymbolInfoView,
  CreateSymbolCommand,
  UpdateSymbolCommand
> = {
  getList: symbolApi.GetList,
  getSelectionList: symbolApi.GetSelectionList,
  create: symbolApi.create,
  batchUpdate: async (cmds) => {
    for (const cmd of cmds) await symbolApi.update(cmd);
    return true;
  },
  delete: symbolApi.delete,
};

export function useSymbolManagement() {
  return { symbolCrudApi };
}