/* ═══════════════ Enums ═══════════════ */

export enum OrderPriceStrategy {
  Fixed = 0,
  HighAllowed = 1,
  LowAllowed = 2,
  LastTraded = 3,
  Closing = 4,
  FirstTraded = 5,
}

export const ORDER_PRICE_STRATEGY_OPTIONS = [
  { value: OrderPriceStrategy.Fixed, label: "قیمت ثابت", display: "ثابت" },
  { value: OrderPriceStrategy.HighAllowed, label: "سقف مجاز روزانه (لحظه‌ای)", display: "سقف مجاز" },
  { value: OrderPriceStrategy.LowAllowed, label: "کف مجاز روزانه (لحظه‌ای)", display: "کف مجاز" },
  { value: OrderPriceStrategy.LastTraded, label: "آخرین قیمت معامله", display: "آخرین معامله" },
  { value: OrderPriceStrategy.Closing, label: "قیمت پایانی", display: "پایانی" },
  { value: OrderPriceStrategy.FirstTraded, label: "اولین قیمت معامله", display: "اولین معامله" },
];

export enum OrderQuantityStrategy {
  Fixed = 0,
  MaxOrderQuantity = 1,
  MinOrderQuantity = 2,
}

export const ORDER_QUANTITY_STRATEGY_OPTIONS = [
  { value: OrderQuantityStrategy.Fixed, label: "تعداد ثابت", display: "ثابت" },
  { value: OrderQuantityStrategy.MaxOrderQuantity, label: "حداکثر مجاز نماد", display: "حداکثر مجاز" },
  { value: OrderQuantityStrategy.MinOrderQuantity, label: "حداقل مجاز نماد", display: "حداقل مجاز" },
];

/* ═══════════════ Views ═══════════════ */

export interface OrderPresetView {
  id: string;
  instrumentId: string;
  instrumentIsin: string;
  instrumentSymbolName: string;
  name: string;
  priceStrategy: number;
  priceStrategyName: string;
  fixedPrice: number | null;
  quantityStrategy: number;
  quantityStrategyName: string;
  fixedQuantity: number | null;
  defaultSide: number;
  defaultSideName: string;
  validityType: number;
  commission: number;
  defaultOrderMode: number;
  defaultOrderModeName: string;
  createdAt: string;
}

export interface ResolvedOrderPresetView {
  presetId: string;
  instrumentId: string;
  instrumentIsin: string;
  instrumentSymbolName: string;

  price: number;
  priceSource: string;
  quantity: number;
  quantitySource: string;
  side: number;
  sideName: string;
  defaultOrderMode: number;
  commission: number;

  liveHighAllowed: number | null;
  liveLowAllowed: number | null;
  liveLastTraded: number | null;
  liveClosing: number | null;
  liveFirstTraded: number | null;

  instrumentRangeUpPercent: number | null;
  instrumentRangeDownPercent: number | null;

  usedAccountId: string | null;
  usedAccountName: string | null;
  warning: string | null;
}

/* ═══════════════ Commands ═══════════════ */

export interface CreateOrderPresetCommand {
  instrumentId: string;
  name: string;
  priceStrategy: number;
  fixedPrice: number | null;
  quantityStrategy: number;
  fixedQuantity: number | null;
  defaultSide: number;
  validityType: number;
  commission: number;
  defaultOrderMode: number;
}

export interface UpdateOrderPresetCommand {
  id: string;
  name: string;
  priceStrategy: number;
  fixedPrice: number | null;
  quantityStrategy: number;
  fixedQuantity: number | null;
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