// modules/DashboardCore/contexts/MenuContext.tsx
import React, { createContext, useState, useCallback, ReactNode, useContext } from 'react';
import { MenuItem } from '../types';

interface MenuContextType {
  registerMenuItem: (item: MenuItem) => void;
  getMenuItems: () => MenuItem[];
}

const MenuContext = createContext<MenuContextType | undefined>(undefined);

export const MenuProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [menuItems, setMenuItems] = useState<MenuItem[]>([]);

  const registerMenuItem = useCallback((item: MenuItem) => {
    setMenuItems(prev => {
      if (prev.some(i => i.id === item.id)) {
        console.warn(`Menu item with id "${item.id}" already registered.`);
        return prev;
      }
      return [...prev, item];
    });
  }, []);

  const getMenuItems = useCallback(() => {
    return [...menuItems].sort((a, b) => (a.order ?? 0) - (b.order ?? 0));
  }, [menuItems]);

  return (
    <MenuContext.Provider value={{ registerMenuItem, getMenuItems }}>
      {children}
    </MenuContext.Provider>
  );
};

export const useMenu = () => {
  const context = useContext(MenuContext);
  if (!context) throw new Error('useMenu must be used within MenuProvider');
  return context;
};