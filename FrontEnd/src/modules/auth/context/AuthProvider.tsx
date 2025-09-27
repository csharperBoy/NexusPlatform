//modules/auth/context/AuthProvider.tsx
import { createContext, useContext, useState, ReactNode, useCallback } from "react";


import { storage } from "@/core/utils/storage";
import { authApi } from "../api/authApi";
import type { AuthResponse } from "../models/AuthResponse";
import type { LoginRequest } from "../models/LoginRequest";

type AuthContextType = {
  token: string | null;
  user: { email?: string } | null;
  login: (payload: LoginRequest) => Promise<AuthResponse>;
  logout: () => void;
  isAuthenticated: boolean;
};

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [token, setToken] = useState<string | null>(storage.getToken());
  const [user, setUser] = useState<{ email?: string } | null>(token ? { email: undefined } : null);

  const login = useCallback(async (payload: LoginRequest) => {
    // use authApi (module-local) to perform login
    const res = await authApi.login(payload); // { token, expires, email }
    storage.setToken(res.token);
    setToken(res.token);
    setUser({ email: res.email });
    return res;
  }, []);

  const logout = useCallback(() => {
    storage.clearToken();
    setToken(null);
    setUser(null);
  }, []);

  return (
    <AuthContext.Provider value={{ token, user, login, logout, isAuthenticated: !!token }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used inside AuthProvider");
  return ctx;
};
