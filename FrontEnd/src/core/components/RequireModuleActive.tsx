// src/core/components/RequireModuleActive.tsx
import React from "react";
import { useActiveModules } from "@/core/context/ModuleContext";

export const RequireModuleActive: React.FC<{
  moduleName: string;
  children: React.ReactNode;
}> = ({ moduleName, children }) => {
  const { activeModules } = useActiveModules();
  return activeModules.has(moduleName) ? <>{children}</> : null;
};
