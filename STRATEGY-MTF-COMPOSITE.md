
# MtfCompositeStrategy — مستند کامل استراتژی

> **نسخه**: ۱.۰  
> **تاریخ**: ۱۴۰۴/۰۹  
> **StrategyKey**: `"mtf-composite"`  
> **وضعیت**: پیاده‌سازی‌شده (Phase 1)

---

## فهرست

1. [توضیح مادربزرگی](#۱-توضیح-مادربزرگی)
2. [مفهوم اصلی — تصمیم چندلایه](#۲-مفهوم-اصلی--تصمیم-چندلایه)
3. [سه لایه تصمیم](#۳-سه-لایه-تصمیم)
4. [قدرت سیگنال (Strength) — چطور ترکیب می‌شن](#۴-قدرت-سیگنال-strength--چطور-ترکیب-می‌شن)
5. [تصمیم نهایی — Buy، Sell، Hold](#۵-تصمیم-نهایی--buy-sell-hold)
6. [SL/TP — مدیریت ریسک هر معامله](#۶-sltp--مدیریت-ریسک-هر-معامله)
7. [Intra-Candle — تصمیم قبل از بسته شدن کندل](#۷-intra-candle--تصمیم-قبل-از-بسته-شدن-کندل)
8. [کندل اول روز — پرقدرت‌ترین سیگنال](#۸-کندل-اول-روز--پرقدرت‌ترین-سیگنال)
9. [راهنمای تنظیمات (Config Reference)](#۹-راهنمای-تنظیمات-config-reference)
10. [سناریوهای نمونه](#۱۰-سناریوهای-نمونه)
11. [حالت‌های خاص و Edge Cases](#۱۱-حالت‌های-خاص-و-edge-cases)
12. [محدودیت‌ها — این استراتژی چه کاری نمی‌کنه](#۱۲-محدودیت‌ها--این-استراتژی-چه-کاری-نمی‌کنه)
13. [راهنمای Tuning (بعد از Backtest)](#۱۳-راهنمای-tuning-بعد-از-backtest)
14. [Glossary](#۱۴-glossary)

---

## ۱. توضیح مادربزرگی

تصور کن می‌خوای یه خونه بخری. آدم عجول فقط می‌بینه "قیمتش مناسبه؟" و می‌خره. آدم عاقل چند تا چیز رو با هم می‌سنجه:

1. **محله داره رشد می‌کنه یا افت می‌کنه؟** (روند — Trend)
2. **الآن بازار خریدار داره یا فروشنده؟** (قدرت حرکت — Momentum)
3. **مردم دارن هجوم میارن یا خالی می‌کنن؟** (حجم و صف — Microstructure)

اگه **همه‌ی این سه نشونه** مثبت باشن → خرید منطقیه.
اگه فقط یکی مثبته → شک کن.
اگه دو تاش منفیه → **نخر**.

استراتژی ما هم دقیقاً همین کار رو می‌کنه، فقط برای خرید و فروش **سهام**. به جای "خونه"، "سهم" می‌خره. به جای "محله داره رشد می‌کنه"، نگاه می‌کنه به روند قیمت. به جای "مردم هجوم آوردن"، نگاه می‌کنه به حجم معاملات و صف خرید/فروش.

**تفاوت مهم با استراتژی قبلی:**

استراتژی قبلی (RSI) مثل این بود که فقط **نبض** بیمار رو چک کنه. نبض تند می‌زد → می‌گفت "بیماره". ولی نبض تند می‌تونه از دویدن باشه، از ترس باشه، یا از واقعاً بیماری.

استراتژی جدید مثل **دکتر متخصص** هست:
- نبض (RSI) رو چک می‌کنه
- فشار خون (Trend / EMA) رو چک می‌کنه
- آزمایش خون (MACD / Momentum) می‌گیره
- سابقه‌ی بیمار (Volume Baseline) رو نگاه می‌کنه
- وضعیت فعلی (Order Book — صف خرید/فروش) رو می‌بینه

**اگه همه‌ی این‌ها با هم مثبت بودن** → نسخه‌ی خرید می‌نویسه.
**اگه همه با هم منفی بودن** → نسخه‌ی فروش می‌نویسه.
**اگه ناهماهنگ بودن** → هیچ کاری نمی‌کنه، صبر می‌کنه تا شرایط روشن‌تر بشه.

**نتیجه:** استراتژی ما **کمتر معامله می‌کنه، ولی هر معامله‌اش با فکرتره**. مهم‌تر از تعداد معامله، **کیفیت** معامله‌ست.

### یک مثال عینی

فرض کن ساعت ۱۰ صبح داری فولاد رو نگاه می‌کنی:

- **روند (D1)**: EMA20 بالای EMA50 هست و هر دو صعودی‌ن. قیمت هم بالای هر دو. → **روند مثبت** ✅
- **حرکت (H1)**: RSI روی ۶۲ هست (بالای ۵۵، ولی خیلی بالا نیست). MACD هم تازه cross up کرده. → **حرکت مثبت** ✅
- **میکرو (H1)**: حجم نیم ساعت اول این کندل ۲ برابر معمول بوده. BuyRatio هم ۱.۳ هست (خریدار بیشتر از فروشنده). صف خرید هم عمیق‌تره. → **میکرو مثبت** ✅

استراتژی می‌گه: "هر سه لایه مثبتن، برو بخر!". و **حد ضرر** و **حد سود** هم بر اساس نوسان سهم (ATR) حساب می‌کنه.

حالا فرض کن فردا صبح، فولاد:

- **روند**: هنوز مثبته ✅
- **حرکت**: RSI افتاده به ۴۵ (خنثی)، MACD هم منفی شده ❌
- **میکرو**: حجم معمولیه، BuyRatio ۰.۹ (فروشنده بیشتره) ❌

استراتژی می‌گه: "روند مثبته ولی حرکت و میکرو منفی‌ن، نخر". صبر می‌کنه.

---

## ۲. مفهوم اصلی — تصمیم چندلایه

### 2.1 چرا یک لایه کافی نیست

هر اندیکاتور تنها **یک زاویه** از بازار رو می‌بینه:

| اندیکاتور | چه می‌گه | چه نمی‌گه |
|---|---|---|
| RSI | "قیمت نسبت به قبل کجاست" | "روند کلی چیه؟" |
| EMA | "روند کجاست" | "حرکت الآن چقدر قویه؟" |
| MACD | "حرکت قویه یا ضعیف؟" | "صف خرید داریم یا نه؟" |
| Volume | "حجم زیاده یا کم" | "این حجم به نفع کیه؟" |

اگه فقط یکی رو ببینیم، همون داستان "نبض بیمار" پیش میاد.

### 2.2 راه‌حل: سه لایه مستقل

استراتژی ما **سه لایه** مستقل داره که هر کدام **زاویه‌ی متفاوتی** از بازار رو می‌بینن:

 
┌─────────────────────────────────────────────────┐
│  Layer 1: Trend (روند)                          │
│  منبع: EMA Fast/Slow روی TrendTimeFrame (D1)   │
│  سؤال: "جهت کلی بازار چیه؟"                     │
└─────────────────────────────────────────────────┘
           ↓
┌─────────────────────────────────────────────────┐
│  Layer 2: Momentum (حرکت)                       │
│  منبع: RSI + MACD روی PrimaryTimeFrame          │
│  سؤال: "قدرت حرکت الآن چقدره؟"                 │
└─────────────────────────────────────────────────┘
           ↓
┌─────────────────────────────────────────────────┐
│  Layer 3: Microstructure (میکرو)                │
│  منبع: Volume + Order Book (Buy/Sell Ratio)    │
│  سؤال: "پول واقعی داره میاد یا نه؟"            │
└─────────────────────────────────────────────────┘
           ↓
    ┌──────────────┐
    │  Composite   │
    │   Decision   │
    └──────────────┘
 

**چرا این ترتیب؟**
- **Trend** کندترین لایه‌ست (روزانه) — جهت کلی رو می‌ده
- **Momentum** متوسطه (ساعتی) — قدرت لحظه‌ای رو می‌ده
- **Micro** سریع‌ترینه (لحظه‌ای) — تأیید نهایی

این یعنی هر سیگنال ما:
1. با روند کلی هم‌جهته
2. با حرکت قوی تأیید شده
3. با پول واقعی پشتیبانی شده

### 2.3 چرا AND نیست؟

اگه بگیم "همه سه لایه باید مثبت باشن" → خیلی کم سیگنال می‌گیریم. بازار همیشه کامل نیست.

به جاش، **هر لایه یه Score بین ۰ تا ۱** می‌ده، بعد با وزن ترکیب می‌شن:


Strength = 0.40 × TrendScore + 0.35 × MomentumScore + 0.25 × MicroScore


اگه Trend=1.0، Momentum=0.3، Micro=0.6 → Strength = 0.4+0.105+0.15 = 0.655
- اگه Threshold = 0.65 → **Buy** (حتی با Momentum ضعیف، چون Trend خیلی قویه)

این انعطاف بزرگ استراتژی ما هست: **یک لایه قوی می‌تونه ضعف بقیه رو جبران کنه، ولی نه همیشه**.

---

## ۳. سه لایه تصمیم

### 3.1 Layer 1: Trend Score

**فلسفه**: اگه روند کلی صعودیه، خرید (pullback) امن‌تره. اگه روند نزولیه، خرید "توپ در حال سقوط" گرفتنه.

**منبع داده**: کندل‌های `TrendTimeFrame` (پیش‌فرض D1). اگه PrimaryTimeFrame خودش D1 بود، از H4 استفاده می‌شه (باید TrendTimeFrame < PrimaryTimeFrame باشه — بهتر بگیم ≤).

**محاسبه**:

 
EMA_fast = EMA(closes, EmaFastPeriod)   // پیش‌فرض 20
EMA_slow = EMA(closes, EmaSlowPeriod)   // پیش‌فرض 50
Price    = closes[-1]

TrendBullishScore:
    Trend_Slow_Rising = EMA_slow[-1] > EMA_slow[-2]   // امروز > دیروز
    Price_Above_Slow  = Price > EMA_slow[-1]

    if Trend_Slow_Rising AND Price_Above_Slow:
        return 1.0    // روند کامل صعودی
    elif Trend_Slow_Rising OR Price_Above_Slow:
        return 0.5    // نصفه‌نیمه
    else:
        return 0.0    // روند نزولی یا بی‌جهت

TrendBearishScore:
    Trend_Slow_Falling = EMA_slow[-1] < EMA_slow[-2]
    Price_Below_Slow   = Price < EMA_slow[-1]

    if Trend_Slow_Falling AND Price_Below_Slow:
        return 1.0
    elif Trend_Slow_Falling OR Price_Below_Slow:
        return 0.5
    else:
        return 0.0
 

**نکته**: Bullish و Bearish دو عدد مستقل هستن. اگه روند کاملاً صعودی باشه → Bullish=1.0، Bearish=0.0. اگه بی‌جهت باشه → هر دو=0.5 (یا 0). هیچ‌وقت هر دو 1.0 نمی‌شن.

**مثال**:
- EMA20=105، EMA50=100 (دیروز 99) → Rising ✅، Price=107 > 100 ✅ → **Bullish=1.0، Bearish=0.0**
- EMA20=95، EMA50=100 (دیروز 101) → Falling ✅، Price=93 < 100 ✅ → **Bullish=0.0، Bearish=1.0**
- EMA20=100، EMA50=100 (دیروز 100) → نه Rising نه Falling، Price=100 = EMA50 → **Bullish=0.0، Bearish=0.0** (بی‌جهت مطلق)

### 3.2 Layer 2: Momentum Score

**فلسفه**: حتی تو روند صعودی، اگه حرکت ضعیف باشه، ورود ضرر می‌ده. Momentum می‌گه "الآن قدرت کافیه یا نه".

**منبع**: RSI + MACD روی `PrimaryTimeFrame` (همون کندل‌هایی که سیگنال می‌گیریم).

**محاسبه RSI Component**:

 
rsi = RSI(closes, RsiPeriod)   // پیش‌فرض 14

RsiBullish:
    if rsi >= RsiBullishThreshold (55):   // بالای ۵۵ = حرکت صعودی
        return clamp((rsi - 55) / 20, 0, 1)   // ۵۵→0، ۷۵→1
    else:
        return 0

RsiBearish:
    if rsi <= RsiBearishThreshold (45):   // زیر ۴۵ = حرکت نزولی
        return clamp((45 - rsi) / 20, 0, 1)   // ۴۵→0، ۲۵→1
    else:
        return 0
 

**محاسبه MACD Component**:

 
macd_line   = EMA(closes, MacdFast) - EMA(closes, MacdSlow)
signal_line = EMA(macd_line, MacdSignal)
histogram   = macd_line - signal_line

MacdBullish:
    if histogram > 0 AND histogram > histogram[-1]:   // مثبت و در حال رشد
        return 1.0
    elif histogram > 0:                                // فقط مثبت
        return 0.5
    else:
        return 0.0

MacdBearish:
    if histogram < 0 AND histogram < histogram[-1]:   // منفی و در حال افت
        return 1.0
    elif histogram < 0:                                // فقط منفی
        return 0.5
    else:
        return 0.0
 

**ترکیب Momentum**:

 
MomentumBullish = (RsiBullish + MacdBullish) / 2
MomentumBearish = (RsiBearish + MacdBearish) / 2
 

**مثال**:
- RSI=62 → RsiBullish = clamp((62-55)/20, 0, 1) = 0.35
- MACD histogram=+0.8، دیروز +0.5 → MacdBullish = 1.0
- MomentumBullish = (0.35 + 1.0) / 2 = **0.675**

### 3.3 Layer 3: Microstructure Score

**فلسفه**: قیمت و RSI فقط "عدد" هستن. "پول واقعی" پشتش هست یا نه؟ Volume + Order Book این رو می‌گن.

**منبع داده**:
- **Volume**: کندل‌های `PrimaryTimeFrame` + baseline تاریخی
- **Order Book**: `MarketSnapshot` (Live و Backtest با snapshot)

#### 3.3.1 Volume Component — با Baseline دوگانه

**مسئله**: مقایسه‌ی حجم فعلی با "میانگین ۲۰ کندل قبلی" اشتباهه، چون:
- کندل اول روز حجم بالایی داره (از پیش‌گشایش) → اگه با کندل‌های معمولی مقایسه بشه، همیشه "بالا" به نظر می‌رسه (سیگنال کاذب)
- کندل‌های آخر وقت هم معمولاً حجم بالایی دارن (بستن پوزیشن‌ها)

**راه‌حل: دو Baseline مستقل**

 
دو دسته کندل تاریخی جدا می‌کنیم:

دسته A (Intra-Day): کندل‌هایی که نه اولین کندل روز هستن، نه آخرین کندل
  → Baseline_A = median(حجم دسته A، ۱۰۰ کندل اخیر)

دسته B (Opening): فقط اولین کندل هر روز
  → Baseline_B = median(حجم دسته B، ۳۰ روز اخیر)
 

**چطور تشخیص می‌دیم "اولین کندل روز" هست؟**

 
اگه فاصله‌ی زمانی بین OpenTime کندل با کندل قبلی > ۶ ساعت باشه:
    → این کندل، اولین کندل روز هست (بازار بین این دو کندل بسته بوده)
 

**چرا ۶ ساعت؟** بازار ایران حداکثر ۴-۵ ساعت بازه. اگه بیش از ۶ ساعت فاصله بود، یعنی یه شب گذشته.

**محاسبه Volume Score**:

 
current_volume = کندل جاری یا کندل کامل
is_opening = آیا کندل جاری، اولین کندل امروزه؟

baseline = is_opening ? Baseline_B : Baseline_A

اگه نمونه‌های baseline < MinBaselineSamples (پیش‌فرض ۱۰):
    → VolumeScore = 0 (baseline معتبر نیست، Hold)

VolumeRatio = current_volume / baseline

VolumeBullish = VolumeBearish = min(1.0, VolumeRatio / VolumeMultiplier)
   // VolumeMultiplier = 1.5 (اگه حجم ۱.۵ برابر baseline، VolumeScore=1.0)
 

**نکته**: VolumeScore **جهت‌دار نیست** (فقط "زیاده یا کم"). جهت از BuyRatio و MACD میاد. Volume صرفاً **اطمینان** می‌ده.

#### 3.3.2 Order Book Component

**منبع**: `MarketSnapshot` که در Stage 1 پر می‌شه.

 
BuyRatio  = snapshot.BuyRatio   // حجم خرید / حجم فروش در صف
SellRatio = snapshot.SellRatio
BestBidQty = snapshot.BestBuyQuantity
BestAskQty = snapshot.BestSellQuantity

DepthRatio = BestBidQty / BestAskQty   // یا برعکس

OrderBookBullish:
    BuyRatioScore  = clamp(BuyRatio / BuyRatioThreshold (1.1), 0, 1)
    DepthScore     = clamp(DepthRatio / OrderBookDepthRatio (1.5), 0, 1)
    return (BuyRatioScore + DepthScore) / 2

OrderBookBearish:
    SellRatioScore = clamp(SellRatio / SellRatioThreshold (1.1), 0, 1)
    ReverseDepth   = BestAskQty / BestBidQty
    DepthScore     = clamp(ReverseDepth / OrderBookDepthRatio, 0, 1)
    return (SellRatioScore + DepthScore) / 2
 

**اگه Order Book نداشتیم** (مثلاً Backtest بدون snapshot):

 
OrderBookBullish = OrderBookBearish = null
→ نرمال‌سازی وزن‌ها (بخش ۴)
 

#### 3.3.3 ترکیب Micro

 
اگه Order Book موجود:
    MicroBullish = (VolumeBullish + OrderBookBullish) / 2
    MicroBearish = (VolumeBearish + OrderBookBearish) / 2
وگرنه:
    MicroBullish = VolumeBullish
    MicroBearish = VolumeBearish
    → وزن Volume و OrderBook به‌ترتیب در Strength نرمال می‌شن
 

---

## ۴. قدرت سیگنال (Strength) — چطور ترکیب می‌شن

### 4.1 فرمول پایه

 
wT  = TrendWeight       // پیش‌فرض 0.40
wM  = MomentumWeight    // پیش‌فرض 0.35
wMi = MicroWeight       // پیش‌فرض 0.25

BullishStrength = wT × TrendBullish + wM × MomentumBullish + wMi × MicroBullish
BearishStrength = wT × TrendBearish + wM × MomentumBearish + wMi × MicroBearish
 

### 4.2 نرمال‌سازی وقتی Order Book نیست

اگه OrderBook=null، Micro فقط از Volume میاد — ولی وزن Micro همچنان 0.25 هست. این باعث می‌شه Micro "توخالی" باشه. راه‌حل: **وزن‌ها رو بر اساس اجزای موجود نرمال می‌کنیم**.

 
اگه Order Book موجود نبود:
    // Micro فقط Volume هست، پس 0.25 رو Volume می‌گیره
    TotalWeight = wT + wM + wMi = 1.0 (بدون تغییر)
    
    ولی برای محاسبه BullishStrength:
    BullishStrength = (wT×TrendBull + wM×MomBull + wMi×VolumeBull) / 1.0
 

هیچ نرمال‌سازی خاصی لازم نیست چون `MicroBullish = VolumeBullish` خودش یک Score [0,1] هست. فقط اطلاع می‌دیم که Weight های OrderBook در نبودش، به Volume نمی‌رن.

**ولی یه ملاحظه**: اگه Order Book نداشته باشیم، BullishStrength به‌طور سیستماتیک پایین‌تر می‌آد (چون اجزای کمتری داره). این می‌تونه باعث بشه در Backtest (که Order Book نداره) سیگنال کمتر بگیریم.

**راه‌حل نهایی**: به‌جای normalization، **در Backtest همین‌طور می‌ذاریم** (pessimistic). بعداً اگه لازم شد، از `Config.NormalizeForMissingOrderBook` استفاده می‌کنیم.

**توصیه**: در Phase 1، همین رفتار (بدون normalization خاص) رو داریم. اگه Backtest سیگنال کمی داد، در Tuning در نظر می‌گیریم.

### 4.3 Weight Override per-strategy

Weight ها در Config استراتژی قابل تغییر هستن. اگه بعد از Paper Week دیدیم:
- سیگنال کمه → وزن Momentum رو زیاد کن (چون Trend کند و Micro noise داره)
- Win Rate پایینه → وزن Trend رو زیاد کن (محافظه‌کارانه‌تر)

---

## ۵. تصمیم نهایی — Buy، Sell، Hold

### 5.1 الگوریتم تصمیم

 
اول: چک کن که baseline‌ها معتبرن (MinBaselineSamples)
    اگه نبودن → Hold (بازار تازه، داده کم)

دوم: اگه Intra-Candle window هست و حجم trigger نکرده → Hold (صبر)
    (توضیح در بخش ۷)

سوم: محاسبه BullishStrength و BearishStrength
    (توضیح در بخش ۴)

چهارم: تصمیم

    diff = BullishStrength - BearishStrength

    if BullishStrength >= MinSignalStrength (0.65)
       AND diff >= MinStrengthDiff (0.05):
        → Buy Signal با Strength = BullishStrength
        → SL/TP روی سیگنال ست می‌شه (بخش ۶)

    elif BearishStrength >= MinSignalStrength
         AND -diff >= MinStrengthDiff:
        → Sell Signal با Strength = BearishStrength
        → SL/TP روی سیگنال ست نمی‌شه (فقط Buy SL/TP داره)

    else:
        → Hold
 

### 5.2 چرا `MinStrengthDiff` لازمه؟

فرض کن Bullish=0.70، Bearish=0.68. هر دو بالای Threshold. کدوم؟
- اگه فقط بگیم "بالاتر ببره" → هر بار یه سیگنال ضعیف، ممکنه flip-flop بشه (Buy، Sell، Buy، Sell...)
- این Commission رو می‌خوره و مخربه

`MinStrengthDiff = 0.05` می‌گه: "اگه اختلاف کمتر از ۵٪ باشه، تصمیم روشن نیست → Hold".

### 5.3 Sell — بدون SL/TP

سیگنال Sell **Position رو می‌بنده**، نه باز می‌کنه. پس SL/TP روی Sell معنی نداره (SL/TP فقط روی Position باز اعمال می‌شه).

**اگه Sell Signal داد ولی Position باز نداشتیم** → بی‌اثره (OrderIntentBuilder ردش می‌کنه).

---

## ۶. SL/TP — مدیریت ریسک هر معامله

### 6.1 ATR-based SL/TP

**ATR = Average True Range** — میانگین نوسان کندل‌ها.

**محاسبه**:

 
TR[i] = max(
    High[i] - Low[i],
    abs(High[i] - Close[i-1]),
    abs(Low[i]  - Close[i-1])
)

ATR = Wilder's Smoothing روی TR (مثل RSI)
   // پیش‌فرض AtrPeriod = 14
 

**SL/TP روی Buy Signal**:

 
Entry = current_price (قیمت آخرین کندل یا snapshot)

StopLoss   = Entry - AtrStopMultiplier (2.0)   × ATR
TakeProfit = Entry + AtrTakeProfitMultiplier (4.0) × ATR

// R:R = 4.0 / 2.0 = 2:1
 

**مثال**:
- Entry = 10,000 ریال
- ATR = 300 ریال
- SL = 10,000 - 2×300 = **9,400** (-6%)
- TP = 10,000 + 4×300 = **11,200** (+12%)
- R:R = 2:1 → اگه ۵۰٪ معاملات برنده باشه، سودده‌ایم

### 6.2 Run Override — چرا و چطور

**مسئله**: کاربر ممکنه بخواد بگه "هیچ معامله‌ای نباید بیشتر از ۳٪ ضرر بده، حتی اگه ATR سهم بالا باشه".

**راه‌حل**: Run-level SL/TP **Override** می‌کنه.

**منطق**:

 
finalSL = Run.StopLoss?.ResolveForEntry(entryPrice) ?? signal.StopLoss
finalTP = Run.TakeProfit?.ResolveForEntry(entryPrice) ?? signal.TakeProfit
 

- **Run has SL/TP set** → Run wins (کاربر آگاهانه خواسته)
- **Run has no SL/TP** → از strategy signal استفاده می‌شه (ATR-based)

### 6.3 SL/TP اینجا **پیشنهاد** هست، نه قطعی

**نکته مهم**: SL/TP که Strategy می‌ده، **در OrderIntent ذخیره می‌شه**. مدیریت SL/TP **در Stage 4 (OrderLifecycleStage)** اتفاق می‌افته:
- کندل بعد از Entry بررسی می‌شه
- اگه Low ≤ SL → Close at SL
- اگه High ≥ TP → Close at TP

پس Strategy فقط **پیشنهاد** می‌ده، **اجرا** توی OrderLifecycleStage هست.

---

## ۷. Intra-Candle — تصمیم قبل از بسته شدن کندل

### 7.1 مسئله

فرض کن Primary TF = H1، و ساعت ۱۰:۳۰ هست. کندل ۱۰:۰۰-۱۱:۰۰ هنوز بازه. تا ۱۱:۰۰ صبر کنیم، ممکنه قیمت ۳٪ بالا رفته باشه — فرصت از دست رفته.

ولی ممکنه ۱۰:۱۵ بازار یهو حجم بخوره و از قبل معلوم باشه که سیگنال داره.

**راه‌حل**: Intra-Candle Analysis — قبل از بسته شدن کندل، از sub-candle ها نگاه کنیم ببینیم "وضعیت نیمه‌ی کندل نسبت به نیمه‌های تاریخی چطوره".

### 7.2 شرط فعال‌سازی

 
Intra-Candle فعال می‌شه اگه همه‌ی این‌ها برقرار باشن:

1. Config.EnableIntraCandleDecision = true
2. PrimaryTimeFrame >= M15 (برای TFهای کوچک‌تر معنی نداره)
3. SubCandleTimeFrame < PrimaryTimeFrame
4. ExtraTimeFrames[SubCandleTimeFrame] موجود (حداقل SubCandleHistoryDepth کندل)
5. الآن در نیمه‌ی اول کندل جاری هستیم
   (elapsed_in_candle < PrimaryTF_Duration / 2)
 

### 7.3 الگوریتم

 
گام ۱: Sub-Candle های تاریخی رو گروه‌بندی کن بر اساس Primary TF

    برای هر Primary Candle تاریخی:
        نیمه‌ی اول = M5های با OpenTime < Primary_OpenTime + PrimaryTF_Duration/2
        first_half_volume = sum(نیمه‌ی اول)

        اگه این Primary کندل، اولین کندل روزه → Opening group
        وگرنه → IntraDay group

گام ۲: Baseline ها رو حساب کن

    IntraDay_FirstHalf_Baseline = median(IntraDay group)
    Opening_FirstHalf_Baseline  = median(Opening group)

گام ۳: کندل جاری (in-progress) رو ارزیابی کن

    elapsed = now - current_Primary_OpenTime
    current_half_volume = sum(Sub-Candles فعلی)

    // مقایسه‌ی منصفانه: volume فعلی با نسبت زمان scale می‌شه
    // مثال: اگه ۱۵ دقیقه از ۳۰ دقیقه گذشته، ۵۰٪ نیمه‌ی اول
    time_fraction = elapsed / (PrimaryTF_Duration/2)
    selected_baseline = Opening اگر اول-روز، وگرنه IntraDay
    expected_volume = selected_baseline × time_fraction

    ratio = current_half_volume / expected_volume

گام ۴: Trigger

    if ratio >= IntraCandleVolumeThreshold (2.0):
        → intra-candle trigger فعال شد
        → VolumeScore = min(1.0, ratio / (Threshold × 2))
        → بقیه strength از لایه‌های trend/momentum/orderbook
        → اگه Bullish/Bearish ≥ Threshold → همین الآن سیگنال بده
    else:
        → حجم کافی نیست، صبر کن تا کندل بسته بشه
        → Hold (اما VolumeScore = 0 برای همین ارزیابی)
 

### 7.4 مثال

فرض: Primary = H1، Sub = M5، الآن ۱۰:۲۰ (۲۰ دقیقه از کندل H1 گذشته)

 
IntraDay_FirstHalf_Baseline = 100,000 سهم
Opening_FirstHalf_Baseline = 500,000 سهم (اول-روز)

در کندل جاری ۱۰:۰۰-۱۰:۲۰:
  current_half_volume = 80,000 سهم
  time_fraction = 20 / 30 = 0.667
  selected_baseline = IntraDay (نه Opening) = 100,000
  expected_volume = 100,000 × 0.667 = 66,700
  ratio = 80,000 / 66,700 = 1.20

1.20 < 2.0 → trigger نشده → Hold تا ۱۰:۳۰ که کندل بسته بشه
 

حالا فرض کن همون ۱۰:۲۰، ۱۵۰,۰۰۰ سهم معامله شده (نه ۸۰,۰۰۰):

 
ratio = 150,000 / 66,700 = 2.25 ≥ 2.0 → ✅ Trigger

→ VolumeScore = min(1.0, 2.25 / 4.0) = 0.56
→ اگه Trend=1، Mom=0.7، OrderBook=0.8:
   Micro = (0.56 + 0.8) / 2 = 0.68
   Bullish = 0.4×1 + 0.35×0.7 + 0.25×0.68 = 0.4+0.245+0.17 = 0.815 ✅
→ Buy Signal همین الآن (نه صبر تا ۱۱:۰۰)
 

### 7.5 Intra-Candle در Backtest

**Phase 1**: Backtest فقط **close-based**. Intra-candle فقط Live Paper.

**چرا؟** Backtest Engine فعلی event-per-primary-candle می‌سازه. برای intra-candle، باید event-per-sub-candle بسازه، که نیاز به تغییر اساسی داره.

**اثر**: Backtest کمی **pessimistic** هست — فرصت‌هایی که در Live intra-candle از دست نمی‌رن، در Backtest از دست می‌رن. ولی **منطق کلی استراتژی همون هست** — فقط trigger دیرتر (در close).

**Phase 1.5**: اگه Paper Week دیدیم intra-candle ارزش داره، Backtest Engine رو ارتقا می‌دیم.

---

## ۸. کندل اول روز — پرقدرت‌ترین سیگنال

### 8.1 مسئله

کندل اول روز حجم غیرعادی داره — از **پیش‌گشایش** (۹:۰۰-۸:۴۵). سفارش‌هایی که شب قبل ذخیره شدن، همگی در دقیقه‌ی اول معامله می‌شن.

این **نه نویزه، سیگناله**:
- اگه حجم بالا + BuyRatio بالا → **فشار خرید جدی**
- اگه حجم بالا + SellRatio بالا → **فشار فروش جدی**

پس **نباید Ignore بشه** — باید **جدا تحلیل بشه**.

### 8.2 راه‌حل — Dual Baseline (توضیح دقیق)

در بخش 3.3.1 اشاره کردیم. توضیح کامل:

 
دو مجموعه کندل تاریخی داریم:

مجموعه A (Intra-Day):
  کندل‌هایی که نه اولین کندل روز هستن
  → Baseline_A = median(حجم کندل‌های A، ۱۰۰ کندل اخیر)
  → برای تحلیل کندل‌های معمولی روز

مجموعه B (Opening):
  فقط اولین کندل هر روز
  → Baseline_B = median(حجم کندل‌های B، ۳۰ روز اخیر)
  → برای تحلیل کندل‌های اول-روز

تشخیص "اولین کندل روز":
  فاصله‌ی زمانی از کندل قبلی > ۶ ساعت
 

### 8.3 مثال واقعی

فرض کن امروز ۱۴۰۴/۰۹/۱۵، ساعت ۹:۰۰، تحلیل کندل ۹:۰۰-۱۰:۰۰ H1:

 
حجم کندل جاری = 2,500,000 سهم

IntraDay_Baseline = 400,000 سهم
Opening_Baseline = 1,000,000 سهم

چون کندل جاری، اول-روز هست:
    Baseline = Opening_Baseline = 1,000,000

VolumeRatio = 2,500,000 / 1,000,000 = 2.5
VolumeScore = min(1.0, 2.5 / 1.5) = 1.0 ✅

→ Micro خیلی قوی، Strength بالا
→ اگه Trend و Momentum هم مثبت باشن → Buy قوی
 

**بدون Dual Baseline**، اگه با IntraDay_Baseline=400,000 مقایسه می‌کردیم:

 
VolumeRatio = 2,500,000 / 400,000 = 6.25
VolumeScore = min(1.0, 6.25/1.5) = 1.0  → همون 1.0!
 

پس نتیجه یکیه؟ **نه!** بذار ادامه بدم:

فرض کن فردا ۱۴۰۴/۰۹/۱۶، کندل اول روز حجمش **800,000 سهم** باشه (نه 2.5M، نه 400K):

 
با Dual Baseline (Opening = 1M):
    ratio = 800K / 1M = 0.8
    VolumeScore = min(1.0, 0.8/1.5) = 0.53
    → سیگنال ملایم، نه Buy قطعی

بدون Dual Baseline (IntraDay = 400K):
    ratio = 800K / 400K = 2.0
    VolumeScore = min(1.0, 2.0/1.5) = 1.0
    → سیگنال قوی کاذب!
    → Buy اشتباه، چون حجم 800K برای یه کندل معمولی زیاده، ولی برای یه Opening Candle، عادیه
 

**نتیجه**: Dual Baseline از **سیگنال کاذب** در Opening candles جلوگیری می‌کنه، در حالی که اجازه می‌ده **Opening abnormally-high volume** سیگنال قوی بده.

### 8.4 Edge Cases

| سناریو | رفتار |
|---|---|
| کمتر از MinBaselineSamples کندل اول-روز | Opening_Baseline معتبر نیست → Hold (حتی اگه IntraDay کافیه) |
| روز تعطیلی قبلش (مثلاً ۳ روز) | همچنان "اولین کندل روز" تشخیص داده می‌شه (فاصله > ۶ ساعت) |
| نماد تازه IPO | تا ۱۰ روز اول، Hold. از روز ۱۱، Opening_Baseline=۱۰ نمونه → قابل استفاده |
| Backtest با داده ۲ ساله | Opening_Baseline از ۳۰ روز قبل شروع می‌شه، بقیه روزها هم قابل تحلیل |

---

## ۹. راهنمای تنظیمات (Config Reference)

### 9.1 Trend Layer

| فیلد | نوع | پیش‌فرض | توضیح |
|---|---|---|---|
| `EmaFastPeriod` | int | 20 | دوره EMA سریع |
| `EmaSlowPeriod` | int | 50 | دوره EMA کند |
| `TrendTimeFrame` | TimeFrame | D1 | TF برای تحلیل روند |

**نکته**: `TrendTimeFrame` باید **< PrimaryTimeFrame** باشه (اگه Primary = D1، از H4 یا H1 استفاده کن). استراتژی این رو خودکار بررسی می‌کنه و اگه برابر یا بزرگ‌تر بود، به TF یک پله کوچیک‌تر سوییچ می‌کنه (با Warning log).

### 9.2 Momentum Layer

| فیلد | نوع | پیش‌فرض | توضیح |
|---|---|---|---|
| `RsiPeriod` | int | 14 | دوره RSI |
| `RsiBullishThreshold` | decimal | 55 | RSI ≥ این → Bullish Momentum |
| `RsiBearishThreshold` | decimal | 45 | RSI ≤ این → Bearish Momentum |
| `MacdFast` | int | 12 | MACD fast EMA |
| `MacdSlow` | int | 26 | MACD slow EMA |
| `MacdSignal` | int | 9 | MACD signal EMA |

### 9.3 Volume Baselines

| فیلد | نوع | پیش‌فرض | توضیح |
|---|---|---|---|
| `IntraDayBaselineCandles` | int | 100 | تعداد کندل برای baseline روزانه |
| `OpeningBaselineDays` | int | 30 | تعداد روز برای baseline opening |
| `MinBaselineSamples` | int | 10 | حداقل نمونه برای baseline معتبر |
| `VolumeMultiplier` | decimal | 1.5 | ضریب baseline (1.5x = سیگنال) |

### 9.4 Order Book

| فیلد | نوع | پیش‌فرض | توضیح |
|---|---|---|---|
| `BuyRatioThreshold` | decimal | 1.1 | BuyRatio ≥ 1.1 → Bullish |
| `SellRatioThreshold` | decimal | 1.1 | SellRatio ≥ 1.1 → Bearish |
| `OrderBookDepthRatio` | decimal | 1.5 | BidDepth/AskDepth ≥ 1.5 → Bullish |

### 9.5 Weights

| فیلد | نوع | پیش‌فرض | توضیح |
|---|---|---|---|
| `TrendWeight` | decimal | 0.40 | وزن Layer 1 در Strength |
| `MomentumWeight` | decimal | 0.35 | وزن Layer 2 |
| `MicroWeight` | decimal | 0.25 | وزن Layer 3 |

**قید**: مجموع باید = 1.0. استراتژی در Initialize بررسی می‌کنه.

### 9.6 Signal Thresholds

| فیلد | نوع | پیش‌فرض | توضیح |
|---|---|---|---|
| `MinSignalStrength` | decimal | 0.65 | حداقل Strength برای سیگنال |
| `MinStrengthDiff` | decimal | 0.05 | حداقل اختلاف Bullish و Bearish |

### 9.7 SL/TP

| فیلد | نوع | پیش‌فرض | توضیح |
|---|---|---|---|
| `AtrPeriod` | int | 14 | دوره ATR |
| `AtrStopMultiplier` | decimal | 2.0 | فاصله SL = 2×ATR از Entry |
| `AtrTakeProfitMultiplier` | decimal | 4.0 | فاصله TP = 4×ATR از Entry |

### 9.8 Intra-Candle

| فیلد | نوع | پیش‌فرض | توضیح |
|---|---|---|---|
| `EnableIntraCandleDecision` | bool | true | فعال/غیرفعال |
| `SubCandleTimeFrame` | TimeFrame | M5 | TF کندل‌های ریز |
| `SubCandleHistoryDepth` | int | 200 | تعداد کندل M5 برای baseline |
| `IntraCandleVolumeThreshold` | decimal | 2.0 | ratio ≥ 2 → trigger |

---

## ۱۰. سناریوهای نمونه

### سناریو ۱: خرید موفق

 
زمان: سه‌شنبه، ۱۰:۰۰ صبح
Primary TF: H1، Trend TF: D1

الآن:
  کندل ۱۰:۰۰-۱۱:۰۰ در حال شکل‌گیری (۱۵ دقیقه گذشته)

داده‌ها:
  D1: EMA20=105، EMA50=100 (دیروز 98)
      Price=107 → TrendBullish = 1.0 ✅

  H1 close کندل ۹:۰۰-۱۰:۰۰:
      RSI = 62 → RsiBullish = 0.35
      MACD hist = +0.8 (دیروز +0.5) → MacdBullish = 1.0
      MomentumBullish = (0.35+1.0)/2 = 0.675

  Volume (15 دقیقه از H1):
    Sub-Candle M5 ها: مجموع 90,000 سهم
    IntraDay_FirstHalf_Baseline = 100,000
    time_fraction = 15/30 = 0.5
    expected = 100,000 × 0.5 = 50,000
    ratio = 90,000 / 50,000 = 1.8
    1.8 < 2.0 → intra-candle trigger نشده

  → صبر تا ۱۱:۰۰ که کندل بسته بشه

زمان ۱۱:۰۰ (کندل بسته شده):
  H1 close کندل ۱۰:۰۰-۱۱:۰۰:
      volume کامل = 180,000 سهم (دو برابر معمول)
      RSI = 60، MACD hist = +0.9
      BuyRatio = 1.3، DepthRatio = 1.8

  VolumeBullish = min(1, 180,000/100,000/1.5) = min(1, 1.2) = 1.0
  OrderBookBullish = (clamp(1.3/1.1) + clamp(1.8/1.5)) / 2
                   = (1.0 + 1.0) / 2 = 1.0
  MicroBullish = (1.0 + 1.0) / 2 = 1.0

  BullishStrength = 0.4×1.0 + 0.35×0.66 + 0.25×1.0 = 0.4+0.231+0.25 = 0.881 ✅

  Bullish=0.881، Bearish≈0 (Trend نزولی نیست)
  diff = 0.881 ≥ 0.05 ✅
  Strength ≥ 0.65 ✅

  → Buy Signal با Strength=0.881
  → ATR=300 → SL = 10,700-600 = 10,100، TP = 10,700+1200 = 11,900
 

### سناریو ۲: رد سیگنال کاذب

 
زمان: چهارشنبه، ۱۴:۳۰
Primary TF: H1

داده‌ها:
  D1: EMA20=105، EMA50=103 (دیروز 104) → نه Rising، ولی Price=106 > EMA50
      TrendBullish = 0.5 (نصفه)

  H1: RSI=58 → RsiBullish=0.15
      MACD hist = -0.2 (تازه منفی شده) → MacdBullish=0
      MomentumBullish = (0.15+0)/2 = 0.075

  Volume: عادی → VolumeBullish = 0.4
  OrderBook: BuyRatio=0.9، DepthRatio=0.7
      OrderBookBullish = (clamp(0.9/1.1)+clamp(0.7/1.5))/2 = (0.82+0.47)/2 = 0.645
  MicroBullish = (0.4+0.645)/2 = 0.52

  Bullish = 0.4×0.5 + 0.35×0.075 + 0.25×0.52 = 0.2+0.026+0.13 = 0.356

  0.356 < 0.65 → Hold ✅ (سیگنال کاذب رد شد)
 

### سناریو ۳: Opening Anomaly درست تشخیص داده شد

 
زمان: شنبه، ۹:۱۵ صبح (کندل اول روز، ۱۵ دقیقه گذشته)
Primary TF: H1

حجم کندل ۹:۰۰-۹:۱۵: 500,000 سهم
Opening_FirstHalf_Baseline = 800,000 سهم (median ۳۰ روز اخیر)
IntraDay_FirstHalf_Baseline = 150,000 سهم

چون اول-روز هست:
    expected = 800,000 × 0.5 = 400,000
    ratio = 500,000/400,000 = 1.25
    1.25 < 2.0 → trigger نشده، صبر

اگه اشتباهاً IntraDay_Baseline استفاده می‌شد:
    expected = 150,000 × 0.5 = 75,000
    ratio = 500,000/75,000 = 6.67 ≥ 2.0 → trigger کاذب! ❌

→ Dual Baseline از این جلوگیری کرد ✅
 

---

## ۱۱. حالت‌های خاص و Edge Cases

### 11.1 داده کافی نیست

| سناریو | رفتار |
|---|---|
| RequiredCandleCount کافی نیست | Hold با features (Warmup) |
| IntraDay baseline < MinBaselineSamples | Hold (Volume=0) |
| Opening baseline < MinBaselineSamples | Hold فقط برای Opening candle (بقیه کندل‌ها با IntraDay ok) |
| ATR کافی نیست | SL/TP=null (با Warning) |
| Order Book موجود نیست | OrderBook Component حذف می‌شه (Micro فقط Volume) |

### 11.2 کندل‌های First/Last روز

- **First candle** → Opening_Baseline (خودکار)
- **Last candle** → همچنان IntraDay_Baseline (پیش‌فرض). اگه دیدیم حجم آخر وقت مشکل ایجاد می‌کنه، در Config بعدی اضافه می‌کنیم (`LastCandleBaseline`)

### 11.3 TrendTimeFrame > PrimaryTimeFrame

اگه کاربر `PrimaryTimeFrame=D1` و `TrendTimeFrame=D1` گذاشت:
- استراتژی خودکار `TrendTimeFrame` رو به H4 تغییر می‌ده
- Warning در logs

### 11.4 Intra-Candle برای TF کوچک

اگه `PrimaryTimeFrame = M1` یا `M5`:
- `EnableIntraCandleDecision` خودکار غیرفعال می‌شه
- دلیل: Sub-Candle تفاوت معناداری نمی‌سازه

### 11.5 Backtest بدون MarketSnapshot

Backtest Engine فعلی snapshot نداره. پس:
- OrderBookBullish = null
- MicroBullish = VolumeBullish
- Result: Strength احتمالاً پایین‌تر از Live (pessimistic)

### 11.6 نحوه تشخیص "روز جدید" در Backtest

Backtest کندل‌ها رو از DB می‌خونه. بین دو کندل متوالی، فاصله > ۶ ساعت = روز جدید. این منطق در Backtest و Live یکسانه.

---

## ۱۲. محدودیت‌ها — این استراتژی چه کاری نمی‌کنه

### 12.1 چه کارهایی نمی‌کنه

- ❌ **Trailing Stop** — SL ثابت بعد از Entry. Trailing نیاز به منطق پیچیده‌تر داره (Phase 2).
- ❌ **Laddered Exit** (فروش تدریجی) — Sell یا همه یا هیچ. Partial exit در Phase 2.
- ❌ **Time-based exit** — "اگه ۳ روز باز بود، ببند" نداریم.
- ❌ **Multi-Symbol correlation** — فرض می‌کنه هر Symbol مستقله.
- ❌ **News/Event awareness** — تقویم اقتصادی نمی‌بینه.
- ❌ **Sentiment analysis** — از Telegram/Twitter خبر نداره.
- ❌ **Adaptive parameters** — پارامترها ثابت‌ان، خودشون رو تنظیم نمی‌کنن.

### 12.2 وابستگی‌ها

- **Strategy مستقل از Account** — همون‌طور که در معماری تصمیم گرفته شده (D1). Run می‌تونه روی چند Account فَن‌اوت بشه.
- **Strategy مستقل از Broker** — Broker در Stage 3 انتخاب می‌شه.
- **Order Book فقط Live یا Backtest با snapshot** — در Backtest بدون snapshot، OrderBook استفاده نمی‌شه.

### 12.3 فرض‌های ضمنی

- **کندل‌ها UTC هستن** — conversion به Tehran در Level دیگری (BacktestEngine/Stage 4) انجام می‌شه.
- **قیمت در کندل، IRR هست** — TargetCurrency در strategy stream استفاده می‌شه (D57)، ولی SL/TP در IRR باید محاسبه بشه. **در پیاده‌سازی باید حواسمون به این باشه** — استراتژی باید در currency تبدیل‌شده تصمیم بگیره ولی SL/TP رو در IRR بده. این complexity رو در فاز پیاده‌سازی حل می‌کنیم.

### 12.4 Performance

- **Per-candle**: O(1) محاسبات به‌جز EMA/MACD/ATR که **incremental** هستن (cache می‌شن).
- **Baseline**: یک‌بار در ابتدای Backtest محاسبه می‌شن.
- **Intra-Candle**: برای هر کندل جاری، یک‌بار sum sub-candles. برای Sub-Candle 200 کندل، O(200) — قابل اغماض.
- **Memory**: ~۱۰۰KB per strategy instance.

---

## ۱۳. راهنمای Tuning (بعد از Backtest)

### 13.1 علائم و درمان

| علامت | علت احتمالی | راه‌حل |
|---|---|---|
| **۰-۱ سیگنال در ماه** | خیلی محافظه‌کار | `MinSignalStrength`: 0.65 → 0.55<br>`TrendWeight`: 0.4 → 0.3 (Micro مؤثرتر)<br>یا `VolumeMultiplier`: 1.5 → 1.2 |
| **Win Rate زیر ۴۰٪** | سیگنال کاذب زیاد | `MinSignalStrength`: 0.65 → 0.70<br>`MinStrengthDiff`: 0.05 → 0.10<br>`TrendWeight`: 0.4 → 0.5 |
| **Profit Factor < 1** | R:R اشتباه یا Commission خورده | `AtrTakeProfitMultiplier`: 4 → 5 (R:R=2.5:1)<br>Commission در Backtest چک کن |
| **Max DD بالا** | SL بزرگ | `AtrStopMultiplier`: 2.0 → 1.5 |
| **سیگنال زیاد ولی بازده کم** | Overtrading | `MinSignalStrength`: → 0.70 |
| **Sell زیاد، Win کم** | Sell هم باید threshold بالاتر داشته باشه | فعلاً هم threshold. اگه مشکل بود، در Config جدا اضافه می‌کنیم |

### 13.2 Order توصیه‌شده

بعد از اولین Backtest:

1. **با Config پیش‌فرض اجرا کن** (روی ۵-۱۰ نماد، ۱-۲ سال)
2. **نتیجه رو تحلیل کن**: تعداد سیگنال، Win Rate، Profit Factor، Max DD
3. **اگه مشکل بود، فقط یک پارامتر در هر iteration تغییر بده** — نه چند تا با هم (تا اثرش رو ببینی)
4. **مقایسه**: هر تغییر رو با baseline مقایسه کن

### 13.3 Config های پیشنهادی برای تست

**A. محافظه‌کارانه**:
 
MinSignalStrength = 0.70
TrendWeight = 0.5, MomentumWeight = 0.3, MicroWeight = 0.2
AtrStopMultiplier = 1.5
VolumeMultiplier = 1.5
 

**B. پیش‌فرض** (شروع):
 
MinSignalStrength = 0.65
0.4 / 0.35 / 0.25
AtrStop = 2.0، AtrTP = 4.0
VolumeMultiplier = 1.5
 

**C. تهاجمی** (اگه تعداد سیگنال کم بود):
 
MinSignalStrength = 0.55
TrendWeight = 0.3, MomentumWeight = 0.4, MicroWeight = 0.3
AtrStopMultiplier = 2.5 (بیشتر فضای نفس)
VolumeMultiplier = 1.2
 

---

## ۱۴. Glossary

| اصطلاح | معنی |
|---|---|
| **EMA** | Exponential Moving Average — میانگین متحرک با تأکید بر داده‌های اخیر |
| **RSI** | Relative Strength Index — شاخص قدرت نسبی (۰-۱۰۰) |
| **MACD** | Moving Average Convergence Divergence — تفاضل دو EMA |
| **ATR** | Average True Range — میانگین دامنه‌ی واقعی (نوسان) |
| **Baseline** | خط پایه — عدد مرجع که با آن مقایسه می‌کنیم |
| **Bullish / Bearish** | صعودی / نزولی |
| **Strength** | قدرت سیگنال (۰-۱) |
| **Threshold** | آستانه — حداقل مقدار برای فعال شدن |
| **Intra-Candle** | درون کندل — قبل از بسته شدن کندل تصمیم گرفتن |
| **Sub-Candle** | کندل ریزتر (مثلاً M5 داخل H1) |
| **Opening Candle** | اولین کندل روز |
| **Order Book** | دفتر سفارش — صف خرید و فروش |
| **Buy Ratio / Sell Ratio** | نسبت خرید / فروش در صف |
| **Depth** | عمق صف — مقدار سهم در بهترین قیمت |
| **R:R** | Risk-to-Reward — نسبت ریسک به بازده |
| **SL / TP** | Stop Loss / Take Profit — حد ضرر / حد سود |
| **Win Rate** | درصد معاملات برنده |
| **Profit Factor** | مجموع سود / مجموع ضرر |
| **Max DD** | حداکثر افت سرمایه |

---

## پیوست A — خلاصه در یک صفحه

**MtfCompositeStrategy** یک استراتژی چندلایه است که:

1. **سه دیدگاه مستقل** از بازار می‌گیره: Trend (D1 EMA)، Momentum (RSI + MACD)، Micro (Volume + Order Book)

2. **هر دیدگاه یک Score** بین ۰ تا ۱ می‌ده (Bullish و Bearish جداگانه)

3. **Weighted combination** با وزن ۰.۴/۰.۳۵/۰.۲۵ — نه AND، نه OR

4. **Threshold + MinDiff** برای فیلتر کردن سیگنال‌های ضعیف و جلوگیری از flip-flop

5. **SL/TP بر اساس ATR** — تطبیق‌پذیر با نوسان هر سهم. **Run override** می‌کنه.

6. **Intra-Candle** — قبل از بسته شدن کندل با sub-candle ها حجم رو بررسی می‌کنه

7. **Opening Anomaly** — با Dual Baseline، کندل اول روز رو جدا تحلیل می‌کنه (نه Ignore، نه با baseline اشتباه)

8. **Symmetric** — Buy و Sell یکسان (فقط جهت معکوس)

**فلسفه**: کمتر معامله، باکیفیت‌تر. اگه همه لایه‌ها هم‌سو نبودن، صبر.

---

## پیوست B — تفاوت با RSI Strategy

| موضوع | RSI Strategy | MtfComposite Strategy |
|---|---|---|
| **تعداد لایه** | ۱ (RSI) | ۳ (Trend + Momentum + Micro) |
| **Strength** | یک عدد از RSI | Weighted average |
| **TimeFrame** | فقط Primary | Multi-TF (Trend از D1، سیگنال از H1) |
| **Volume** | ❌ | ✅ با baseline |
| **Order Book** | ❌ | ✅ |
| **SL/TP** | از Run فقط | ATR-based (استراتژی پیشنهاد، Run override) |
| **Intra-Candle** | ❌ | ✅ |
| **Opening Handling** | ❌ | ✅ Dual baseline |
| **Symmetric** | ✅ | ✅ |
| **پیچیدگی** | پایین | متوسط-بالا |
| **تعداد سیگنال** | متوسط | کم (محافظه‌کارانه) |
| **کیفیت سیگنال** | متوسط | بالا |
