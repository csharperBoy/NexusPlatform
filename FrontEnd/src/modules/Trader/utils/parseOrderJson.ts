import type { Symbol } from "../models";

/**
 * JSON چسبانده‌شده از Network کارگزاری رو به Symbol[] تبدیل میکنه.
 * - میتونه تک آبجکت باشه یا آرایه
 * - با یا بدون wrapper «order»
 * - فیلدهای ناشناخته نادیده گرفته میشن
 */
export function parseOrderJson(text: string): Symbol[] {
  const parsed = JSON.parse(text);
  const items = Array.isArray(parsed) ? parsed : [parsed];

  return items.map((item, i) => {
    const o = (item as Record<string, unknown>)?.order ?? item;
    const rec = o as Record<string, unknown>;
    if (!rec || !rec.symbolIsin) {
      throw new Error(`آیتم ${i + 1}: فیلد symbolIsin پیدا نشد`);
    }
    return {
      symbolName: String(rec.symbolName ?? ""),
      symbolIsin: String(rec.symbolIsin),
      price: Number(rec.price) || 0,
      quantity: Number(rec.quantity) || 0,
      side: (rec.side === 1 ? 1 : 0) as 0 | 1,
      validityType: Number(rec.validityType ?? 0),
      commission: Number(rec.commission) || 0.003712,
      orderModelType: Number(rec.orderModelType ?? 1),
      orderFrom: Number(rec.orderFrom ?? 34),
    };
  });
}