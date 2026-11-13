# Backtest / Live Parity Checklist

هر PR که به Live orchestration یا Domain logic دست می‌زند، این چک‌لیست را مرور کن.

## ✅ خودکار به Backtest می‌رسد (Domain، مشترک)

- `IStrategy.Evaluate` (Strategy logic)
- `FillMath` (commission, PnL, avg, max qty)
- `IOrderSizeCalculator.CalculateBuy/CalculateSell`
- `StopLossConfig.ResolveForEntry` / `TakeProfitConfig.ResolveForEntry`
- `ICandleTransformer.TransformAsync` (currency)
- `IStrategyFactory`
- `BacktestTrade.Create` (uses FillMath)

## ⚠️ باید دستی به Backtest منتقل شود

- **Risk Guard config** — `MaxPositionSize.MaxPercentPerSymbol` → `BacktestEngine.HandleSignal` pass به `CalculateBuy`
- **Order Size inputs** — اگر `OrderIntentBuilder` فیلد جدیدی به `BuySizeInput` اضافه کرد، `HandleSignal` هم باید pass کند
- **SL/TP resolution** — `ResolveStopLoss`/`ResolveTakeProfit` در BacktestEngine از `StopLossConfig.FromPercentage().ResolveForEntry()` استفاده می‌کنند
- **Commission rates** — هر تغییر در `Instrument.BuyCommissionRate` semantics → `ExecuteFill` (Backtest) + `ApplyBuyFill` (Live)

## ❌ در Backtest نادیده گرفته می‌شود (design، D55)

- KillSwitchGuard، DailyLossLimitGuard، CircuitBreakerGuard، RateLimiterGuard
- Order lifecycle (Channel + Stage 4 polling)
- Broker Rejections / Partial fills از broker
- Session / rate limits
- Real-time market sessions (`TradingSchedule`)

## 🎯 Backtest-only

- Slippage (`IOrderFillSimulator`)
- Next-candle-open fill
- Sub-candle SL/TP resolution (`ISubCandleResolver`)
- Candle preload from DB
- Two-Stream (IRR vs transformed) — D57

## 💱 TargetCurrency (D57)

- **Strategy input:** transformed candles (IRR → USD/EUR/...)
- **Execution:** IRR — fill، cash، position، PnL، گزارش
- **Never:** transform execution prices

## ⚙️ SL/TP on Add (D56)

- Live: `Position.ApplyBuyFill` SL/TP را تغییر نمی‌دهد
- Backtest: هم‌راستا — SL/TP از intent اصلی
- Trailing SL/TP = feature جدید (E.2)، نه bug-fix