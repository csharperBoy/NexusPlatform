// vite.config.website.ts
import { defineConfig, loadEnv } from "vite";
import tailwindcss from "@tailwindcss/vite";
import react from "@vitejs/plugin-react";
import { fileURLToPath } from "url";
import { dirname, resolve } from "path";

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

export default defineConfig(({ mode }) => {
  // env مخصوص WebSite
  const env = loadEnv(mode, resolve(__dirname, "src/apps/AkSteelWelfarePlatform/WebSite"));

  return {
    root: resolve(__dirname, "src/apps/AkSteelWelfarePlatform/WebSite"),
    plugins: [tailwindcss(), react()],
    resolve: {
      alias: {
        "@": resolve(__dirname, "src"),
      },
    },
    server: {
      port: 5174,
    },
    build: {
      outDir: resolve(__dirname, "dist/website"),
      emptyOutDir: true,
      rollupOptions: {
        input: resolve(__dirname, "src/apps/AkSteelWelfarePlatform/WebSite/index.html"),
      },
    },
    define: {
      __APP_ENV__: JSON.stringify(env),
    },
  };
});
