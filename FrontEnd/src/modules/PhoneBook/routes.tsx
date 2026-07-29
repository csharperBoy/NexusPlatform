// src/modules/PhoneBook/routes.tsx
import type { RouteObject } from "react-router-dom";
import PhoneBookPage from "./pages/PhoneBook/PhoneBookPage";

export const phonebookPublicRoutes: RouteObject[] = [
  
];

export const phonebookPanelRoutes: RouteObject[] = [
  { path: "phonebook", element: <PhoneBookPage /> },   
];