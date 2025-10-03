// src/contexts/AuthContext.tsx
import React, { createContext, useState, useEffect, useContext } from 'react';
import { login as apiLogin, logout as apiLogout, validateToken as apiValidate } from '../api/AuthCollection';
import type { LoginRequest } from '../models/auth';

interface AuthContextType {
  userId: string | null;
  roles: string[];
  isAuthenticated: boolean;
  loading: boolean;
  login: (p: LoginRequest) => Promise<void>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<React.PropsWithChildren> = ({ children }) => {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [userId, setUserId]                 = useState<string | null>(null);
  const [roles, setRoles]                   = useState<string[]>([]);
  const [loading, setLoading]               = useState(true);  // ← اضافه شد

  useEffect(() => {
    const token = localStorage.getItem('authToken');
    if (token) {
      apiValidate()
        .then(res => {
          setIsAuthenticated(true);
          setUserId(res.userId);
          setRoles(res.roles);
        })
        .catch(() => {
          localStorage.removeItem('authToken');
        })
        .finally(() => {
          setLoading(false);  // ← اعتبارسنجی انجام شد
        });
    } else {
      setLoading(false);  // ← اصلاً توکنی نیست، تمام
    }
  }, []);

  const login = async (payload: LoginRequest) => {
    const data = await apiLogin(payload);
    localStorage.setItem('authToken', data.accessToken);

    const res = await apiValidate();
    setIsAuthenticated(true);
    setUserId(res.userId);
    setRoles(res.roles);
  };

  const logout = async () => {
    await apiLogout();
    localStorage.removeItem('authToken');
    setIsAuthenticated(false);
    setUserId(null);
    setRoles([]);
  };

  return (
    <AuthContext.Provider value={{ isAuthenticated, userId, roles, loading, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
};
