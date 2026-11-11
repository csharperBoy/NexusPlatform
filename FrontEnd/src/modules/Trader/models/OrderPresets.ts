/* ═══════════════ Views ═══════════════ */

export interface OrderPresetView {
  id: string;
  instrumentId: string;
  instrumentIsin: string;
  instrumentSymbolName: string;
  name: string;
  defaultPrice: number;
  defaultQuantity: number;
  defaultSide: number;
  validityType: number;
  commission: number;
  defaultOrderMode: number;
  createdAt: string;
}

/* ═══════════════ Commands ═══════════════ */

export interface CreateOrderPresetCommand {
  instrumentId: string;
  name: string;
  defaultPrice: number;
  defaultQuantity: number;
  defaultSide: number;
  validityType: number;
  commission: number;
  defaultOrderMode: number;
}

export interface UpdateOrderPresetCommand {
  id: string;
  name: string;
  defaultPrice: number;
  defaultQuantity: number;
  defaultSide: number;
  validityType: number;
  commission: number;
  defaultOrderMode: number;
}

/* ═══════════════ Labels ═══════════════ */

export const DEFAULT_SIDE_OPTIONS = [
  { value: 0, label: "خرید", display: "خرید" },
  { value: 1, label: "فروش", display: "فروش" },
];

export const DEFAULT_ORDER_MODE_OPTIONS = [
  { value: 0, label: "بر اساس تعداد", display: "بر اساس تعداد" },
  { value: 1, label: "بر اساس ارزش کل", display: "بر اساس ارزش کل" },
];

export const VALIDITY_TYPE_OPTIONS = [
  { value: 0, label: "روز (Day)", display: "روز (Day)" },
  { value: 1, label: "معتبر تا لغو (GTC)", display: "معتبر تا لغو (GTC)" },
  { value: 2, label: "معتبر تا تاریخ", display: "معتبر تا تاریخ" },
];

export function labelOf(
  options: { value: number; label: string }[],
  value: number,
): string {
  return options.find((o) => o.value === value)?.label ?? String(value);
}