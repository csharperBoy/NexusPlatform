# Lightstreamer Integration (Future)

> **وضعیت:** مستندشده، پیاده‌سازی نشده
> **اولویت:** پایین — بعد از تکمیل REST APIها و framework استراتژی‌ها
> **آخرین به‌روزرسانی:** ۲۰۲۶-۱۰-۰۱

---

## چیه؟

سرور push دیتای لحظه‌ای EasyTrader بر بستر Lightstreamer.
Lightstreamer یه استاندارد صنعتی برای streaming دیتای بازار است (فارکس، سهام، کریپتو) که یه پروتکل اختصاصی به اسم **TLCP** (Text Lightstreamer Client Protocol) داره.

---

## Endpoints (کشف‌شده از DevTools)

### ۱. دیتای چارت (Tick / Candle)

- **URL:** `wss://ls.easytrader.ir/lightstreamer`
- **پروتکل:** `TLCP-2.1.0.lightstreamer.com`
- **کاربرد:** قیمت لحظه‌ای، کندل‌های real-time، تیک‌ها
- **مرجع:** EasyChart

### ۲. دیتای Market Sheet

- **URL:** `wss://ls-marketsheet.easytrader.ir/lightstreamer`
- **پروتکل:** `TLCP-2.1.0.lightstreamer.com`
- **کاربرد:** دیتای تابلو، بهترین خرید/فروش، حجم‌ها
- **مرجع:** Market Sheet

**چرا دو تا جدا؟** دیتای چارت (سنگین، فرکانس بالا) با دیتای Market Sheet (سبک‌تر، فرکانس پایین‌تر) قاطی نشه و بار روی هر سرور جدا بمونه.

---

## Headers نمونه

```
Sec-WebSocket-Protocol: TLCP-2.1.0.lightstreamer.com
Sec-WebSocket-Version: 13
Sec-WebSocket-Extensions: permessage-deflate; client_max_window_bits
credentials: omit
mode: cors
```

### ⚠️ نکته‌ی حیاتی: `credentials: omit`

کوکی OIDC فرستاده نمی‌شه. احراز هویت **بعد از اتصال WebSocket** و با ارسال پیام `create_session` انجام می‌شه (که توش access token قرار داره).

---

## Flow احراز هویت (استنباطی)

> ⚠️ این flow **استنباطی** است. برای تأیید نهایی باید کلاینت Lightstreamer رو با DevTools مانیتور کنی و پیام‌های واقعی رو ببینی.

```
1. OIDC login → access_token (فرایند فعلی ما در IBrokerClient.LoginAsync)

2. WebSocket باز می‌شه (بدون auth)
   wss://ls.easytrader.ir/lightstreamer
   Protocol: TLCP-2.1.0.lightstreamer.com

3. پیام create_session فرستاده می‌شه:
   create_session
   LS_user=...
   LS_password=<access_token>
   LS_adapter_set=...

4. سرور session برمی‌گردونه:
   LS_session=Sxxxxx

5. کلاینت subscribe می‌کنه:
   subscribe
   LS_session=Sxxxxx
   LS_subId=1
   LS_mode=MERGE
   LS_group=<isin>
   LS_schema=lastTradedPrice bestBuy bestSell ...

6. سرور شروع می‌کنه به push کردن updateها (هر تیک / هر تغییر)
```

---

## SDK رسمی .NET

```
NuGet Package:  Lightstreamer.DotNetStandard.Client
Version:        6.2.1
Target:         .NET Standard 2.0
```

### قابلیت‌ها

- مدیریت خودکار WebSocket و HTTP transport
- Auto-recovery در قطعی شبکه
- Subscription management
- Decoupling کامل subscription از connection
- Thread-safe event handlers

### نصب

```bash
dotnet add package Lightstreamer.DotNetStandard.Client --version 6.2.1
```

---

## چرا پیاده‌سازی الان نه؟

| دلیل | توضیح |
|------|-------|
| **معماری متفاوت** | event-driven، نه request/response. با `IBrokerClient` فعلی که stateless و per-request هست جور در نمیاد. |
| **lifecycle پیچیده** | connect / subscribe / reconnect / session recovery / unsubscribe همگی state دارن. |
| **REST polling کافیه** | برای استراتژی‌های دقیقه‌ای یا بالاتر، polling با فاصله‌ی ۱-۵ ثانیه کافیه. |
| **اولویت** | REST APIها و framework استراتژی‌ها مهم‌ترن. |
| **پیچیدگی debug** | edge caseهای زیادی داره (نیمه‌قطع، backoff، token expiry وسط session، ...). |

---

## چرا بعداً (شاید) لازم بشه؟

