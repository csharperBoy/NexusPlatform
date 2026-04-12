// src/modules/Authorization/routes.tsx
import type { RouteObject } from "react-router-dom";
import ResourceCreatePage from "./pages/ResourceCreatePage";

import ResourceUpdatePage from "./pages/ResourceUpdatePage";
import ResourceManagementPage from "./pages/ResourceManagementPage";

export const authorizationPublicRoutes: RouteObject[] = [
  
];

export const authorizationPanelRoutes: RouteObject[] = [
  { path: "resources/create", element: <ResourceCreatePage /> }, 
  { path: "resources/edit/:id", element: <ResourceUpdatePage /> }, 
  { path: "resources", element: <ResourceManagementPage /> },
];