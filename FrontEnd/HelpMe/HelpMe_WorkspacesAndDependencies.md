# 📖 راهنمای مدیریت وابستگی‌ها (Dependencies) در Nexus Frontend

این پروژه از **pnpm workspaces** استفاده می‌کند تا چندین اپلیکیشن (apps) و ماژول (core) رو به‌صورت یکپارچه مدیریت کنیم.  
این ساختار باعث میشه:
- 📦 پکیج‌های مشترک فقط یک‌بار نصب بشن (سبک‌تر و سریع‌تر)
- 🚀 هر پروژه بتونه پکیج‌های مخصوص خودش رو داشته باشه
- 🔄 تیم راحت‌تر پروژه‌های جدید اضافه کنه یا وابستگی‌ها رو مدیریت کنه

---

## 🗂 ساختار پروژه
```
nexus-frontend/
  ├── package.json            # Root package.json (وابستگی‌های مشترک)
  ├── pnpm-workspace.yaml     # تعریف workspaceها
  ├── src/
  │   ├── core/               # ماژول Core (کد و ابزار مشترک بین اپ‌ها)
  │   └── apps/               # اپلیکیشن‌ها
  │       ├── MaharRayanesh/
  │       │   ├── AdminPanel/
  │       │   │   └── package.json   # فقط وابستگی‌های خاص AdminPanel
  │       │   └── Website/
  │       │       └── package.json   # فقط وابستگی‌های خاص Website
  │       └── PMIS/
  │           └── package.json       # فقط وابستگی‌های خاص PMIS
```

---

## 🛠 انواع وابستگی‌ها

### 1️⃣ **وابستگی‌های عمومی (Shared Dependencies)**
- پکیج‌هایی که همه‌ی پروژه‌ها و core نیاز دارن.
- باید در **root `package.json`** اضافه بشن.
- مثال‌ها:
  - `react`, `react-dom`
  - `react-router-dom`
  - `axios`
  - `tailwindcss`, `@tailwindcss/vite`
  - `@tanstack/react-query`
  - ابزارهای build/lint (مثل `vite`, `eslint`, `typescript`)

👉 نصب در root:
```bash
pnpm add axios
pnpm add -D typescript
```

---

### 2️⃣ **وابستگی‌های مخصوص یک اپلیکیشن**
- فقط در یک اپلیکیشن استفاده میشه (بقیه نیازی ندارن).
- باید در **package.json همون اپ** اضافه بشه.
- مثال‌ها:
  - `@mui/material`, `@mui/icons-material` (فقط PMIS)
  - `recharts` (فقط در بعضی اپ‌ها)
  - `react-multi-date-picker` (فقط PMIS)

👉 نصب برای یک اپ خاص (مثلاً pmis):
```bash
pnpm --filter pmis add @mui/material
```

👉 نصب devDependency برای یک اپ خاص:
```bash
pnpm --filter pmis add -D daisyui
```

---

### 3️⃣ **وابستگی‌های core**
- اگر core به پکیجی نیاز داشت که عمومی نیست، می‌تونه در **package.json خودش (`src/core/package.json`)** تعریف کنه.
- اما اگر اون پکیج توسط همه‌ی اپ‌ها استفاده میشه، بهتره منتقل بشه به root.

---

## 🚀 افزودن پروژه جدید
1. پوشه جدید در `src/apps/` بساز (مثلاً `NewApp/`).
2. یک فایل `package.json` خالی برای وابستگی‌های خاصش ایجاد کن:
   ```json
   {
     "name": "newapp",
     "version": "0.0.0",
     "private": true,
     "scripts": {
       "dev": "vite",
       "build": "vite build",
       "preview": "vite preview"
     },
     "dependencies": {},
     "devDependencies": {}
   }
   ```
3. اسکریپت‌های root رو برای newapp مشابه بقیه اپ‌ها اضافه کن:
   ```json
   "dev:newapp": "pnpm --filter newapp run dev",
   "build:newapp": "pnpm --filter newapp run build",
   "preview:newapp": "pnpm --filter newapp run preview"
   ```

---

## ⚡️ دستورهای مهم pnpm
- نصب همه پکیج‌ها (root + apps + core):
  ```bash
  pnpm install
  ```
- افزودن پکیج عمومی به root:
  ```bash
  pnpm add axios
  ```
- افزودن پکیج مخصوص یک اپ:
  ```bash
  pnpm --filter pmis add @mui/x-data-grid
  ```
- اجرای dev یک اپ خاص:
  ```bash
  pnpm run dev:trader.server
  ```
- اجرای build یک اپ خاص:
  ```bash
  pnpm run build:mahar.admin
  ```

---

## ✅ نکات مهم
- هیچ‌وقت پکیج مشترک رو در اپ‌ها نصب نکن، همیشه بیار توی root.  
- اپ‌ها فقط باید پکیج‌های خاص خودشون رو نگه دارن.  
- بعد از هر تغییر در package.json یادت باشه `pnpm install` بزنی.  
- `pnpm-lock.yaml` همیشه باید commit بشه (نسخه دقیق پکیج‌ها رو نگه میداره).  

---

📌 با این ساختار، پروژه هم **ماژولار**ه، هم **بهینه**، و هم برای تیم توسعه خیلی شفافه.
