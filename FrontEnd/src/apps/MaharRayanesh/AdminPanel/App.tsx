// apps/MaharRayanesh/AdminPanel/App.tsx
import { useRoutes } from "react-router-dom";
import { authRoutes } from "../../../modules/auth";
import Dashboard from "./pages/Dashboard";

export default function App() {
  const routes = useRoutes([
    ...authRoutes,
    { path: "/dashboard", element: <Dashboard /> },
    { path: "*", element: <Dashboard /> },
  ]);

  return routes;
}

