// src/modules/Identity/hooks/Forms/useLoginForm.ts
import { useState } from "react";
import { authApi } from "../../api/identityApi";
import { useAuth } from "../useAuth";

export const useLoginForm = (onSuccess?: () => void) => {
  const { login } = useAuth();

  const [identifier, setIdentifier] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setLoading(true);
      const res = await authApi.login({
        userIdentifier: identifier,
        password,
      });

      login(res);
      onSuccess?.();
    } catch (err: any) {
      setError(err?.response?.data || "ورود ناموفق بود");
    } finally {
      setLoading(false);
    }
  };

  return {
    identifier,
    password,
    loading,
    error,
    setIdentifier,
    setPassword,
    handleSubmit,
  };
};