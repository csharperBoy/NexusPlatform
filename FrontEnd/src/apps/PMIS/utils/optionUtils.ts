import type { Option } from '../models/Option';

export function normalizeText(text: string): string {
  return String(text)
    .replace(/[۰-۹]/g, (d) => '۰۱۲۳۴۵۶۷۸۹'.indexOf(d).toString())
    .replace(/\s+/g, ' ')
    .replace(/[\u200C\u200B\u00A0]/g, '')
    .trim();
}

export function matchOptionLabel(
  rawValue: string,
  options: (Option | string)[],
  context?: { field?: string; debug?: boolean }
): string {
  const normalizedValue = normalizeText(rawValue);

  const match = options.find((opt) => {
    const label = typeof opt === 'object' ? opt.label : opt;
    return normalizeText(label) === normalizedValue;
  });

  if (!match && context?.debug) {
    console.warn(`❗ تطابق برای "${rawValue}" در ستون "${context.field ?? 'نامشخص'}" پیدا نشد`);
    console.warn('مقدار نرمال‌شده:', normalizedValue);
    console.warn('گزینه‌های موجود:', options.map((opt) => normalizeText(typeof opt === 'object' ? opt.label : opt)));
  }

  return typeof match === 'object' ? match.value : match ?? rawValue;
}
