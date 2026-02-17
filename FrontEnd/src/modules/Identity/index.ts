// modules/identity/index.ts
export { default as LoginForm } from "./components/Forms/LoginForm";
export { default as LoginPage } from "./pages/LoginPage";
export * from "./hooks/Forms/useLoginForm";
export { useAuth } from "./context/AuthContext";          // از AuthContext
export { AuthProvider } from "./context/AuthContext";     // از AuthContext
export { getAccessToken, setGlobalAccessToken } from "./context/AuthContext"; // در صورت نیاز
export * from "./api/identityApi";
export { default as authRoutes } from "./routes";
export { default as ProtectedRoute } from "./components/ProtectedRoute";
export { LoginPageWithCustomForm } from './components/CustomPage/LoginPage';
export type { RenderFormProps, LoginPageWithCustomFormProps } from './components/CustomPage/LoginPage';