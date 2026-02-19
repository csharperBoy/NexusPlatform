// modules/identity/routes.tsx
import type { RouteObject } from "react-router-dom";
import Login from "./pages/LoginPage";
import Register from "./pages/RegisterPage";
import UsersPage from "./pages/UsersPage";

export const identityPublicRoutes: RouteObject[] = [
  { path: "/login", element: <Login /> },
  { path: "/register", element: <Register /> },
];

export const identityPanelRoutes: RouteObject[] = [
  { path: "/users", element: <UsersPage /> }, // مسیر نسبی یا مطلق؟ بهتر است نسبی باشد: "users"
];