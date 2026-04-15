// modules/identity/index.ts

export { useAuth } from "./context/AuthContext";          // از AuthContext
export { AuthProvider } from "./context/AuthContext";     // از AuthContext
export { getAccessToken, setGlobalAccessToken } from "./context/AuthContext"; // در صورت نیاز
export * from "./api/identityApi";
export { identityPublicRoutes, identityPanelRoutes } from "./routes"; 
export { default as ProtectedRoute } from "./components/ProtectedRoute";

// Login page
export { LoginPageWithCustomForm } from './Interface/ILoginPage';
export type { RenderFormProps, ILoginPageProps } from './Interface/ILoginPage';
export { default as LoginPage } from "./pages/LoginPage";
export * from "./hooks/Forms/useLoginForm";

// Register Page
export { useRegisterForm } from './hooks/Forms/useRegisterForm';
export { RegisterPageWithCustomForm } from './Interface/IRegisterPage';
export type { RenderRegisterFormProps, IRegisterPageProps } from './Interface/IRegisterPage';
export { default as RegisterPage } from './pages/RegisterPage';


// UserManagementPage
export { UserManagementForm } from './Interface/IUserManagementPage';
export type { 
  RenderFormProps as RenderUserManagementFormProps, 
  IUserManagementPageProps 
} from './Interface/IUserManagementPage';
export { default as UserManagementPage } from './pages/UsersManagementPage';
export { useUserManagement } from './hooks/Forms/useUserManagementForm';


// UserUpdatePage
export { UserUpdateForm } from './Interface/IUserUpdatePage';
export type { 
  RenderFormProps as RenderUseUpdateFormProps, 
  IUserUpdatePageProps 
} from './Interface/IUserUpdatePage';
export { default as UserUpdatePage } from './pages/UserUpdatePage';
export { useUserUpdateForm } from './hooks/Forms/useUserUpdateForm';


// UserCreatePage
export { UserCreateForm } from './Interface/IUserCreatePage';
export type { 
  RenderFormProps as RenderUseCreateFormProps, 
  IUserCreatePageProps 
} from './Interface/IUserCreatePage';
export { default as UserCreatePage } from './pages/UserCreatePage';
export { useUserCreateForm } from './hooks/Forms/useUserCreateForm';