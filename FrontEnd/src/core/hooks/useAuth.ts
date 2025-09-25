// core/hooks/useAuth.ts
import { useAuthContext } from "../providers/AuthProvider";

export const useAuth = () => {
  const { token, login, logout } = useAuthContext();
  return { token, login, logout, isAuthenticated: !!token };
};
