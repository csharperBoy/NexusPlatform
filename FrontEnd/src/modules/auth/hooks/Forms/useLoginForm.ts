// src/modules/auth/hooks/Forms/useLoginForm.ts
import { useState, useEffect, useCallback } from "react";
import { authApi } from "../../api/authApi";
import { useAuth } from "../useAuth";

type LoginType = "email" | "username";

export const useLoginForm = (loginType: LoginType = "email", onSuccess?: () => void) => {
  const { login: authLogin } = useAuth();
  
  const [identifier, setIdentifier] = useState(""); // ایمیل یا نام کاربری
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [hasSubmitted, setHasSubmitted] = useState(false);

  useEffect(() => {
    if (hasSubmitted && !loading && !error) {
      onSuccess?.();
      setHasSubmitted(false);
      resetForm();
    }
  }, [loading, error, hasSubmitted, onSuccess]);

  const login = useCallback(async (identifier: string, password: string) => {
    try {
      setLoading(true);
      setError(null);
      
      let response;
      if (loginType === "email") {
        response = await authApi.loginWithEmail({ username: identifier, password });
      } else {
        response = await authApi.loginWithUsername({ username: identifier, password });
      }
      
      authLogin(response);
      return response;
    } catch (err: any) {
     const errorMessage = err?.response?.data 
            || err?.message 
            || "ورود ناموفق بود";
      setError(errorMessage);
      throw err;
    } finally {
      setLoading(false);
    }
  }, [authLogin, loginType]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!validateForm()) {
      return;
    }

    setHasSubmitted(true);
    
    try {
      await login(identifier, password);
    } catch (err) {
      console.error("Login error:", err);
    }
  };

  const validateForm = (): boolean => {
    if (!identifier || !password) {
      setError("لطفا تمام فیلدها را پر کنید");
      return false;
    }

    if (loginType === "email" && !identifier.includes("@")) {
      setError("لطفا یک ایمیل معتبر وارد کنید");
      return false;
    }

    if (loginType === "username" && identifier.length < 3) {
      setError("نام کاربری باید حداقل ۳ کاراکتر باشد");
      return false;
    }

    if (password.length < 6) {
      setError("رمز عبور باید حداقل ۶ کاراکتر باشد");
      return false;
    }

    return true;
  };

  const resetForm = useCallback(() => {
    setIdentifier("");
    setPassword("");
    setHasSubmitted(false);
    setError(null);
  }, []);

  const handleIdentifierChange = useCallback((value: string) => {
    setIdentifier(value);
    if (error) setError(null);
  }, [error]);

  const handlePasswordChange = useCallback((value: string) => {
    setPassword(value);
    if (error) setError(null);
  }, [error]);

  return {
    // State
    identifier,
    password,
    loading,
    error,
    
    // Setters
    setIdentifier: handleIdentifierChange,
    setPassword: handlePasswordChange,
    
    // Actions
    handleSubmit,
    resetForm,
    login: () => login(identifier, password),
    
    // Derived state
    isValid: loginType === "email" 
      ? identifier.includes("@") && password.length >= 6
      : identifier.length >= 3 && password.length >= 6,
    isDirty: identifier !== "" || password !== "",
    loginType
  };
};