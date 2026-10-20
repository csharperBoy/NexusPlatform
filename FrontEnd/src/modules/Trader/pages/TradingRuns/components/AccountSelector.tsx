import React, { useEffect, useState } from "react";
import { accountApi } from "../../../api/accountApi";
import { SearchableMultiSelect } from "@/core/components/Selection/SearchableMultiSelect";
import type { SelectionListDto } from "@/core/models/SelectionListDto";

interface Props {
  value: string[];
  onChange: (ids: string[]) => void;
}

export const AccountSelector: React.FC<Props> = ({ value, onChange }) => {
  const [options, setOptions] = useState<SelectionListDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    (async () => {
      try {
        const list = await accountApi.GetSelectionList();
        setOptions(list);
      } catch {
        setOptions([]);
      } finally {
        setLoading(false);
      }
    })();
  }, []);

  if (loading) {
    return (
      <p className="text-xs text-gray-500">در حال بارگذاری حساب‌ها...</p>
    );
  }

  if (options.length === 0) {
    return (
      <p className="rounded border border-dashed border-amber-500 p-3 text-xs text-amber-600 dark:border-amber-600">
        هیچ حساب معاملاتی ثبت نشده است. ابتدا از صفحه‌ی حساب‌ها یک حساب
        اضافه کنید.
      </p>
    );
  }

  return (
    <SearchableMultiSelect
      options={options}
      value={value}
      onChange={onChange}
    />
  );
};