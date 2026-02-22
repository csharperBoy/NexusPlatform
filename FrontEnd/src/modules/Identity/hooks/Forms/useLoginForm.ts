// src/modules/Identity/hooks/Forms/useLoginForm.ts
import { useState } from "react";
import { identityApi } from "../../api/identityApi";
import { useAuth } from "../../context/AuthContext";

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
       console.log("Sending login request...");
    
      const res = await identityApi.login({
        userIdentifier: identifier,
        password,
      });
console.log("Login response:", res); // اینجا باید accessToken را ببینیم
    
      login(res);
      console.log("after login call"); // ببینیم تا اینجا می‌رسد؟
    
      onSuccess?.();
    } catch (err: any) {
      console.error("Login error:", err); // اینجا خطا را نشان می‌دهد
    
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