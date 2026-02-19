// modules/DashboardCore/types/index.ts
import { ComponentType, LazyExoticComponent } from 'react';

export type WidgetArea = 'sidebar' | 'main' | 'widgets' | 'header' | string;

export interface DashboardWidget {
  id: string;
  area: WidgetArea;
  priority?: number;
  component: LazyExoticComponent<ComponentType<any>> | ComponentType<any>;
  props?: Record<string, any>;
  permissions?: string[];
  title?: string;
  icon?: React.ReactNode;
}

export interface MenuItem {
  id: string;
  title: string;
  path: string;
  icon?: React.ReactNode;
  permissions?: string[];
  order?: number;
  parentId?: string; // برای منوهای چند سطحی (اختیاری)
}

