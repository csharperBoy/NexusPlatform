// src/modules/Trader/pages/StrategyTuning/StrategyTuningCreateModal.tsx
import React, { useEffect, useState } from "react";
import { strategyApi } from "../../api/strategyApi";
import { instrumentApi } from "../../api/instrumentApi";
import { strategyTuningApi } from "../../api/strategyTuningApi";
import type { StrategyInfoView, CreateStrategyTuningModel } from "../../models";
import { TimeFrame, TIMEFRAME_LABELS } from "../../models";

interface Props { isOpen: boolean; onClose: () => void; onCreated: (id: string) => void; }

const todayISO = () => new Date().toISOString().slice(0, 10);
const yearAgoISO = () => {
  const d = new Date(); d.setFullYear(d.getFullYear() - 2);
  return d.toISOString().slice(0, 10);
};

export const StrategyTuningCreateModal: React.FC<Props> = ({ isOpen, onClose, onCreated }) => {
  const [strategies, setStrategies] = useState<StrategyInfoView[]>([]);
  const [instruments, setInstruments] = useState<{ value: string; display?: string }[]>([]);

  const [name, setName] = useState("");
  const [strategyKey, setStrategyKey] = useState("");
  const [isin, setIsin] = useState("");
  const [targetCurrency, setTargetCurrency] = useState("");
  const [primaryTf, setPrimaryTf] = useState<number>(TimeFrame.D1);
  const [subTf, setSubTf] = useState<string>("");
  const [dateFrom, setDateFrom] = useState(yearAgoISO());
  const [dateTo, setDateTo] = useState(todayISO());
  const [initialCapital, setInitialCapital] = useState("100000000");
  const [slippagePct, setSlippagePct] = useState("0.1");
  const [selectedScales, setSelectedScales] = useState<number>(1); // Yearly
  const [primaryScale, setPrimaryScale] = useState<number>(1);
  const [minTrades, setMinTrades] = useState("0");
  const [popSize, setPopSize] = useState("40");
  const [gens, setGens] = useState("50");
  const [mut, setMut] = useState("0.1");
  const [cross, setCross] = useState("0.8");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!isOpen) return;
    strategyApi.GetList().then(setStrategies).catch(() => setStrategies([]));
    instrumentApi.GetSelectionList()
      .then((list: any[]) => setInstruments(list.map(o => ({ value: o.value, display: o.display || o.label }))))
      .catch(() => setInstruments([]));
  }, [isOpen]);

  useEffect(() => {
    if (!isOpen) return;
    setName("");
    setTargetCurrency("");
    setError(null);
  }, [isOpen]);

  const toggleScale = (bit: number) => {
    setSelectedScales(prev => (prev & bit) ? (prev & ~bit) : (prev | bit));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    if (!name.trim()) { setError("نام الزامی"); return; }
    if (!strategyKey) { setError("استراتژی الزامی"); return; }
    if (!isin) { setError("نماد الزامی"); return; }
    if (selectedScales === 0) { setError("حداقل یک مقیاس"); return; }
    if ((selectedScales & primaryScale) === 0) {
      setError("مقیاس اصلی باید در بین انتخاب‌شده‌ها باشد"); return;
    }

    const pop = Number(popSize), g = Number(gens);
    if (pop * g > 5000) {
      if (!confirm(`این اجرا با ${pop * g} evaluation ممکن است چند ساعت طول بکشد. ادامه؟`)) return;
    }

    const model: CreateStrategyTuningModel = {
      name: name.trim(),
      strategyKey,
      isin,
      targetCurrency: targetCurrency || null,
      primaryTimeFrame: primaryTf,
      subCandleTimeFrame: subTf ? Number(subTf) : null,
      dateFrom: new Date(dateFrom).toISOString(),
      dateTo: new Date(dateTo + "T23:59:59Z").toISOString(),
      initialCapital: Number(initialCapital),
      slippagePercent: Number(slippagePct) / 100,
      selectedScales,
      primaryScale,
      minTradesPerWindow: Number(minTrades),
      populationSize: pop,
      generations: g,
      mutationRate: Number(mut),
      crossoverRate: Number(cross),
    };

    setSubmitting(true);
    try { const id = await strategyTuningApi.Run(model); onCreated(id); onClose(); }
    catch (e) { setError(e instanceof Error ? e.message : "خطا"); }
    finally { setSubmitting(false); }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-start justify-center overflow-y-auto bg-black/50 p-4">
      <div className="my-8 w-full max-w-2xl rounded-xl bg-white shadow-xl dark:bg-gray-800">
        <div className="flex justify-between border-b border-gray-200 px-5 py-3 dark:border-gray-700">
          <h3 className="text-lg font-bold">اجرای بهینه‌سازی جدید</h3>
          <button onClick={onClose} className="text-2xl text-gray-500">×</button>
        </div>
        <form onSubmit={handleSubmit} className="space-y-3 p-5">
          {error && <div className="rounded bg-red-50 p-2 text-sm text-red-700">⚠️ {error}</div>}

          <input type="text" value={name} onChange={e => setName(e.target.value)}
            placeholder="نام" className="w-full rounded border p-2 text-sm dark:bg-gray-700 dark:text-white" />

          <div className="grid grid-cols-2 gap-2">
            <select value={strategyKey} onChange={e => setStrategyKey(e.target.value)}
              className="rounded border p-2 text-sm dark:bg-gray-700 dark:text-white">
              <option value="">استراتژی...</option>
              {strategies.map(s => <option key={s.key} value={s.key}>{s.displayName}</option>)}
            </select>
            <select value={isin} onChange={e => setIsin(e.target.value)}
              className="rounded border p-2 text-sm dark:bg-gray-700 dark:text-white">
              <option value="">نماد...</option>
              {instruments.map(i => <option key={i.value} value={i.value}>{i.display || i.value}</option>)}
            </select>
          </div>

          <div className="grid grid-cols-2 gap-2">
            <select value={primaryTf} onChange={e => setPrimaryTf(Number(e.target.value))}
              className="rounded border p-2 text-sm dark:bg-gray-700 dark:text-white">
              {Object.entries(TIMEFRAME_LABELS).map(([k, v]) =>
                <option key={k} value={k}>{v}</option>)}
            </select>
            <select value={subTf} onChange={e => setSubTf(e.target.value)}
              className="rounded border p-2 text-sm dark:bg-gray-700 dark:text-white">
              <option value="">بدون Sub-Candle</option>
              <option value={TimeFrame.M1}>M1</option>
              <option value={TimeFrame.M5}>M5</option>
              <option value={TimeFrame.M15}>M15</option>
            </select>
          </div>

          <div className="grid grid-cols-2 gap-2">
            <input type="date" value={dateFrom} onChange={e => setDateFrom(e.target.value)}
              className="rounded border p-2 text-sm dark:bg-gray-700 dark:text-white" />
            <input type="date" value={dateTo} onChange={e => setDateTo(e.target.value)}
              className="rounded border p-2 text-sm dark:bg-gray-700 dark:text-white" />
          </div>

          <div className="grid grid-cols-3 gap-2">
            <input type="number" value={initialCapital} onChange={e => setInitialCapital(e.target.value)}
              placeholder="سرمایه" className="rounded border p-2 text-sm dark:bg-gray-700 dark:text-white" />
            <input type="number" value={slippagePct} onChange={e => setSlippagePct(e.target.value)}
              placeholder="Slippage %" className="rounded border p-2 text-sm dark:bg-gray-700 dark:text-white" />
            <select value={targetCurrency} onChange={e => setTargetCurrency(e.target.value)}
              className="rounded border p-2 text-sm dark:bg-gray-700 dark:text-white">
              <option value="">IRR</option>
              <option value="USD">USD</option>
              <option value="EUR">EUR</option>
              <option value="GOLD">GOLD</option>
            </select>
          </div>

          <div className="rounded border border-gray-200 p-3 dark:border-gray-700">
            <p className="mb-2 text-xs font-semibold">مقیاس‌های بهینه‌سازی</p>
            <div className="flex flex-wrap gap-3 text-xs">
              {[{bit:1,l:"سالانه"},{bit:2,l:"۶ ماهه"},{bit:4,l:"۳ ماهه"},{bit:8,l:"۱ ماهه"}].map(s => (
                <label key={s.bit} className="flex items-center gap-1">
                  <input type="checkbox"
                    checked={(selectedScales & s.bit) !== 0}
                    onChange={() => toggleScale(s.bit)} />
                  {s.l}
                </label>
              ))}
            </div>
            <p className="mt-2 mb-1 text-xs font-semibold">مقیاس اصلی (GA هدایت‌کننده)</p>
            <select value={primaryScale} onChange={e => setPrimaryScale(Number(e.target.value))}
              className="w-full rounded border p-2 text-sm dark:bg-gray-700 dark:text-white">
              {[{v:1,l:"سالانه"},{v:2,l:"۶ ماهه"},{v:4,l:"۳ ماهه"},{v:8,l:"۱ ماهه"}].map(s =>
                (selectedScales & s.v) && <option key={s.v} value={s.v}>{s.l}</option>)}
            </select>
          </div>

          <details className="rounded border border-gray-200 p-3 dark:border-gray-700">
            <summary className="cursor-pointer text-xs font-semibold">تنظیمات GA</summary>
            <div className="mt-2 grid grid-cols-4 gap-2 text-xs">
              <div><label>Population</label>
                <input type="number" value={popSize} onChange={e => setPopSize(e.target.value)}
                  className="w-full rounded border p-1 dark:bg-gray-700 dark:text-white" /></div>
              <div><label>Generations</label>
                <input type="number" value={gens} onChange={e => setGens(e.target.value)}
                  className="w-full rounded border p-1 dark:bg-gray-700 dark:text-white" /></div>
              <div><label>Mutation</label>
                <input type="number" step={0.01} value={mut} onChange={e => setMut(e.target.value)}
                  className="w-full rounded border p-1 dark:bg-gray-700 dark:text-white" /></div>
              <div><label>Crossover</label>
                <input type="number" step={0.01} value={cross} onChange={e => setCross(e.target.value)}
                  className="w-full rounded border p-1 dark:bg-gray-700 dark:text-white" /></div>
            </div>
            <p className="mt-2 text-[10px] text-gray-500">
              کل evaluations: {Number(popSize) * Number(gens)}
            </p>
          </details>

          <div className="flex justify-end gap-2 border-t border-gray-200 pt-3 dark:border-gray-700">
            <button type="button" onClick={onClose}
              className="rounded border px-4 py-2 text-sm dark:border-gray-600 dark:text-gray-300">انصراف</button>
            <button type="submit" disabled={submitting}
              className="rounded bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700 disabled:opacity-50">
              {submitting ? "..." : "شروع"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};