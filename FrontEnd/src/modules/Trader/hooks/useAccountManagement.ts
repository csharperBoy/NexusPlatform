import { accountApi } from "../api/accountApi";
import type {
  AccountInfoView,
  CreateAccountCommand,
  UpdateAccountCommand,
} from "../models";
import { GenericCrudApi } from "@/core/components/crud/types";

/**
 * آداپتور accountApi برای GenericCrudPage
 * همه‌ی متدهای GenericCrudApi رو فراهم می‌کنه.
 */
export const accountCrudApi: GenericCrudApi<
  AccountInfoView,
  CreateAccountCommand,
  UpdateAccountCommand
> = {
  getList: accountApi.GetList,
  getSelectionList: accountApi.GetSelectionList,
  create: accountApi.create,
  batchUpdate: async (cmds) => {
    // بک‌اند batchUpdate نداره → تک‌تک می‌زنیم
    for (const cmd of cmds) await accountApi.update(cmd);
    return true;
  },
  delete: accountApi.delete,
};

export function useAccountManagement() {
  return { accountCrudApi };
}