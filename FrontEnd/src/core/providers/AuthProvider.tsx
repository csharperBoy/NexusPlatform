// core/providers/AuthProvider.tsx
import { createContext, useContext, useState, type ReactNode } from "react";
import { storage } from "../utils/storage";

type AuthContextType = {
  token: string | null;
  login: (token: string) => void;
  logout: () => void;
};

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [token, setToken] = useState<string | null>(storage.getToken());

  const login = (newToken: string) => {
    storage.setToken(newToken);
    setToken(newToken);
  };

  const logout = () => {
    storage.clearToken();
    setToken(null);
  };

  return (
    <AuthContext.Provider value={{ token, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuthContext = () => {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuthContext must be inside AuthProvider");
  return ctx;
};
