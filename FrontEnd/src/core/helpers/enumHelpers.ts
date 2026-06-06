// src/modules/Core/helpers/enumHelpers.ts)
import { SelectionListDto } from "../models/SelectionListDto";

export function enumToSelectionList<T extends Record<string, string | number>>(
  enumObj: T,
  displayMap: Record<keyof T, string>  // نقشه عنوان نمایشی
): SelectionListDto[] {
  return Object.entries(enumObj)
    .filter(([key, value]) => isNaN(Number(key))) // فقط کلیدهای رشته‌ای
    .map(([key, value]) => ({
      value: String(value),
      label: key,
      display: displayMap[key as keyof T] || key,
    }));
}