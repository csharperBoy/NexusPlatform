//src/apps/AkSteel Wellfare Platform/AdminPanel/App.tsx
import { useRoutes } from "react-router-dom";
import { authRoutes, ProtectedRoute } from "../../../modules/auth";
import Dashboard from "./pages/Dashboard";
import TailwindTest from "./pages/TailwindTest";
import LoginForm from "./pages/LoginPage";

export default function App() {
  const routes = useRoutes([
    
    {
      path: "/dashboard",
      element: (
        <ProtectedRoute>
          <Dashboard />
        </ProtectedRoute>
      ),
    },
    {
      path: "/login",
      element: (
          <LoginForm />
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
