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
  { path: "user/create", element: <UserCreatePage /> }, 
  { path: "/users", element: <UsersManagementPage /> }, 
  { path: "user/edit/:id", element: <UserUpdatePage /> },  
  
    { path: "role/create", element: <RoleCreatePage /> }, 
  { path: "/roles", element: <RolesManagementPage /> }, 
  { path: "role/edit/:id", element: <RoleUpdatePage /> }, 
];