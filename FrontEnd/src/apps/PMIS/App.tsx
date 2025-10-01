// src/App.tsx
import React from 'react';
import {
  createBrowserRouter,
  RouterProvider,
  Outlet,
  ScrollRestoration,
  Navigate,
} from 'react-router-dom';

import { AuthProvider } from './contexts/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';
import IndicatorValueInput from './pages/IndicatorValueInput';
import RegisterIndicatorForm from './pages/RegisterIndicatorForm';
import IndicatorForm from './pages/IndicatorForm';
import IndicatorAssignToCategory from './pages/IndicatorAssignToCategory';
import Login from './pages/Login';
//import Home from './pages/Home';

import Navbar from './components/Navbar';
import Menu from './components/menu/Menu';
import Footer from './components/Footer';
import ToasterProvider from './components/ToasterProvider';
import ErrorPage from './pages/Error';
import IndicatorAssignToClaim from './pages/IndicatorAssignToClaim';
import IndicatorWizardStepper from './pages/IndicatorWizardStepper';
import ChangePassword from './pages/ChangePassword';

function App() {
  const Layout: React.FC = () => (
    <div
      id="rootContainer"
      className="w-full p-0 m-0 overflow-visible min-h-screen flex flex-col justify-between"
    >
      <ToasterProvider />
      <ScrollRestoration />

      {/* ناوبار ثابت بالا */}
      <Navbar />

      {/* سایدبار و محتوای صفحات */}
      <div className="w-full flex gap-0 pt-20 xl:pt-[96px] 2xl:pt-[112px] mb-auto">
        <aside className="hidden xl:block xl:w-[250px] 2xl:w-[280px] 3xl:w-[350px] border-r-2 border-base-300 dark:border-slate-700 px-3 xl:px-4 xl:py-1">
          <Menu />
        </aside>
        <main className="w-full px-4 xl:px-4 2xl:px-5 xl:py-2 overflow-x-auto">
  <Outlet />
</main>
      </div>

      {/* فوتر ثابت پایین */}
      <Footer />
    </div>
  );

  const router = createBrowserRouter([
    // صفحه لاگین (عمومی)
    {
      path: '/login',
      element: <Login />,
      errorElement: <ErrorPage />,
    },
    // همه مسیرهای محافظت‌شده
    {
      path: '/',
      element: (
        <ProtectedRoute>
          <Layout />
        </ProtectedRoute>
      ),
      errorElement: <ErrorPage />,
      children: [
        {
          index: true,
          // element: <Home />,
          element: <IndicatorValueInput />,
        },
      {
        path: 'indicatorValueInput',
        element: <IndicatorValueInput />, // ← اینجا اضافه شده
      },
      {
        path: 'IndicatorForm',
        element: <IndicatorForm />, 
      },       
      {
        path: 'IndicatorAssignToCategory',
        element: <IndicatorAssignToCategory />,
      },      
      {
        path: 'IndicatorWizardStepper',
        element: <IndicatorWizardStepper />, 
      },
      
      {
        path: 'IndicatorAssignToClaim',
        element: <IndicatorAssignToClaim />, 
      },
      
      {
        path: 'RegisterIndicatorForm',
        element: <RegisterIndicatorForm />, 
      },
      {
        path: 'ChangePassword',
        element: <ChangePassword />, 
      },
      ],
    },
    // مسیر catch-all برای 404: بازگرداندن به خانه
    {
      path: '*',
      element: <Navigate to="/" replace />,
    },
  ]);

  return (
    <AuthProvider>
      <RouterProvider router={router} />
    </AuthProvider>
  );
}

export default App;