- استراتژی‌های **sub-second** (scalping، market making)
- محاسبه‌ی **micro-structure** بازار (order flow، trade imbalance)
- واکنش سریع به تغییرات order book (< ۱۰۰ms)

---

## مقایسه: REST vs Lightstreamer

| ویژگی | REST (فعلی) | Lightstreamer |
|--------|-------------|---------------|
| **تأخیر** | ~۵۰-۲۰۰ms | ~۵-۲۰ms |
| **فرکانس آپدیت** | محدود به polling | هر تیک بازار |
| **بار سرور** | هر poll = یه request کامل | یه connection دائمی |
| **پیچیدگی** | ساده | پیچیده |
| **مناسب برای** | Order, Account, History | قیمت لحظه‌ای، Order Book، چارت |

---

## معماری پیشنهادی (وقتی پیاده شد)

```
┌──────────────────────────────────────────────────┐
│  Strategy Layer (event-driven)                   │
│  - OnTick(symbol, price)                         │
│  - OnOrderBookUpdate(symbol, bestBid, bestAsk)   │
└──────────────────────────────────────────────────┘
              ▲
              │ events
┌─────────────┴────────────────────────────────────┐
│  IMarketDataStream (interface جدید)              │
│  - Subscribe(symbol, fields)                     │
│  - event OnUpdate(symbol, field, value)          │
│  - Unsubscribe(symbol)                           │
│  - ConnectionState { Connected, Reconnecting }   │
└──────────────────────────────────────────────────┘
              ▲
              │
┌─────────────┴────────────────────────────────────┐
│  EasyTraderLightstreamerClient                   │
│  - از SDK رسمی استفاده می‌کنه                     │
│  - مدیریت reconnect و session                    │
│  - نگاشت field names                             │
│  - لاگ‌گیری از رخدادهای اتصال                     │
└──────────────────────────────────────────────────┘
              │
              ▼
       wss://ls.easytrader.ir/lightstreamer
       wss://ls-marketsheet.easytrader.ir/lightstreamer
```

**نکته‌ی معماری:** این یه interface **جداست** (`IMarketDataStream`)، نه بخشی از `IBrokerClient`. چون ماهیتش event-driven هست، نه request/response.

---

## قدم‌های کشف (برای فاز بعد)

1. **مانیتور DevTools:**
   - تب Network → فیلتر WS
   - روی `ls.easytrader.ir` کلیک کن → تب Messages
   - پیام‌های `create_session` و `subscribe` رو کامل ببین

2. **استخراج پارامترها:**
   - لیست `LS_schema` (field names مثل `lastTradedPrice`, `bestBuy`, ...)
   - مقدار `LS_adapter_set`
   - فرمت `LS_group` (ISIN تنها یا با کاما جدا)

3. **تست SDK:**
   - یه Console App بساز
   - با flow استنباطی بالا وصل شو
   - ببین updateها میان یا نه

4. **کشف سیاست‌ها:**
   - Rate limit داره؟
   - Reconnect policy چیه؟
   - اگه token وسط session expire بشه چی می‌شه؟
   - حداکثر تعداد subscription چقدره؟

---

## نکته: اندیکاتورهای چارت (RSI, MACD, ...)

اندیکاتورهایی که توی EasyChart می‌بینی (RSI, MACD, MA, Bollinger, ...) **از Lightstreamer نمیان**.

طبق الگوی استاندارد چارت‌های معاملاتی:
- سرور فقط **OHLCV خام** می‌فرسته
- **کلاینت (JavaScript)** خودش اندیکاتورها رو محاسبه می‌کنه

**برای پروژه‌ی ما این یعنی:**
- ❌ دنبال endpoint اندیکاتور نگرد — وجود نداره
- ✅ خودت توی C# محاسبه کن با کتابخانه‌هایی مثل:
  - `Skender.Stock.Indicators`
  - `Tulip.NET`
  - یا پیاده‌سازی دستی (برای اندیکاتورهای ساده)

**استثنا:** `/easy/api/symbol-analysis/technical-analysis` که endpoint سرور-side هست و امتیاز Buy/Sell/Neutral می‌ده (که داری).

---

## مراجع

- [Lightstreamer Official Site](https://www.lightstreamer.com/)
- [.NET Client SDK Documentation](https://www.lightstreamer.com/documentation/)
- [TLCP Protocol Specification](https://www.lightstreamer.com/documentation/#PROTOCOL)
- NuGet: `Lightstreamer.DotNetStandard.Client`

---

## Changelog

| تاریخ | تغییر |
|-------|-------|
| ۲۰۲۶-۱۰-۰۱ | مستند اولیه — کشف از DevTools |