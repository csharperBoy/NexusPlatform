// modules/auth/routes.tsx
import React from "react";
import { RouteObject } from "react-router-dom";
import Login from "./pages/LoginPage";
import Register from "./pages/Register";

const authRoutes: RouteObject[] = [
  { path: "/login", element: <Login /> },
  { path: "/register", element: <Register /> },
];

export default authRoutes;
