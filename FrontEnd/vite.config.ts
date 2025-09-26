// vite.config.ts
import { defineConfig } from "vite";
import tailwindcss from "@tailwindcss/vite";
import react from "@vitejs/plugin-react";
import { fileURLToPath } from "url";
import { dirname, resolve } from "path";

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

export default defineConfig({
  plugins: [tailwindcss(), react()],
  resolve: {
    alias: {
      "@": resolve(__dirname, "src"),
    },
  },
  build: {
    rollupOptions: {
      input: {
        admin: resolve(__dirname, "src/apps/MaharRayanesh/AdminPanel/main.tsx"),
        website: resolve(__dirname, "src/apps/MaharRayanesh/WebSite/main.tsx"),
      },
    },
  },
});
