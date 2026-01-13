import { LogoutButton } from "@/modules/auth/components/Buttons/LogoutButton";
import React from "react";

const Dashboard: React.FC = () => {
  return (
    <div className="p-6">
      <h1 className="text-3xl font-bold">داشبورد ادمین</h1>
      <p className="mt-4">خوش آمدید! شما وارد شده‌اید.</p>
      <LogoutButton className="px-4 py-2 bg-red-500 text-white rounded hover:bg-red-600" />
    </div>
  );
};

export default Dashboard;
