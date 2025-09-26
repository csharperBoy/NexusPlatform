// modules/auth/hooks/useLogin.ts
import { useState } from "react";
import { authApi } from "../api/authApi";
import { useAuth } from "../../../core/hooks/useAuth";
import type { LoginRequest } from "../models/LoginRequest";

export const useLogin = () => {
  const { login } = useAuth();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const doLogin = async (data: LoginRequest) => {
    try {
      setLoading(true);
      setError(null);
      const response = await authApi.login(data);
      login(response.token);
    } catch (err: any) {
      setError(err.response?.data || "خطایی رخ داده است");
    } finally {
      setLoading(false);
    }
  };

  return { doLogin, loading, error };
};
