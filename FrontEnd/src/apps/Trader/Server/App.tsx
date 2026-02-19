// src/apps/Trader/Server/App.tsx
import { useRoutes } from "react-router-dom";
import { authRoutes, ProtectedRoute } from "../../../modules/Identity";
import Dashboard from "./pages/Dashboard";
import TailwindTest from "./pages/TailwindTest";
import LoginPage from "./pages/LoginPage"; // صفحه اختصاصی لاگین

export default function App() {
  const routes = useRoutes([
    //  مسیر های صفحات سفارشی شده مربوط به ماژول Identity - اولویت بالاتر را دارد نسبت به صفحات پیش فرض چون زودتر ثبت شده است
     {
      path: "/login",
      element: <LoginPage />,
    },
    ...authRoutes, // صفحات پیش فرض ماژول Identity - در صورتی که در بالا ثبت نشده باشد
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