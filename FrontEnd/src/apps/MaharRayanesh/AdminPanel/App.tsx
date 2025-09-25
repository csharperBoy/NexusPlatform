// apps/MaharRayanesh/AdminPanel/App.tsx
import React from "react";
import { Routes, Route, Navigate } from "react-router-dom";
import { LoginPage } from "@/modules/auth"; // فقط ایمپورت از ماژول
import Dashboard from "./pages/Dashboard";

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/dashboard" element={<Dashboard />} />
      <Route path="*" element={<Navigate to="/login" />} />
    </Routes>
  );
}
