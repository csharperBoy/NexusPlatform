//modules/auth/context/AuthProvider.tsx
import React, { createContext, useState, useEffect, ReactNode } from "react";
import { AuthResponse } from "../models/AuthResponse";
import { storage } from "@/core/utils/storage";

interface AuthContextType {
  user: { email: string } | null;
  token: string | null;
  login: (data: AuthResponse) => void;
  logout: () => void;
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [user, setUser] = useState<{ email: string } | null>(null);
  const [token, setToken] = useState<string | null>(null);

  useEffect(() => {
    const savedToken = storage.getToken();
    if (savedToken) {
      setToken(savedToken);
      // فعلاً فقط ایمیل رو از localStorage نمی‌گیریم چون ذخیره نمی‌شه
    }
  }, []);

  const login = (data: AuthResponse) => {
    setUser({ email: data.email });
    setToken(data.token);
    storage.setToken(data.token);
  };

  const logout = () => {
    setUser(null);
    setToken(null);
    storage.clearToken();
  };

  return (
    <AuthContext.Provider value={{ user, token, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
};
