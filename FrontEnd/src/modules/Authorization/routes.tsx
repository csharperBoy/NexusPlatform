// src/modules/Authorization/routes.tsx
import type { RouteObject } from "react-router-dom";
import ResourceCreatePage from "./pages/Resource/ResourceCreatePage";

import ResourceUpdatePage from "./pages/Resource/ResourceUpdatePage";
import ResourceManagementPage from "./pages/Resource/ResourceManagementPage";
import PermissionCreatePage from "./pages/Permission/PermissionCreatePage";
import PermissionsManagementPage from "./pages/Permission/PermissionsManagementPage";
import PermissionUpdatePage from "./pages/Permission/PermissionUpdatePage";
import PermissionRuleCreatePage from "./pages/PermissionRule/PermissionRuleCreatePage";
import PermissionRuleUpdatePage from "./pages/PermissionRule/PermissionRuleUpdatePage";
import PermissionRulesManagementPage from "./pages/PermissionRule/PermissionRulesManagemetPage";

export const authorizationPublicRoutes: RouteObject[] = [
  
];

export const authorizationPanelRoutes: RouteObject[] = [
  { path: "authorization/resources/create", element: <ResourceCreatePage /> },   
  { path: "authorization/resources/create/:parentId", element: <ResourceCreatePage /> }, 
  { path: "authorization/resources/edit/:id", element: <ResourceUpdatePage /> }, 
  { path: "authorization/resources", element: <ResourceManagementPage /> },

  { path: "authorization/permissions/create", element: <PermissionCreatePage /> },   
  { path: "authorization/permissions/create/:resourceId", element: <PermissionCreatePage /> }, 
  { path: "authorization/permissions/edit/:id", element: <PermissionUpdatePage /> }, 
  { path: "authorization/permissions", element: <PermissionsManagementPage /> },

  { path: "authorization/permissionRules/create", element: <PermissionRuleCreatePage /> },   
  { path: "authorization/permissionRules/create/:permissionId", element: <PermissionRuleCreatePage /> }, 
  { path: "authorization/permissionRules/edit/:id", element: <PermissionRuleUpdatePage /> }, 
  { path: "authorization/permissionRules", element: <PermissionRulesManagementPage /> },
];