// import { defineConfig, loadEnv } from "vite";
// import tailwindcss from "@tailwindcss/vite";
// import react from "@vitejs/plugin-react";
// import { fileURLToPath } from "url";
// import { dirname, resolve } from "path";
// import basicSsl from "@vitejs/plugin-basic-ssl";

// const __filename = fileURLToPath(import.meta.url);
// const __dirname = dirname(__filename);

// export default defineConfig(({ mode }) => {
//   const env = loadEnv(mode, resolve(__dirname, "src/apps/Trader"));
//   const isDev = mode.includes("development");

//   return {
//     root: resolve(__dirname, "src/apps/Trader"),
//     plugins: [
//       tailwindcss(),
//       react(),
//       isDev && basicSsl(),
//     ].filter(Boolean),
//     resolve: {
//       alias: { "@": resolve(__dirname, "src") },
//     },
//     server: {
//       port: 5174,
//       https: isDev ? {} : undefined,
//       fs: {
//         allow: [resolve(__dirname)],
//       },
//       proxy: {
//         /* ─── EasyTrader API (سفارش، سینک ساعت) ─── */
//         "/easytrader": {
//           target: "https://api-mts.orbis.easytrader.ir",
//           changeOrigin: true,
//           secure: true,
//           rewrite: (p) => p.replace(/^\/easytrader/, ""),
//         },
//         /* ─── Auth Server (لاگین) ─── */
//         "/authserver": {
//           target: "http://localhost:5000",
//           changeOrigin: true,
//           rewrite: (p) => p.replace(/^\/authserver/, ""),
//         },
//       },
//     },
//     build: {
//       outDir: resolve(__dirname, "dist/Trader"),
//       emptyOutDir: true,
//       rollupOptions: {
//         input: resolve(__dirname, "src/apps/Trader/index.html"),
//       },
//     },
//     define: {
//       __APP_ENV__: JSON.stringify(env),
//     },
//   };
// });


// vite.config.trader.admin.ts
import { defineConfig, loadEnv } from "vite";
import tailwindcss from "@tailwindcss/vite";
import react from "@vitejs/plugin-react";
import { fileURLToPath } from "url";
import { dirname, resolve } from "path";
import basicSsl from '@vitejs/plugin-basic-ssl';

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

export default defineConfig(({ mode }) => {
  // env مخصوص WebSite
  const env = loadEnv(mode, resolve(__dirname, "src/apps/Trader"));
const isDev =  mode.includes('development');
  return {
    root: resolve(__dirname, "src/apps/Trader"),
    plugins: [
      tailwindcss(),
      react(),
      isDev && basicSsl(), // فقط در حالت توسعه
    ].filter(Boolean), // حذف مقادیر false
    resolve: {
      alias: {
        "@": resolve(__dirname, "src"),
      },
    },
    server: {
      port: 5174,
        https: isDev ? {} : undefined, // فقط در حالت توسعه فعال باشد
        fs: {
        // اجازه دسترسی Vite به کل ریشه پروژه
        allow: [resolve(__dirname)],
      },
    },
    build: {
      outDir: resolve(__dirname, "dist/Trader"),
      emptyOutDir: true,
      rollupOptions: {
        input: resolve(__dirname, "src/apps/Trader/index.html"),
      },
    },
    define: {
      __APP_ENV__: JSON.stringify(env),
    },
  };
});
