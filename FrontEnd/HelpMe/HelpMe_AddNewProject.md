# 📖 راهنمای اضافه‌کردن پروژه جدید در Frontend

فرض کن می‌خوای یه پروژه جدید به اسم `NewApp` اضافه کنی. این مراحل رو طی کن:

---

## 1️⃣ ایجاد ساختار پوشه
در مسیر زیر یک پوشه برای پروژه بساز:
```
src/apps/MaharRayanesh/NewApp
```

داخلش فایل‌های پایه رو ایجاد کن:
```
NewApp/
  ├── App.tsx
  ├── main.tsx
  ├── index.html
  └── pages/
       └── Dashboard.tsx
```

---

## 2️⃣ ساخت فایل‌های env
برای هر پروژه دو فایل env مخصوص dev و prod بساز (نام‌ها باید مطابق mode باشه):

```
.env.mahar.newapp.development
.env.mahar.newapp.production
```

مثال:
```bash
# .env.mahar.newapp.development
VITE_CURRENT_PROJECT=mahar.newapp
VITE_MAHAR_NEWAPP_AUTH_API=http://localhost:5003
VITE_MAHAR_NEWAPP_BILLING_API=http://localhost:5004

# .env.mahar.newapp.production
VITE_CURRENT_PROJECT=mahar.newapp
VITE_MAHAR_NEWAPP_AUTH_API=https://auth.newapp.com
VITE_MAHAR_NEWAPP_BILLING_API=https://billing.newapp.com
```

> ⚠️ توجه:  
> - مقدار `VITE_CURRENT_PROJECT` باید دقیقا با پروژه‌ات ست بشه.  
> - همه APIها باید با فرمت `VITE_<PROJECT>_<MODULE>_API` تعریف بشن.  
> - مثال بالا برای ماژول‌های `auth` و `billing` بود. هر ماژول جدید رو به همین سبک اضافه کن.

---

## 3️⃣ ساخت فایل Vite config اختصاصی
یک فایل جدید در روت پروژه ایجاد کن:
```
vite.config.newapp.ts
```

کد پیشنهادی:

```ts
import { defineConfig, loadEnv } from "vite";
import tailwindcss from "@tailwindcss/vite";
import react from "@vitejs/plugin-react";
import { fileURLToPath } from "url";
import { dirname, resolve } from "path";

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, resolve(__dirname, "src/apps/MaharRayanesh/NewApp"));

  return {
    root: resolve(__dirname, "src/apps/MaharRayanesh/NewApp"),
    plugins: [tailwindcss(), react()],
    resolve: {
      alias: {
        "@": resolve(__dirname, "src"),
      },
    },
    server: {
      port: 5175, // برای هر اپ یک پورت جدید بگذار
    },
    build: {
      outDir: resolve(__dirname, "dist/newapp"),
      emptyOutDir: true,
      rollupOptions: {
        input: resolve(__dirname, "src/apps/MaharRayanesh/NewApp/index.html"),
      },
    },
    define: {
      __APP_ENV__: JSON.stringify(env),
    },
  };
});
```

---

## 4️⃣ اضافه‌کردن اسکریپت‌ها به package.json
داخل `scripts` این‌ها رو اضافه کن:

```json
"dev:newapp": "vite --config vite.config.newapp.ts --mode mahar.newapp.development",
"build:newapp": "vite build --config vite.config.newapp.ts --mode mahar.newapp.production",
"preview:newapp": "vite preview --config vite.config.newapp.ts --port 5175"
```

---

## 5️⃣ اجرای پروژه
- برای dev:
  ```bash
  npm run dev:newapp
  ```
- برای build:
  ```bash
  npm run build:newapp
  ```
- برای preview:
  ```bash
  npm run preview:newapp
  ```

---

## 6️⃣ استفاده از APIها در کد
دیگه لازم نیست مستقیم آدرس API رو صدا بزنی.  
داخل `axiosClient.ts` همه APIها به صورت اتوماتیک از env خونده میشن.  

مثال:  
```ts
import getAPI from "@/core/api/axiosClient";

const authApi = getAPI("auth");    // میره سراغ VITE_MAHAR_NEWAPP_AUTH_API
const billingApi = getAPI("billing");  // میره سراغ VITE_MAHAR_NEWAPP_BILLING_API
```

---

✅ با این شش مرحله، پروژه جدیدت مثل `AdminPanel` و `WebSite` به راحتی ست میشه.  
فقط کافیه اسم‌ها (مثل newapp) رو با پروژه جدید جایگزین کنی.
