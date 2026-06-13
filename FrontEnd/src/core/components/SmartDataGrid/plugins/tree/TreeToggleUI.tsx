// src/core/components/SmartDataGrid/plugins/tree/TreeToggleUI.tsx
import React from 'react';
import { useGridContext } from '../../core/SmartDataGridContext';

interface TreeToggleUIProps {
  rowKey: string | number;
  depth: number;
  hasChildren: boolean;
  children: React.ReactNode; // محتوای اصلی سلول (مثلاً عنوان پرمیشن)
}

export function TreeToggleUI({ rowKey, depth, hasChildren, children }: TreeToggleUIProps) {
  const instance = useGridContext();
  const treeState = instance.pluginState.tree;

  // اگر کلاً ساختار درختی برای این گرید فعال نباشد، دیتا را عادی رندر کن
  if (!treeState) return <>{children}</>;

  const isExpanded = treeState.expandedKeys?.has(rowKey);

  return (
    <div 
      className="flex items-center gap-2"
      // ایجاد تورفتگی بر اساس عمق (به خاطر راست‌چین بودن از paddingRight استفاده می‌کنیم)
      style={{ paddingRight: `${depth * 20}px` }} 
    >
      {/* دکمه باز و بسته کردن (فقط اگر فرزند داشته باشد) */}
      {hasChildren ? (
        <button 
          type="button"
          onClick={() => instance.actions.toggleRowExpand?.(rowKey)}
          className="w-5 h-5 flex items-center justify-center text-gray-500 hover:bg-gray-100 rounded transition cursor-pointer"
        >
          {isExpanded ? '▼' : '◀'}
        </button>
      ) : (
        // یک المان خالی هم‌اندازه دکمه برای اینکه متن‌ها تراز بمانند
        <span className="w-5 h-5 inline-block"></span>
      )}
      
      {/* محتوای اصلی سلول */}
      <div className="flex-1">{children}</div>
    </div>
  );
}