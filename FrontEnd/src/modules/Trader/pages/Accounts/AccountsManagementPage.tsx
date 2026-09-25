import React from "react";
import { GenericCrudPage } from "@/core/components/crud/components/GenericCrudPage";
import { GenericColumnDef } from "@/core/components/crud/types";
import { accountCrudApi } from "../../hooks/useAccountManagement";
import type {
  AccountInfoView,
  CreateAccountCommand,
  UpdateAccountCommand,
} from "../../models";
import { TokenStatusBadge } from "../SchedulePlans/components/TokenStatusBadge";
const columns: GenericColumnDef<AccountInfoView>[] = [
  { key: "name", label: "نام حساب", type: "text", required: true },
  { key: "username", label: "نام کاربری (کد ملی)", type: "text", dir: "ltr", required: true },
  {
    key: "password",
    label: "رمز عبور",
    type: "text",
    required: true,
    render: () => "••••••", // ← توی جدول نمایش داده نشه
  },
  {
    key: "sessionStatus",
    label: "وضعیت نشست",
    editable: false,
    render: (_v, item) => <TokenStatusBadge status={item.sessionStatus} />,
  },
];

export const AccountsManagementPage: React.FC = () => {
  return (
    <GenericCrudPage<
      AccountInfoView,
      CreateAccountCommand,
      UpdateAccountCommand
    >
      title="مدیریت حساب‌ها"
      columns={columns}
      crudOptions={{
        api: accountCrudApi,
        columns,
        mapToUpdateCommand: (item) => ({
          id: item.id,
          name: item.name,
          username: item.username,
          // رمز فقط وقتی تغییر داده بشه پاس میشه — الآن نداریم
          password: undefined,
        }),
        mapToCreateCommand: (formData) => ({
            broker: 1,   
          name: formData.name,
          username: formData.username,
          password: formData.password ?? "",
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