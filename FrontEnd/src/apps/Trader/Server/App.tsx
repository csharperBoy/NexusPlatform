// src/apps/Trader/Server/App.tsx
import { useRoutes } from "react-router-dom";
import { authRoutes, ProtectedRoute } from "../../../modules/Identity";
import Dashboard from "./pages/Dashboard";
import TailwindTest from "./pages/TailwindTest";
// import LoginPage from "./pages/LoginPage"; // صفحه اختصاصی جدید

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
   /* {
      path: "/login",
      element: <LoginPage />,
    },*/
    {
      path: "/twtest",
      element: <TailwindTest />,
    },
    { path: "*", element: <ProtectedRoute><Dashboard /></ProtectedRoute> },
  ]);

  return routes;
}