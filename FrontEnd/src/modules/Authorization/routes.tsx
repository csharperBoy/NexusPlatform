// src/modules/Authorization/routes.tsx
import type { RouteObject } from "react-router-dom";
import ResourceCreatePage from "./pages/Resource/ResourceCreatePage";

import ResourceUpdatePage from "./pages/Resource/ResourceUpdatePage";
import ResourceManagementPage from "./pages/Resource/ResourceManagementPage";

export const authorizationPublicRoutes: RouteObject[] = [
  
];

export const authorizationPanelRoutes: RouteObject[] = [
  { path: "resources/create", element: <ResourceCreatePage /> },   
  { path: "resources/create/:parentId", element: <ResourceCreatePage /> }, 
  { path: "resources/edit/:id", element: <ResourceUpdatePage /> }, 
  { path: "resources", element: <ResourceManagementPage /> },
];