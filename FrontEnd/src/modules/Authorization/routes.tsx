// src/modules/Authorization/routes.tsx
import type { RouteObject } from "react-router-dom";
import ResourceCreatePage from "./pages/ResourceCreatePage";
import ResourceManagementPage from "./pages/ResourceManagementPage";

export const authorizationPublicRoutes: RouteObject[] = [
  
];

export const authorizationPanelRoutes: RouteObject[] = [
  { path: "resources/create", element: <ResourceCreatePage /> }, // مسیر نسبی
  { path: "resources", element: <ResourceManagementPage /> },
];