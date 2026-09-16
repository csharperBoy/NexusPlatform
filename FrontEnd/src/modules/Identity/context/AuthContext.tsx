import React, {
  createContext,
  useContext,
  useEffect,
  useState,
  ReactNode,
  useRef,
  useCallback,
} from "react";
import { identityApi } from "../api/identityApi";
import { storageAdapter } from "@/core/storage/storageAdapter"; // اضافه شد

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
     EVENT LISTENER FOR 401 UNAUTHORIZED
  =========================== */
  // اضافه شد: شنود رویدادی که از axiosClient در زمان اکسپایر شدن توکن می‌آید
  useEffect(() => {
    const handleUnauthorized = async () => {
      setAccessToken(null);
      setUser(null);
      await storageAdapter.removeAccessToken();
      await storageAdapter.removeItem('user_info');
    };

    window.addEventListener('auth:unauthorized', handleUnauthorized);
    return () => window.removeEventListener('auth:unauthorized', handleUnauthorized);
  }, []);

  /* ===========================
     LOGIN
  =========================== */

  const login = async (data: {
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

    // اضافه شد: ذخیره در حافظه امن برای استفاده‌های آفلاین/موبایل
    await storageAdapter.setAccessToken(data.accessToken);
    await storageAdapter.setItem('user_info', JSON.stringify({
      id: data.userId,
      userName: data.userName
    }));
  };

  /* ===========================
     LOGOUT
  =========================== */

  const logout = useCallback(async () => {
    try {
      await identityApi.logout();
    } catch {
      // ignore
    } finally {
      setAccessToken(null);
      setUser(null);
      
      // پاکسازی حافظه امن
      await storageAdapter.removeAccessToken();
      await storageAdapter.removeItem('user_info');
      
      // تغییر مهم: window.location.href حذف شد. 
      // وقتی stateها null شوند، ProtectedRoute کاربر را به صفحه لاگین می‌فرستد.
    }
  }, []);

  /* ===========================
     SILENT REFRESH ON LOAD (Update for Offline/Mobile)
  =========================== */
  useEffect(() => {
    if (refreshAttempted.current) return;
    refreshAttempted.current = true;

    const initAuth = async () => {
      try {
        // تلاش برای رفرش توکن از سرور (در حالت وب/آنلاین)
        const res = await identityApi.refresh();
        setAccessToken(res.accessToken);
        setUser({ id: res.userId, userName: res.userName });
        
        // همگام‌سازی توکن جدید با استوریج
        await storageAdapter.setAccessToken(res.accessToken);
        await storageAdapter.setItem('user_info', JSON.stringify({ id: res.userId, userName: res.userName }));
        
      } catch {
        // اضافه شد: حالت آفلاین یا موبایل!
        // اگر سرور در دسترس نبود یا کوکی کار نکرد، توکن ذخیره شده در دیتابیس لوکال را می‌خوانیم
        const savedToken = await storageAdapter.getAccessToken();
        const savedUserInfoString = await storageAdapter.getItem('user_info');

        if (savedToken && savedUserInfoString) {
          setAccessToken(savedToken);
          setUser(JSON.parse(savedUserInfoString));
        } else {
          setAccessToken(null);
          setUser(null);
          await storageAdapter.removeAccessToken();
          await storageAdapter.removeItem('user_info');
        }
      } finally {
        setIsLoading(false);
      }
    };

    initAuth();
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