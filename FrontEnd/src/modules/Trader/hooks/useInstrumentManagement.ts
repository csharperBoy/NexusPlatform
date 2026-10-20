import { instrumentApi } from "../api/instrumentApi";
import type {
  InstrumentInfoView,
  CreateInstrumentCommand,
  UpdateInstrumentCommand,
} from "../models";
import { GenericCrudApi } from "@/core/components/crud/types";

export const instrumentCrudApi: GenericCrudApi<
  InstrumentInfoView,
  CreateInstrumentCommand,
  UpdateInstrumentCommand
> = {
  getList: instrumentApi.GetList,
  getSelectionList: instrumentApi.GetSelectionList,
  create: instrumentApi.create,
  batchUpdate: async (cmds) => {
    for (const cmd of cmds) await instrumentApi.update(cmd);
    return true;
  },
  delete: instrumentApi.delete,
};

export function useInstrumentManagement() {
  return { instrumentCrudApi };
}