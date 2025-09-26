// modules/auth/components/LoginForm.tsx
import React, { useState } from "react";
import Button from "../../../../core/components/Button";
import { useLogin } from "../../hooks/useLogin";

export type LoginFormProps = {
  onSuccess?: () => void; // اپ می‌تواند redirect انجام دهد
  className?: string;
};

const LoginForm: React.FC<LoginFormProps> = ({ onSuccess, className }) => {
  const { doLogin, loading, error } = useLogin();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    await doLogin({ email, password });
    onSuccess?.();
  };

  return (
    <div className={className}>
      <form onSubmit={handleSubmit} className="flex flex-col gap-4">
        <input type="email" placeholder="ایمیل" value={email}
          onChange={(e)=>setEmail(e.target.value)} className="border p-2 rounded"/>
        <input type="password" placeholder="رمز عبور" value={password}
          onChange={(e)=>setPassword(e.target.value)} className="border p-2 rounded"/>
        <Button type="submit" disabled={loading}>{loading ? "در حال ورود..." : "ورود"}</Button>
      </form>
      {error && <p className="text-red-500 mt-2">{error}</p>}
    </div>
  );
};

export default LoginForm;
