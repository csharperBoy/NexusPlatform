//src/apps/Trader/Server/main.tsx
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

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <BrowserRouter>
      <AuthProvider>
        {/* فقط AdminPanel با تم admin و استایل flat */}
        <UIProvider initialTheme="Light" initialStyle="Flat">
           <DashboardProvider>
            {/* ثبت ماژول‌ها */}
            <IdentityModuleRegistration />
             <AuthorizationModuleRegistration /> 
            {/* سایر ماژول‌ها مانند SalesModuleRegistration */}

            <App />
          </DashboardProvider>
        </UIProvider>
      </AuthProvider>
    </BrowserRouter>
  </React.StrictMode>
);
