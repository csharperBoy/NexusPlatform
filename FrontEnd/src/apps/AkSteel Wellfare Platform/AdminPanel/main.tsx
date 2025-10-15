//src/apps/AkSteel Wellfare Platform/AdminPanel/main.tsx
import React from "react";
import ReactDOM from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import App from "./App";
import "@/index.css"; 
import { AuthProvider } from "@/modules/auth";
import { UIProvider } from "@/core/context/UIProvider";

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <BrowserRouter>
      <AuthProvider>
        {/* فقط AdminPanel با تم admin و استایل flat */}
        <UIProvider initialTheme="Light" initialStyle="Flat">
          <App />
        </UIProvider>
      </AuthProvider>
    </BrowserRouter>
  </React.StrictMode>
);
