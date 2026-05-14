// src/hooks/useMenu.ts
import { useEffect, useState } from 'react';
import { MenuDto } from '../models/Menu';
import { menuApi } from '../api/menuApi';


export const useMenu = () => {
  const [menus, setMenus] = useState<MenuDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetch = async () => {
      const  data = await menuApi.GetMenus();
      setMenus(data);
      setLoading(false);
    };
    fetch();
  }, []);

  return { menus, loading };
};
