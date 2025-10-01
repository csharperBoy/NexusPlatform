// src/utils/date.ts

export function fixDateTimezone(date: Date): string {
  const fixedDate = new Date(date);
  fixedDate.setMinutes(fixedDate.getMinutes() - fixedDate.getTimezoneOffset());
  return fixedDate.toISOString();
}

export function normalizeDates(obj: any): any {
  if (Array.isArray(obj)) {
    return obj.map(normalizeDates);
  }

  if (obj instanceof Date) {
    return fixDateTimezone(obj);
  }

  if (typeof obj === 'object' && obj !== null) {
    const result: any = {};
    for (const key in obj) {
      const value = obj[key];
      result[key] = normalizeDates(value);
    }
    return result;
  }

  return obj;
}
