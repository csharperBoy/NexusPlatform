// src/modules/PhoneBook/routes.tsx
import type { RouteObject } from "react-router-dom";
import PhoneBookPage from "./pages/PhoneBook/PhoneBookPage";
import EmploymentContactManagementPage from "./pages/EmploymentContact/EmploymentContactManagementPage";
import PostContactManagementPage from "./pages/PostContact/PostContactManagementPage";
import { LocationContactManagementPage } from "./pages/LocationContact/LocationContactManagementPage";

export const ContactPublicRoutes: RouteObject[] = [
  
  { path: "/", element: <PhoneBookPage /> }, 
];

export const ContactPanelRoutes: RouteObject[] = [  
  
  { path: "contact/Employment", element: <EmploymentContactManagementPage /> },
  
  { path: "contact/Post", element: <PostContactManagementPage /> },
  
  { path: "contact/Location", element: <LocationContactManagementPage /> },
];