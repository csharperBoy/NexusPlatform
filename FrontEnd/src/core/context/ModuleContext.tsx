// src/core/context/ModuleContext.tsx
import React, { createContext, useContext, useEffect, useState } from "react";
import { settingApi } from "@/core/api/settingApi";

export interface ModuleContextValue {
  /** Set of names of active modules */
  activeModules: Set<string>;
  /** true while we are still waiting for the API */
  loading: boolean;
}

const ModuleContext = createContext<ModuleContextValue | undefined>(undefined);

/**
 * Provider that fetches active modules once and stores them in context.
 */
export const ModuleProvider: React.FC<{ children: React.ReactNode }> = ({
  children,
}) => {
  const [activeModules, setActiveModules] = useState<Set<string>>(new Set());
  const [loading, setLoading] = useState<boolean>(true);

  useEffect(() => {
    let isMounted = true;
    const fetchModules = async () => {
      try {
        const modules = await settingApi.GetActiveModules();
        if (!isMounted) return;
        const names = modules.map((m) => m.name);
        setActiveModules(new Set(names));
      } finally {
        if (isMounted) setLoading(false);
      }
    };
    fetchModules();
    return () => {
      isMounted = false;
    };
  }, []);

  return (
    <ModuleContext.Provider value={{ activeModules, loading }}>
      {children}
    </ModuleContext.Provider>
  );
};

/**
 * Hook that returns activeModules & loading flag.
 * Throws if used outside of ModuleProvider.
 */
export const useActiveModules = (): ModuleContextValue => {
  const ctx = useContext(ModuleContext);
  if (!ctx) {
    throw new Error(
      "useActiveModules must be used inside a ModuleProvider component"
    );
  }
  return ctx;
};
