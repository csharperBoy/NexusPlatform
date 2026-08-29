// src/core/context/MenuContext.tsx
import React, { createContext, useContext, useEffect, useState, ReactNode } from "react";
import { menuApi } from "../api/menuApi";
import { useAuth } from "@/modules/Identity/context/AuthContext";
import { MenuDto } from "../models/Menu";

interface MenuContextType {
  menus: MenuDto[];
  allowedPaths: Set<string>;
  isMenuLoading: boolean;
}

const MenuContext = createContext<MenuContextType | undefined>(undefined);

// تابع کمکی برای استخراج تمام مسیرها از ساختار درختی منو
const extractPaths = (items: MenuDto[]): Set<string> => {
  const paths = new Set<string>();
  
  const traverse = (nodes: MenuDto[]) => {
    nodes.forEach((node) => {
      if (node.path) {
        // استانداردسازی مسیر (بدون / آخر)
        paths.add(node.path.toLowerCase().replace(/\/$/, ""));
      }
      if (node.children && node.children.length > 0) {
        traverse(node.children);
      }
    });
  };

  traverse(items);
  
  // مسیرهای عمومی و همیشه مجاز در داخل پنل
  paths.add("/dashboard");
  paths.add("/403");
  
  return paths;
};

export const MenuProvider = ({ children }: { children: ReactNode }) => {
  const { isAuthenticated } = useAuth();
  const [menus, setMenus] = useState<MenuDto[]>([]);
  const [allowedPaths, setAllowedPaths] = useState<Set<string>>(new Set());
  const [isMenuLoading, setIsMenuLoading] = useState<boolean>(true);

  useEffect(() => {
    if (!isAuthenticated) {
      setMenus([]);
      setAllowedPaths(new Set());
      setIsMenuLoading(false);
      return;
    }

    const fetchMenus = async () => {
      setIsMenuLoading(true);
      try {
        const data = await menuApi.GetMenus();
        setMenus(data);
        setAllowedPaths(extractPaths(data));
      } catch (error) {
        console.error("خطا در دریافت منوها:", error);
      } finally {
        setIsMenuLoading(false);
      }
    };

    fetchMenus();
  }, [isAuthenticated]);

  return (
    <MenuContext.Provider value={{ menus, allowedPaths, isMenuLoading }}>
      {children}
    </MenuContext.Provider>
  );
};

export const useMenu = () => {
  const context = useContext(MenuContext);
  if (!context) {
    throw new Error("useMenu must be used inside MenuProvider");
  }
  return context;
};