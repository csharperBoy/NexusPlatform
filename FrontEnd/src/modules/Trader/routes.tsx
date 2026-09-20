import { Navigate, type RouteObject } from "react-router-dom";
import { TraderLayout } from "./components";
import { BaseInfoPage } from "./pages/BaseInfoPage";
import {
  SingleOrderPage,
  GroupOrderPage,
  MultiGroupOrderPage,
} from "./pages/Shotgun";

export const TraderShotgunPublicRoutes: RouteObject[] = [
  {
    path: "/",
    element: <TraderLayout />,
    children: [
      { index: true, element: <Navigate to="/base" replace /> },
      { path: "base", element: <BaseInfoPage /> },
      { path: "single", element: <SingleOrderPage /> },
      { path: "group", element: <GroupOrderPage /> },
      { path: "multi", element: <MultiGroupOrderPage /> },
    ],
  },
];