import { NavLink, Outlet } from "react-router-dom";
import { SchedulerRunner } from "./SchedulerRunner";

const links = [
  { to: "/base", label: "اطلاعات پایه" },
  { to: "/single", label: "تک‌سفارش" },
  { to: "/group", label: "چندسفارش" },
  { to: "/multi", label: "چندحساب" },
  { to: "/scheduler", label: "زمان‌بندی" },
];

export function TraderLayout() {
  return (
    <div dir="rtl" className="min-h-screen bg-slate-950 text-slate-100">
      <SchedulerRunner />
      <nav className="sticky top-0 z-50 flex flex-wrap justify-center gap-2 border-b border-slate-800 bg-slate-900 px-4 py-3">
        {links.map((l) => (
          <NavLink
            key={l.to}
            to={l.to}
            className={({ isActive }) =>
              [
                "rounded-lg border px-4 py-2 text-sm font-semibold transition-colors",
                isActive
                  ? "border-green-500 bg-green-500 text-slate-950"
                  : "border-slate-700 bg-slate-800 text-blue-300 hover:bg-slate-700",
              ].join(" ")
            }
          >
            {l.label}
          </NavLink>
        ))}
      </nav>
      <div className="mx-auto max-w-4xl p-6">
        <Outlet />
      </div>
    </div>
  );
}