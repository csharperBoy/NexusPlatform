// modules/auth/hooks/useLogin.ts
import { useState } from "react";
import { useAuth } from "../context/AuthProvider";
import type { LoginRequest } from "../models/LoginRequest";
import type { AuthResponse } from "../models/AuthResponse";

export const useLogin = () => {
  const { login } = useAuth();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const doLogin = async (payload: LoginRequest): Promise<AuthResponse | undefined> => {
    try {
      setLoading(true);
      setError(null);
      const res = await login(payload);
      return res;
    } catch (err: any) {
      setError(err?.response?.data || err?.message || "خطایی رخ داد");
      return undefined;
    } finally {
      setLoading(false);
    }
  };

  return { doLogin, loading, error };
};
