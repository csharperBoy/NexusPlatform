// src/apps/AKSteel/Admin/capacitor.config.ts
import { CapacitorConfig } from '@capacitor/cli';

const config: CapacitorConfig = {
  appId: 'com.aksteel.admin',
  appName: 'AKSteel Admin',
  webDir: '../../../../dist/AKSteelAdmin',
  plugins: {
    CapacitorCookies: {
      enabled: true,
    },
    CapacitorHttp: {
      enabled: true,
    }
  },
  server: {
    androidScheme: 'https',
    cleartext: true
  }
};

export default config;

/*
pnpm run icon:aksteel.admin    // برای تغییر آیکون برنامه اندروید که توی package.json تعیین شده است
pnpm run build:aksteel.admin     // ساخت خروجی وب
pnpm run cap:add:aksteel.admin   // افزودن پلتفرم اندروید    
pnpm run cap:sync:aksteel.admin  // همگام‌سازی تغییرات وب با پروژه اندروید
***********************************************************

pnpm run apk:aksteel.admin       // ساخت فایل APK برای اندروید   - همه مراحل بالا رو انجام میدهد و خروجی رو توی پوشه dist/android-build قرار میدهد

*************************************************
پیش نیازها:
۱. نصب Android Studio
نرم‌افزار Android Studio را دانلود و نصب کن. در مراحل نصب مطمئن شو گزینه Android SDK علامت خورده باشد.

۲. تنظیم متغیر JAVA_HOME در ویندوز
اندروید استودیو نسخه Java JDK را به همراه دارد. پس از نصب، باید مسیر آن را به ویندوز معرفی کنی:

کلیدهای Win + R را بزن، عبارت sysdm.cpl را تایپ کن و Enter بزن تا پنجره System Properties باز شود.

به تب Advanced برو و روی دکمه Environment Variables کلیک کن.

در بخش System variables (بخش پایینی)، روی دکمه New کلیک کن:

Variable name: JAVA_HOME

Variable value: C:\Program Files\Android\Android Studio\jbr

روی OK کلیک کن.

در همان بخش System variables، متغیر Path را پیدا کن، روی آن کلیک کرده و Edit را بزن.

روی New کلیک کن و عبارت %JAVA_HOME%\bin را اضافه کن و همه پنجره‌ها را OK کن.

***************************************************************************** 
اگر باز هم خطا داشتیم ولی توی اندروید استودیو پروژه بیلد میشد احتمالا بخاطر نسخه جاواست. اینکار رو بکن:
پروژه را در اندروید استودیو باز کنید.

از منوی بالا به مسیر File > Settings > Build, Execution, Deployment > Build Tools > Gradle بروید.

مقدار عبارت Gradle JDK را نگاه کنید و مسیر دقیق آن را کپی کنید (معمولاً در مسلیر زیر است):
C:\Users\<Your-Username>\.jdks\jbr-21... یا مسیرهای مشابه در پوشه .jdks

۲. بروزرسانی متغیر JAVA_HOME در ویندوز
کلیدهای Win + R را بزنید، sysdm.cpl را تایپ کرده و Enter بزنید.

به تب Advanced رفته و روی Environment Variables کلیک کنید.

متغیر JAVA_HOME را پیدا کنید، روی Edit بزنید و مسری که در مرحله قبل کپی کردید را جایگزین کنید.

همه پنجره‌ها را OK کنید.

ترمینال یا VS Code را کامل ببندید و دوباره باز کنید.


*/