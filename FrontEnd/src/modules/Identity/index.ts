// modules/identity/index.ts

export { useAuth } from "./context/AuthContext";          // از AuthContext
export { AuthProvider } from "./context/AuthContext";     // از AuthContext
export { getAccessToken, setGlobalAccessToken } from "./context/AuthContext"; // در صورت نیاز
export * from "./api/identityApi";
export { identityPublicRoutes, identityPanelRoutes } from "./routes"; 
export { default as ProtectedRoute } from "./components/ProtectedRoute";

// Login page
export { LoginPageWithCustomForm } from './Interface/ILoginPage';
export type { RenderFormProps, LoginPageWithCustomFormProps } from './Interface/ILoginPage';
export { default as LoginPage } from "./pages/LoginPage";
export * from "./hooks/Forms/useLoginForm";

// Register Page
export { useRegisterForm } from './hooks/Forms/useRegisterForm';
export { RegisterPageWithCustomForm } from './Interface/IRegisterPage';
export type { RenderRegisterFormProps, RegisterPageWithCustomFormProps } from './Interface/IRegisterPage';
export { default as RegisterPage } from './pages/RegisterPage';