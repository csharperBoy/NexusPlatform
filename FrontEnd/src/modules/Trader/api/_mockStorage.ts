/* ─── helper برای mock با localStorage ─── */
export function mockLoad<T>(key: string, fallback: T): T {
  try {
    const raw = localStorage.getItem(key);
    return raw ? (JSON.parse(raw) as T) : fallback;
  } catch {
    return fallback;
  }
}

export function mockSave<T>(key: string, value: T): void {
  localStorage.setItem(key, JSON.stringify(value));
}

export const mockDelay = (ms: number) =>
  new Promise((r) => setTimeout(r, ms));

export const mockUid = (): string => crypto.randomUUID();