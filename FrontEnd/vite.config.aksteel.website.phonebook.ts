// vite.config.aksteel.website.phonebook.ts
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
  const env = loadEnv(mode, resolve(__dirname, "src/apps/AKSteel/WebSite/PhoneBook"));
const isDev =  mode.includes('development');
  return {
    root: resolve(__dirname, "src/apps/AKSteel/WebSite/PhoneBook"),
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
      port: 5173,
        https: isDev ? {} : undefined, // فقط در حالت توسعه فعال باشد
    },
    build: {
      outDir: resolve(__dirname, "dist/AKSteelWebSitePhoneBook"),
      emptyOutDir: true,
      rollupOptions: {
        input: resolve(__dirname, "src/apps/AKSteel/WebSite/PhoneBook/index.html"),
      },
    },
    define: {
      __APP_ENV__: JSON.stringify(env),
    },
  };
});
