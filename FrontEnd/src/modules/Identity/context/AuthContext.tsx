// src/modules/Identity/context/AuthContext.tsx
import React, {
  createContext,
  useContext,
  useEffect,
  useState,
  ReactNode,
  useRef ,
  useCallback,
} from "react";
import { authApi } from "../api/identityApi";

interface UserInfo {
  id: string;
  userName: string;
}

interface AuthContextType {
  accessToken: string | null;
  user: UserInfo | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (data: {
    accessToken: string;
    userId: string;
    userName: string;
  }) => void;
  logout: () => Promise<void>;
  setAccessToken: (token: string | null) => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

/* ============================================================
   GLOBAL ACCESS TOKEN HOLDER (for axios)
============================================================ */

let inMemoryAccessToken: string | null = null;

export function getAccessToken(): string | null {
  return inMemoryAccessToken;
}

export function setGlobalAccessToken(token: string | null) {
  inMemoryAccessToken = token;
}

/* ============================================================
   PROVIDER
============================================================ */

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [accessToken, setAccessTokenState] = useState<string | null>(null);
  const [user, setUser] = useState<UserInfo | null>(null);
  const [isLoading, setIsLoading] = useState(true); 
  
   const refreshAttempted = useRef(false); 
  const setAccessToken = (token: string | null) => {
    setAccessTokenState(token);
    setGlobalAccessToken(token);
  };

  /* ===========================
     LOGIN
  =========================== */

  const login = (data: {
    accessToken: string;
    userId: string;
    userName: string;
  }) => {
    console.log("AuthContext login called with:", data);
  
    setAccessToken(data.accessToken);
    setUser({
      id: data.userId,
      userName: data.userName,
    });
  };

  /* ===========================
     LOGOUT
  =========================== */

  const logout = useCallback(async () => {
    try {
      await authApi.logout();
    } catch {
      // ignore
    } finally {
      setAccessToken(null);
      setUser(null);
      window.location.href = "/login";
    }
  }, []);

  /* ===========================
     SILENT REFRESH ON LOAD
  =========================== */
  useEffect(() => {
    // جلوگیری از اجرای دو باره در StrictMode
    if (refreshAttempted.current) return;
    refreshAttempted.current = true;

    const silentRefresh = async () => {
      try {
        const res = await authApi.refresh();
        setAccessToken(res.accessToken);
        setUser({ id: res.userId, userName: res.userName });
      } catch {
        setAccessToken(null);
        setUser(null);
      } finally {
      setIsLoading(false); // پایان بارگذاری
    }
    };

    silentRefresh();
  }, []);

  return (
    <AuthContext.Provider
      value={{
        accessToken,
        user,
        isAuthenticated: !!accessToken,
        isLoading,
        login,
        logout,
        setAccessToken,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

/* ============================================================
   HOOK
============================================================ */

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used inside AuthProvider");
  }
  return context;
}
