import { useRoutes } from "react-router-dom";
import { authRoutes, ProtectedRoute } from "../../../modules/auth";
import Dashboard from "./pages/Dashboard";

export default function App() {
  const routes = useRoutes([
    ...authRoutes,
    {
      path: "/dashboard",
      element: (
        <ProtectedRoute>
          <Dashboard />
        </ProtectedRoute>
      ),
    },
    { path: "*", element: <ProtectedRoute><Dashboard /></ProtectedRoute> },
  ]);

  return routes;
}
