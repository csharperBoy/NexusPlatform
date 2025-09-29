// src/core/context/UIProvider.tsx
// src/core/context/UIProvider.tsx
import React, { createContext, useContext, useState, ReactNode } from "react";

export type Theme = "Light" | "Dark" | "Auto";
export type Style = "Minimalist" | "MaterialDesign" | "Flat"  | "Glassmorphism" |  "Neumorphism";

type UIConfig = {
  theme: Theme;
  style: Style;
  setTheme: (theme: Theme) => void;
  setStyle: (style: Style) => void;
};

const UIContext = createContext<UIConfig | undefined>(undefined);

type UIProviderProps = {
  children: ReactNode;
  initialTheme?: Theme;
  initialStyle?: Style;
};

export const UIProvider = ({
  children,
  initialTheme = "Light",
  initialStyle = "Flat",
}: UIProviderProps) => {
  const [theme, setTheme] = useState<Theme>(initialTheme);
  const [style, setStyle] = useState<Style>(initialStyle);

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

