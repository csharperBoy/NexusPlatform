// src/apps/Test/App.tsx
import { useRoutes, Navigate } from "react-router-dom";
import { TestPage } from "../../core/components/Generic/templates/pages/TestPage";

export default function App() {
  console.info('start:');
  console.warn('start=');

  const routes = useRoutes([
    /* صفحه تست اصلی */
    { path: "/", element: <TestPage /> },

    /* مسیر پیش‌فرض */
    { path: "*", element: <Navigate to="/" replace /> },
  ]);

  return routes;
}