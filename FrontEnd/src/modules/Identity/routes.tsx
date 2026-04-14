// modules/identity/routes.tsx
import type { RouteObject } from "react-router-dom";
import Login from "./pages/LoginPage";
import Register from "./pages/RegisterPage";
import UsersManagementPage from "./pages/UsersManagementPage";

export const identityPublicRoutes: RouteObject[] = [
  { path: "/login", element: <Login /> },
  { path: "/register", element: <Register /> },
];

export const identityPanelRoutes: RouteObject[] = [
  { path: "/users", element: <UsersManagementPage /> }, // مسیر نسبی یا مطلق؟ بهتر است نسبی باشد: "users"
];