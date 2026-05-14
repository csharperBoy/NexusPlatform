// modules/DashboardCore/components/Sidebar.tsx
import { NavLink } from 'react-router-dom';
import { useMenu } from '@/core/hooks/useMenu';
import { getIconComponent } from '@/core/components/IconMapper';
import { MenuDto } from '@/core/models/Menu';

export const Sidebar: React.FC = () => {
  const { menus, loading } = useMenu();

  if (loading) return <div className="p-4">در حال بارگذاری...</div>;

  return (
    <aside className="bg-gray-800 text-white w-64 flex-shrink-0">
      <div className="p-4 text-xl font-bold border-b border-gray-700">
        پنل مدیریت
      </div>
      <nav className="mt-4">
        {menus.map((m: MenuDto) => (
          <NavLink
            key={m.id}
            to={m.path}
            className={({ isActive }) =>
              `flex items-center py-3 px-4 hover:bg-gray-700 transition-colors ${
                isActive ? 'bg-gray-700' : ''
              }`
            }
          >
            {getIconComponent(m.icon)}
            <span>{m.title}</span>
          </NavLink>
        ))}
      </nav>
    </aside>
  );
};
