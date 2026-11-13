// src/apps/Trader/main.tsx
import React from "react";
import ReactDOM from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import App from "./App";
import "@/index.css";
import { AuthProvider } from "@/modules/Identity";
import { UIProvider } from "@/core/context/UIProvider";
import { DashboardProvider } from "@/modules/DashboardCore";
import { IdentityModuleRegistration } from "@/modules/Identity/IdentityModuleRegistration";
import { AuthorizationModuleRegistration } from "@/modules/Authorization/AuthorizationModuleRegistration";
import { ModuleProvider } from "@/core/context/ModuleContext";
import { RequireModuleActive } from "@/core/components/RequireModuleActive";
import { TraderModuleRegistration } from "@/modules/Trader";

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 5_000,
      refetchOnWindowFocus: false,
      retry: 1,
    },
  },
});

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <AuthProvider>
          <UIProvider initialTheme="Light" initialStyle="Flat">
            <DashboardProvider>
              <ModuleProvider>
              {/* ثبت ماژول‌ها فقط اگر فعال باشند */}
                <RequireModuleActive moduleName="Identity">
                  <IdentityModuleRegistration />
                </RequireModuleActive>
                <RequireModuleActive moduleName="Authorization">
                  <AuthorizationModuleRegistration />
                </RequireModuleActive>
                <RequireModuleActive moduleName="Trader">
                  <TraderModuleRegistration />
                </RequireModuleActive>
                <App />
              </ModuleProvider>
            </DashboardProvider>
          </UIProvider>
        </AuthProvider>
      </BrowserRouter>
    </QueryClientProvider>
  </React.StrictMode>
);