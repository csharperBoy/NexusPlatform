// src/core/components/Table/CustomTable.tsx
import React from 'react';
import { useTable } from './useTable';
import { UseTableProps, UseTableReturn } from './Table.type';

export interface CustomTableProps<T> extends UseTableProps<T> {
  className?: string;
}

export function CustomTable<T>({
  className,
  ...useTableProps
}: CustomTableProps<T>) {
  const treeState = useTable(useTableProps);
//   return <div className={`custom-tree ${className || ''}`}>{children(treeState)}</div>;

  return;
}