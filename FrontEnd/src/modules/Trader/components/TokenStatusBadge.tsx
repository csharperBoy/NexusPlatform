import { getTokenStatus } from "../stores";

interface Props {
  token: string | undefined;
  compact?: boolean;
}

export function TokenStatusBadge({ token, compact = false }: Props) {
  const st = getTokenStatus(token);

  if (st.state === "empty")
    return <span className="text-xs text-red-400">✗ خالی</span>;
  if (st.state === "invalid")
    return <span className="text-xs text-yellow-400">? نامعتبر</span>;
  if (st.state === "expired")
    return <span className="text-xs text-red-400">✗ منقضی شده</span>;

  const h = Math.floor((st.remainMs ?? 0) / 3_600_000);
  const m = Math.floor(((st.remainMs ?? 0) % 3_600_000) / 60_000);
  const text = compact
    ? `${h}:${String(m).padStart(2, "0")}`
    : `معتبر تا ${h}:${String(m).padStart(2, "0")} دیگر`;

  return <span className="text-xs text-green-400">✓ {text}</span>;
}