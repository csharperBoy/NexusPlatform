// src/modules/Identity/hooks/Forms/useRegisterForm.ts
import { useState } from "react";
import { authApi } from "../../api/identityApi";
import { useAuth } from "../../context/AuthContext";
import type { RegisterRequest } from "../../models/RegisterRequest";

export const useRegisterForm = (onSuccess?: () => void) => {
  const { login } = useAuth();

  const [username, setUsername] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [displayName, setDisplayName] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (password !== confirmPassword) {
      setError("رمز عبور و تکرار آن مطابقت ندارند");
      return;
    }

    try {
      setLoading(true);
      setError(null);

      const registerData: RegisterRequest = {
        username,
        email,
        password,
        displayName: displayName || undefined,
      };

      const res = await authApi.register(registerData);
      login(res);
      onSuccess?.();
    } catch (err: any) {
      setError(err?.response?.data || "ثبت‌نام ناموفق بود");
    } finally {
      setLoading(false);
    }
  };

  return {
    username,
    email,
    password,
    confirmPassword,
    displayName,
    loading,
    error,
    setUsername,
    setEmail,
    setPassword,
    setConfirmPassword,
    setDisplayName,
    handleSubmit,
  };
};