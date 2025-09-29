//src/apps/MaharRayanesh/AdminPanel/App.tsx
import { useRoutes } from "react-router-dom";
import { authRoutes, ProtectedRoute } from "../../../modules/auth";
import Dashboard from "./pages/Dashboard";
import TailwindTest from "./pages/TailwindTest";

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
    {
  path: "/twtest",
  element: <TailwindTest />,
},
    { path: "*", element: <ProtectedRoute><Dashboard /></ProtectedRoute> },
  ]);

  return routes;
}
