import React from "react";
import Button from "@/core/components/Button";
import Input from "@/core/components/Input";
import Card from "@/core/components/Card";
import { useLoginForm } from "@/modules/auth/hooks/Forms/useLoginForm";

export type LoginFormProps = {
  onSuccess?: () => void;
  className?: string;
};

const LoginForm: React.FC<LoginFormProps> = ({ 
  onSuccess, 
  className 
}) => {
  const {
    identifier,
    password,
    loading,
    error,
    setIdentifier,
    setPassword,
    handleSubmit,
    loginType
  } = useLoginForm("email", onSuccess); // مشخص کردن نوع لاگین به عنوان ایمیل

  return (
    <div className="bg-yellow-400 dark:bg-gray-800 h-screen overflow-hidden flex items-center justify-center">
      <Card >
     
        <form onSubmit={handleSubmit}>
          <div >
            <Input 
              type="UserName" 
              id="identifier"
              value={identifier}
              onChange={(e) => setIdentifier(e.target.value)}
            placeholder="UserName" 
              disabled={loading}
              required
            />
            
          </div>
          
          <div >
            <Input 
              type="password" 
              id="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Password" 
              disabled={loading}
              required
            />
            
          </div>

          {error && (
            <div >
              {error}
            </div>
          )}

          <Button 
            type="submit"
            disabled={loading}
          >
            {loading ? "در حال ورود..." : "Login"}
          </Button>
        </form>
      </Card>
    </div>
  );
};

export default LoginForm;