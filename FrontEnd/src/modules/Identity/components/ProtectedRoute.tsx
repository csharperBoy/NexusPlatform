// src/modules/Identity/components/ProtectedRoute.tsx
import { Navigate, useLocation } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { useMenu } from "@/core/context/MenuContext";
import LoadingIndicator from "@/core/components/LoadingIndicator";

interface ProtectedRouteProps {
  children: React.ReactElement;
  loadingComponent?: React.ReactNode;
}

const ProtectedRoute = ({ children, loadingComponent }: ProtectedRouteProps) => {
  const { isAuthenticated, isLoading: isAuthLoading } = useAuth();
  const { allowedPaths, isMenuLoading } = useMenu();
  const location = useLocation();

  // ۱. بررسی بارگذاری احراز هویت یا منوها
  if (isAuthLoading || isMenuLoading) {
    return loadingComponent ? <>{loadingComponent}</> : <LoadingIndicator />;
  }

  // ۲. عدم لاگین -> هدایت به لاگین
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  // ۳. بررسی دسترسی به URL وارد شده
  const currentPath = location.pathname.toLowerCase().replace(/\/$/, "");
  
  // بررسی اینکه آیا مسیر فعلی یا ریشه آن در دسترسی‌ها وجود دارد یا خیر
  const hasAccess = Array.from(allowedPaths).some(
    (allowed) => currentPath === allowed || currentPath.startsWith(`${allowed}/`)
  );

  if (!hasAccess) {
    console.warn(`عدم دسترسی به مسیر: ${currentPath}`);
    return <Navigate to="/dashboard" replace />; // یا هدایت به صفحه /403
  }

  return children;
};

export default ProtectedRoute;