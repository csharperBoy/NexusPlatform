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
  RenderFormProps as RenderUserCreateFormProps, 
  IUserCreatePageProps 
} from './Interface/IUserCreatePage';
export { default as UserCreatePage } from './pages/UserCreatePage';
export { useUserCreateForm } from './hooks/Forms/useUserCreateForm';


// RoleManagementPage
export { RoleManagementForm } from './Interface/Role/IRoleManagementPage';
export type { 
  RenderFormProps as RenderRoleManagementFormProps, 
  IRoleManagementPageProps 
} from './Interface/Role/IRoleManagementPage';
export { default as RoleManagementPage } from './pages/Role/RolesManagementPage';
export { useRoleManagement } from './hooks/Forms/Role/useRoleManagementForm';


// RoleUpdatePage
export { RoleUpdateForm } from './Interface/Role/IRoleUpdatePage';
export type { 
  RenderFormProps as RenderRoleUpdateFormProps, 
  IRoleUpdatePageProps 
} from './Interface/Role/IRoleUpdatePage';
export { default as RoleUpdatePage } from './pages/Role/RoleUpdatePage';
export { useRoleUpdateForm } from './hooks/Forms/Role/useRoleUpdateForm';


// RoleCreatePage
export { RoleCreateForm } from './Interface/Role/IRoleCreatePage';
export type { 
  RenderFormProps as RenderUseCreateFormProps, 
  IRoleCreatePageProps 
} from './Interface/Role/IRoleCreatePage';
export { default as RoleCreatePage } from './pages/Role/RoleCreatePage';
export { useRoleCreateForm } from './hooks/Forms/Role/useRoleCreateForm';