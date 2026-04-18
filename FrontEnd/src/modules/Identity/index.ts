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
export { UserManagementForm } from './Interface/User/IUserManagementPage';
export type { 
  RenderFormProps as RenderUserManagementFormProps, 
  IUserManagementPageProps 
} from './Interface/User/IUserManagementPage';
export { default as UserManagementPage } from './pages/User/UsersManagementPage';
export { useUserManagement } from './hooks/Forms/User/useUserManagementForm';


// UserCreateUpdatePage
export { IUserCreateUpdatePage } from './Interface/User/IUserCreateUpdatePage';
export type { 
  RenderFormProps as RenderUseUpdateFormProps, 
  IUserCreateUpdatePageProps 
} from './Interface/User/IUserCreateUpdatePage';
export { default as UserUpdatePage } from './pages/User/UserUpdatePage';
export { useUserCreateUpdateForm } from './hooks/Forms/User/useUserCreateUpdateForm';



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