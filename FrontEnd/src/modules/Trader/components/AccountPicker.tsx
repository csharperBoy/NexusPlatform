import { useAccountsStore } from "../stores";
import { TokenStatusBadge } from "./TokenStatusBadge";

interface Props {
  value: string | null;
  onChange: (id: string) => void;
  disabled?: boolean;
  label?: string;
}

export function AccountPicker({
  value,
  onChange,
  disabled,
  label = "حساب",
}: Props) {
  const accounts = useAccountsStore((s) => s.accounts);

  if (accounts.length === 0) {
    return (
      <div className="text-xs text-red-400 mt-2">
        هیچ حسابی تعریف نشده — برو به «اطلاعات پایه»
      </div>
    );
  }

  const current = accounts.find((a) => a.id === value);

  return (
    <>
      <label className="block text-xs text-slate-400 mb-1 mt-3">{label}</label>
      <select
        className="w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-slate-100 focus:outline-none focus:ring-2 focus:ring-blue-500"
        value={value ?? ""}
        onChange={(e) => onChange(e.target.value)}
        disabled={disabled}
      >
        {accounts.map((a) => (
          <option key={a.id} value={a.id}>
            {a.name}
          </option>
        ))}
      </select>
      {current && (
        <div className="mt-1">
          <TokenStatusBadge token={current.token} />
        </div>
      )}
    </>
  );
}