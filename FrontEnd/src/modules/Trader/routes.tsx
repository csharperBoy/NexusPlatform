import { Navigate, type RouteObject } from "react-router-dom";
import { TraderLayout } from "./components";
import {
  SingleOrderPage,
  ScheduledOrdersPage,
  BaseInfoPage,
  GroupOrderPage,
  MultiGroupOrderPage,
} from "./pages";

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
      { path: "scheduler", element: <ScheduledOrdersPage /> },
    ],
  },
];