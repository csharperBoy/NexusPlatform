// src/Core/components/IconMapper.tsx
import * as Fa from 'react-icons/fa';
import * as Md from 'react-icons/md';
import * as Gi from 'react-icons/gi';
import * as Bi from 'react-icons/bi';
import * as Ai from 'react-icons/ai';
// … سایر کتابخانه‌های مورد نیاز

/**
 * تبدیل رشته مثل "fa-solid:folder" → کامپوننت React
 */
const parseIconKey = (key: string): string => {
  // حذف "fa-solid:" یا "md:" و تبدیل به camelCase
  const [prefix, name] = key.split(':');
  // اگر نام با '-' جدا شده باشد، به camelCase تبدیل می‌کنیم
  const camel = name.replace(/-([a-z])/g, (_, g) => g.toUpperCase());
  return prefix === 'fa' ? `Fa${camel}` :
         prefix === 'md' ? `Md${camel}` :
         prefix === 'gi' ? `Gi${camel}` :
         prefix === 'bi' ? `Bi${camel}` :
         prefix === 'ai' ? `Ai${camel}` :
         name; // fallback
};

const iconMap: Record<string, React.ComponentType> = {
  // اضافه کردن به‌صورت دستی یا خودکار
  FaFolder: Fa.FaFolder,
  MdFolder: Md.MdFolder,
  GiFolderOpen: Gi.GiOpenFolder,
  BiCog: Bi.BiCog,
  AiOutlineUser: Ai.AiOutlineUser,
  // ...
};

export const getIconComponent = (iconKey: string): React.ReactNode => {
  if (!iconKey) return null;

  const compKey = parseIconKey(iconKey);
  const IconComp = iconMap[compKey as keyof typeof iconMap];

  if (!IconComp) {
    console.warn(`Icon not found for key: ${iconKey} (parsed: ${compKey})`);
    return null;
  }

  return <IconComp />;
};
