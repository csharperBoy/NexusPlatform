import { useEffect } from "react";
import { useSymbolInfoStore } from "../stores";
import type { SymbolInfo } from "../models";

interface Props {
  isin: string | null | undefined;
  token: string | null | undefined;
  side: 0 | 1;
  onPick: (price: number) => void;
  disabled?: boolean;
}

const fmt = (n: number) => n.toLocaleString("fa-IR");

export function PriceSuggestions({
  isin,
  token,
  side,
  onPick,
  disabled,
}: Props) {
  const info = useSymbolInfoStore((s) => (isin ? s.cache[isin] : undefined));
  const loading = useSymbolInfoStore((s) => (isin ? s.loading[isin] : false));
  const error = useSymbolInfoStore((s) => (isin ? s.errors[isin] : null));
  const ensureInfo = useSymbolInfoStore((s) => s.ensureInfo);

  useEffect(() => {
    if (isin && token) {
      ensureInfo(isin, token);
    }
  }, [isin, token, ensureInfo]);

  if (!isin) return null;

  if (loading && !info) {
    return (
      <div className="mt-2 text-xs text-slate-500">⏳ در حال دریافت اطلاعات...</div>
    );
  }

  if (error && !info) {
    return (
      <div className="mt-2 text-xs text-red-400">
        ⚠️ {error}
        <button
          type="button"
          className="mr-2 underline hover:no-underline"
          onClick={() => token && ensureInfo(isin, token, true)}
        >
          تلاش دوباره
        </button>
      </div>
    );
  }

  if (!info) return null;

  const { highAllowedPrice, lowAllowedPrice, lastTradedPrice, tradeDate } = info;

 const refetch = () => {
    if (token) ensureInfo(isin, token, true);
  };
  return (
    <div className="mt-2 flex flex-wrap gap-2">
      {highAllowedPrice != null && (
        <PriceChip
          label="سقف مجاز"
          value={highAllowedPrice}
          highlight={side === 0}
          color="green"
          disabled={disabled}
          onClick={() => onPick(highAllowedPrice)}
        />
      )}
      {lowAllowedPrice != null && (
        <PriceChip
          label="کف مجاز"
          value={lowAllowedPrice}
          highlight={side === 1}
          color="red"
          disabled={disabled}
          onClick={() => onPick(lowAllowedPrice)}
        />
      )}
      {lastTradedPrice != null &&
        lastTradedPrice !== highAllowedPrice &&
        lastTradedPrice !== lowAllowedPrice && (
          <PriceChip
            label="آخرین"
            value={lastTradedPrice}
            color="slate"
            disabled={disabled}
            onClick={() => onPick(lastTradedPrice)}
          />
        )}

        
      <button
        type="button"
        onClick={refetch}
        disabled={disabled || loading}
        className="rounded-lg border border-slate-700 bg-slate-800/60 px-2 py-1.5 text-xs text-slate-300 hover:bg-slate-700/60 disabled:opacity-50"
        title={tradeDate ? `آخرین معامله: ${tradeDate}` : "بروزرسانی"}
      >
        {loading ? "⏳" : "🔄"}
      </button>
    </div>
    
  );
}

interface ChipProps {
  label: string;
  value: number;
  highlight?: boolean;
  color: "green" | "red" | "slate";
  disabled?: boolean;
  onClick: () => void;
}

function PriceChip({
  label,
  value,
  highlight,
  color,
  disabled,
  onClick,
}: ChipProps) {
  const colors = {
    green: {
      base: "border-green-800/60 bg-green-950/40 text-green-300 hover:bg-green-900/40",
      highlight: "border-green-500 bg-green-900/60 text-green-200 ring-2 ring-green-500/40",
    },
    red: {
      base: "border-red-800/60 bg-red-950/40 text-red-300 hover:bg-red-900/40",
      highlight: "border-red-500 bg-red-900/60 text-red-200 ring-2 ring-red-500/40",
    },
    slate: {
      base: "border-slate-700 bg-slate-800/60 text-slate-300 hover:bg-slate-700/60",
      highlight: "",
    },
  } as const;

  const cls = highlight ? colors[color].highlight : colors[color].base;

  return (
    <button
      type="button"
      className={`rounded-lg border px-3 py-1.5 font-mono text-xs transition-colors disabled:cursor-not-allowed disabled:opacity-50 ${cls}`}
      onClick={onClick}
      disabled={disabled}
      title={`${label}: ${fmt(value)} ریال (کلیک برای قرار دادن)`}
    >
      <span className="ml-1 opacity-70">{label}:</span>
      <span className="font-semibold">{fmt(value)}</span>
    </button>
  );
}

/* در صورت نیاز به use بیرون از این کامپوننت */
export type { SymbolInfo };