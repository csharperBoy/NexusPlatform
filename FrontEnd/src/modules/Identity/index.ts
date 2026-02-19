// modules/identity/index.ts

export { useAuth } from "./context/AuthContext";          // از AuthContext
export { AuthProvider } from "./context/AuthContext";     // از AuthContext
export { getAccessToken, setGlobalAccessToken } from "./context/AuthContext"; // در صورت نیاز
export * from "./api/identityApi";
export { default as authRoutes } from "./routes";
export { default as ProtectedRoute } from "./components/ProtectedRoute";

// Login page
export { LoginPageWithCustomForm } from './components/CustomPage/LoginPage';
export type { RenderFormProps, LoginPageWithCustomFormProps } from './components/CustomPage/LoginPage';
export { default as LoginPage } from "./pages/LoginPage";
export * from "./hooks/Forms/useLoginForm";

// Register Page
export { useRegisterForm } from './hooks/Forms/useRegisterForm';
export { RegisterPageWithCustomForm } from './components/CustomPage/RegisterPage';
export type { RenderRegisterFormProps, RegisterPageWithCustomFormProps } from './components/CustomPage/RegisterPage';
export { default as RegisterPage } from './pages/RegisterPage';