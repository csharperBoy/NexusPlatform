// modules/auth/components/LogoutButton.tsx
import React from "react";
import { useAuth } from "../../hooks/useAuth";
import { useNavigate } from "react-router-dom";

interface LogoutButtonProps {
  redirectTo?: string; // بعد از خروج کجا بره
  className?: string;
}

export const LogoutButton: React.FC<LogoutButtonProps> = ({
  redirectTo = "/login",
  className = "",
}) => {
  const { logout } = useAuth();
  const nav = useNavigate();

  const handleLogout = () => {
    logout();
    nav(redirectTo);
  };

  return (
    <button onClick={handleLogout} className={className}>
      خروج
    </button>
  );
};
