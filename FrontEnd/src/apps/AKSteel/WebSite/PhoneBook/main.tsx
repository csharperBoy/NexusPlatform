// src/apps/AKSteel/Website/PhoneBook/main.tsx
import React from "react";
import ReactDOM from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import App from "./App";
import "@/index.css";
import { UIProvider } from "@/core/context/UIProvider";
import { ModuleProvider } from "@/core/context/ModuleContext";
import { RequireModuleActive } from "@/core/components/RequireModuleActive";

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <BrowserRouter>
        <UIProvider initialTheme="Light" initialStyle="Flat">
            <ModuleProvider>
             
              <App />
            </ModuleProvider>
        </UIProvider>
    </BrowserRouter>
  </React.StrictMode>
);
