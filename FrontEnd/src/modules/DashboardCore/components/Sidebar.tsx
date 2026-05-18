// modules/DashboardCore/components/Sidebar.tsx
import { NavLink } from 'react-router-dom';
import { useMenu } from '@/core/hooks/useMenu';
import { getIconComponent } from '@/core/components/IconMapper';
import { MenuDto } from '@/core/models/Menu';


const renderMenuItems = (items: MenuDto[], depth = 0): React.ReactNode => {
  return items
    .sort((a, b) => a.order - b.order)
    .map((item) => (
      <div key={item.id}>
        {/* آیتم منو */}
        <NavLink
          to={item.path}
          className={({ isActive }) =>
            `flex items-center py-3 px-4 hover:bg-gray-700 transition-colors ${
              isActive ? 'bg-gray-700' : ''
            }`
          }
          style={{ paddingRight: depth * 16 }}   // indent برای زیرمجموعه‌ها
        >
          {getIconComponent(item.icon)}
          <span>{item.title}</span>
        </NavLink>

        {/* ریکرسیو برای children */}
        {item.children && item.children.length > 0 && (
          <div style={{ paddingLeft: 16 }}>
            {renderMenuItems(item.children, depth + 1)}
          </div>
        )}
      </div>
    ));
};

export const Sidebar: React.FC = () => {
  const { menus, loading } = useMenu();

  if (loading) return <div className="p-4">در حال بارگذاری…</div>;

  return (
    <aside className="bg-gray-800 text-white w-64 flex-shrink-0">
      <div className="p-4 text-xl font-bold border-b border-gray-700">
        پنل مدیریت
      </div>
      <nav className="mt-4">{renderMenuItems(menus)}</nav>
    </aside>
  );
};
