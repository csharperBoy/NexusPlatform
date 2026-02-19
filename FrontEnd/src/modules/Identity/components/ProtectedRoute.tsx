//src/modules/Identity/components/ProtectedRoute.tsx
import { Navigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import LoadingIndicator from '@/core/components/LoadingIndicator'; 

interface ProtectedRouteProps {
  children: React.ReactElement;
  loadingComponent?: React.ReactNode;
}

const ProtectedRoute = ({ children, loadingComponent }: ProtectedRouteProps) => {
  const { isAuthenticated, isLoading } = useAuth();
  if (isLoading) {
    console.log(" isLoading:");
    return loadingComponent ? <>{loadingComponent}</> : <LoadingIndicator />;
  }
  
  if (!isAuthenticated) {
    
    console.log(" ! isAuthenticated");
    return <Navigate to="/login" replace />;
  }
console.log("  children");
  return children;
};

export default ProtectedRoute;