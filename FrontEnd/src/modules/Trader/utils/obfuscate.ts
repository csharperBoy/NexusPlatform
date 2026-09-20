/* ─── obfuscation سبک برای رمزها (نه امنیتی، فقط ظاهری) ─── */
const OBF_KEY = "trader-local-2026";

export function obfuscate(text: string): string {
  if (!text) return "";
  let out = "";
  for (let i = 0; i < text.length; i++) {
    out += String.fromCharCode(
      text.charCodeAt(i) ^ OBF_KEY.charCodeAt(i % OBF_KEY.length),
    );
  }
  return btoa(unescape(encodeURIComponent(out)));
}

export function deobfuscate(encoded: string): string {
  if (!encoded) return "";
  try {
    const raw = decodeURIComponent(escape(atob(encoded)));
    let out = "";
    for (let i = 0; i < raw.length; i++) {
      out += String.fromCharCode(
        raw.charCodeAt(i) ^ OBF_KEY.charCodeAt(i % OBF_KEY.length),
      );
    }
    return out;
  } catch {
    return "";
  }
}