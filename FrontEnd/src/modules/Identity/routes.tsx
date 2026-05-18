// modules/identity/routes.tsx
import type { RouteObject } from "react-router-dom";
import Login from "./pages/LoginPage";
import Register from "./pages/RegisterPage";
import UsersManagementPage from "./pages/User/UsersManagementPage";
import UserUpdatePage from "./pages/User/UserUpdatePage";
import UserCreatePage from "./pages/User/UserCreatePage";
import RoleCreatePage from "./pages/Role/RoleCreatePage";
import RolesManagementPage from "./pages/Role/RolesManagementPage";
import RoleUpdatePage from "./pages/Role/RoleUpdatePage";

export const identityPublicRoutes: RouteObject[] = [
  { path: "/login", element: <Login /> },
  { path: "/register", element: <Register /> },
];

export const identityPanelRoutes: RouteObject[] = [  
  { path: "identity/user/create", element: <UserCreatePage /> }, 
  { path: "identity/users", element: <UsersManagementPage /> }, 
  { path: "identity/user/edit/:id", element: <UserUpdatePage /> },    
    { path: "identity/role/create", element: <RoleCreatePage /> }, 
  { path: "identity/roles", element: <RolesManagementPage /> }, 
  { path: "identity/role/edit/:id", element: <RoleUpdatePage /> }, 
];