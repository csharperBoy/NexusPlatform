// import React from "react";
// import ReactDOM from "react-dom/client";
// import { BrowserRouter } from "react-router-dom";
// import App from "./App";
// import "@/index.css";
// import { UIProvider } from "@/core/context/UIProvider";
// import { ModuleProvider } from "@/core/context/ModuleContext";

// ReactDOM.createRoot(document.getElementById("root")!).render(
//   <React.StrictMode>
//     <BrowserRouter>
//       <UIProvider initialTheme="Light" initialStyle="Flat">
//         <ModuleProvider>
//           <App />
//         </ModuleProvider>
//       </UIProvider>
//     </BrowserRouter>
//   </React.StrictMode>,
// );



// src/apps/AKSteel/Admin/main.tsx
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
import { TraderModuleRegistration } from "@/modules/Trader";

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
              <RequireModuleActive moduleName="Trader">
                <TraderModuleRegistration />
              </RequireModuleActive>
              <App />
            </ModuleProvider>
          </DashboardProvider>
        </UIProvider>
      </AuthProvider>
    </BrowserRouter>
  </React.StrictMode>
);
