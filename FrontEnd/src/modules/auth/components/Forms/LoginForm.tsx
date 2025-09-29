// src/modules/auth/components/forms/LoginForm.tsx
import React, { useState, useEffect } from "react";
import Button from "@/core/components/Button";
import Input from "@/core/components/Input";
import Card from "@/core/components/Card";
import { useLogin } from "../../hooks/useLogin";

export type LoginFormProps = {
  onSuccess?: () => void;
  className?: string;
};

const LoginForm: React.FC<LoginFormProps> = ({ 
  onSuccess, 
  className 
}) => {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [hasSubmitted, setHasSubmitted] = useState(false);
  const { login, loading, error } = useLogin();

  useEffect(() => {
    if (hasSubmitted && !loading && !error) {
      onSuccess?.();
      setHasSubmitted(false);
    }
  }, [loading, error, hasSubmitted, onSuccess]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setHasSubmitted(true);
    await login({ email, password });
  };

  return (
    <div className="min-h-screen flex items-center justify-center p-4">
      <Card className="w-full max-w-sm" padding="lg">
        <form onSubmit={handleSubmit} className="space-y-4">
          <Input
            type="email"
            placeholder="ایمیل"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            disabled={loading}
            className="w-full"
          />

          <Input
            type="password"
            placeholder="رمز عبور"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            disabled={loading}
            className="w-full"
          />

          {error && (
            <div className="text-red-600 text-sm text-center">
              {error}
            </div>
          )}

          <Button
            type="submit"
            variant="primary"
            size="lg"
            disabled={loading}
          >
            {loading ? "در حال ورود..." : "ورود"}
          </Button>
        </form>
      </Card>
    </div>
  );
};

export default LoginForm;