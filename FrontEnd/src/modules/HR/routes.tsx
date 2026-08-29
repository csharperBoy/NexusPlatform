// src/modules/Authorization/routes.tsx
import type { RouteObject } from "react-router-dom";
import PostManagementPage from "./pages/Post/PostManagementPage";
import EmploymentManagementPage from "./pages/Employment/EmploymentManagementPage";
import LocationManagementPage from "./pages/Location/LocationManagementPage";
import { SyncPage } from "./pages/Sync/SyncPage";

export const hrPublicRoutes: RouteObject[] = [
  
];

export const hrPanelRoutes: RouteObject[] = [
  { path: "hr/post", element: <PostManagementPage /> },   
  
  { path: "hr/employment", element: <EmploymentManagementPage /> },  
  
  { path: "hr/location", element: <LocationManagementPage/> }, 
  
  { path: "hr/sync", element: <SyncPage/> }, 
];