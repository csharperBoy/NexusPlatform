// src/core/context/UIProvider.tsx
import React, { createContext, useContext, useState, ReactNode } from "react";

// تعریف انواع
export type Theme = "light" | "dark" | "admin";
export type Style = "flat" | "3d" | "glass";

type UIConfig = {
  theme: Theme;
  style: Style;
  setTheme: (theme: Theme) => void;
  setStyle: (style: Style) => void;
};

const UIContext = createContext<UIConfig | undefined>(undefined);

export const UIProvider = ({ children }: { children: ReactNode }) => {
  const [theme, setTheme] = useState<Theme>("light");
  const [style, setStyle] = useState<Style>("flat");

  return (
    <UIContext.Provider value={{ theme, style, setTheme, setStyle }}>
      <div className={`theme-${theme} style-${style}`}>{children}</div>
    </UIContext.Provider>
  );
};

export const useUIConfig = () => {
  const ctx = useContext(UIContext);
  if (!ctx) throw new Error("useUIConfig must be used inside UIProvider");
  return ctx;
};
