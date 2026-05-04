// src/apps/Trader/Server/main.tsx
import React from "react";
import ReactDOM from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import App from "./App";
import "@/index.css";
import { AuthProvider } from "@/modules/Identity";
import { UIProvider } from "@/core/context/UIProvider";
import { DashboardProvider } from "@/modules/DashboardCore";
import { IdentityModuleRegistration } from "@/modules/Identity/IdentityModuleRegistration";
import { AuthorizationModuleRegistration } from "@/modules/Authorization/AuthorizationModuleRegistration";
import { ModuleProvider } from "@/core/context/ModuleContext";
import { RequireModuleActive } from "@/core/components/RequireModuleActive";

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
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
              {/* ... سایر ماژول‌های خود را اینجا اضافه کنید */}

              <App />
            </ModuleProvider>
          </DashboardProvider>
        </UIProvider>
      </AuthProvider>
    </BrowserRouter>
  </React.StrictMode>
);
