/**
 * استخراج exp (زمان انقضا) از JWT بدون verify.
 * JWT رو در سه بخش با "." جدا میکنیم و بخش دوم رو decode میکنیم.
 */
export function decodeJwtExp(token: string | null | undefined): number | null {
  if (!token) return null;
  try {
    const parts = token.trim().split(".");
    if (parts.length !== 3) return null;
    const payload = JSON.parse(
      atob(parts[1].replace(/-/g, "+").replace(/_/g, "/")),
    ) as { exp?: number };
    return payload.exp ? payload.exp * 1000 : null;
  } catch {
    return null;
  }
}

export function isTokenValid(token: string | null | undefined): boolean {
  const exp = decodeJwtExp(token);
  if (!exp) return false;
  return exp > Date.now();
}