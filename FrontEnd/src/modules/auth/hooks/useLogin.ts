// modules/auth/hooks/useLogin.ts
import { useState } from "react";
import { authApi } from "../api/authApi";
import { LoginRequest } from "../models/LoginRequest";
import { AuthResponse } from "../models/AuthResponse";
import { useAuth } from "./useAuth";

export const useLogin = () => {
  const { login } = useAuth();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleLogin = async (credentials: LoginRequest) => {
    try {
      setLoading(true);
      setError(null);
      const response: AuthResponse = await authApi.login(credentials);
      login(response);
    } catch (err: any) {
      setError(err?.message || "Login failed");
    } finally {
      setLoading(false);
    }
  };

  return {
    login: handleLogin,
    loading,
    error,
  };
};
