import { defineConfig } from "vite";
import tailwindcss from "@tailwindcss/vite";
import react from "@vitejs/plugin-react";
import { resolve } from "path";

export default defineConfig({
  plugins: [tailwindcss(), react()],
  resolve: {
    alias: {
      "@": resolve(__dirname, "src"), // یعنی از این به بعد "@/core/..." کار میکنه
    },
  },
  build: {
    rollupOptions: {
      input: {
        admin: resolve(__dirname, "src/apps/MaharRayanesh/AdminPanel/main.tsx"),
        website: resolve(
          __dirname,
          "src/apps/MaharRayanesh/WebSite/main.tsx"
        ),
      },
    },
  },
});
