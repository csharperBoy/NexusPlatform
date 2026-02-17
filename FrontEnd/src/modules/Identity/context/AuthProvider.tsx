// modules/auth/context/AuthProvider.tsx

import React, {
  createContext,
  useState,
  useEffect,
  ReactNode,
  useCallback,
} from "react";
import { authApi } from "../api/identityApi";
import type { AuthResponse } from "../models/AuthResponse";

interface AuthContextType {
  accessToken: string | null;
  user: { id: string; userName: string } | null;
  login: (data: AuthResponse) => void;
  logout: () => Promise<void>;
  isAuthenticated: boolean;
  setAccessToken: (token: string | null) => void;
}

export const AuthContext = createContext<AuthContextType | undefined>(
  undefined
);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [accessToken, setAccessToken] = useState<string | null>(null);
  const [user, setUser] = useState<{
    id: string;
    userName: string;
  } | null>(null);

  // -------------------------
  // Login (بعد از موفقیت فرم)
  // -------------------------
  const login = (data: AuthResponse) => {
    setAccessToken(data.accessToken);
    setUser({
      id: data.userId,
      userName: data.userName,
    });
  };

  // -------------------------
  // Logout
  // -------------------------
  const logout = useCallback(async () => {
    try {
      await authApi.logout(); // cookie حذف می‌شود
    } catch (err) {
      console.warn("Logout request failed", err);
    } finally {
      setAccessToken(null);
      setUser(null);
    }
  }, []);

  // -------------------------
  // Silent Refresh هنگام load اولیه
  // -------------------------
  useEffect(() => {
    const silentRefresh = async () => {
      try {
        const res = await authApi.refresh();
        setAccessToken(res.accessToken);
      } catch {
        setAccessToken(null);
        setUser(null);
      }
    };

    silentRefresh();
  }, []);

  return (
    <AuthContext.Provider
      value={{
        accessToken,
        user,
        login,
        logout,
        isAuthenticated: !!accessToken,
        setAccessToken,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};