// src/modules/Authorization/routes.tsx
import type { RouteObject } from "react-router-dom";
import PostManagementPage from "./pages/Post/PostManagementPage";

export const hrPublicRoutes: RouteObject[] = [
  
];

export const hrPanelRoutes: RouteObject[] = [
  { path: "hr/post", element: <PostManagementPage /> },   
];