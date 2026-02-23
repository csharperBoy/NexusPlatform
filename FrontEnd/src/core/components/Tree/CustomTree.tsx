// src/core/components/Tree/CustomTree.tsx
import React from 'react';
import { useTree } from './useTree';
import { UseTreeProps, UseTreeReturn } from './Tree.types';

export interface CustomTreeProps<T> extends UseTreeProps<T> {
  children: (props: UseTreeReturn<T>) => React.ReactNode;
  className?: string;
}

export function CustomTree<T>({
  children,
  className,
  ...useTreeProps
}: CustomTreeProps<T>) {
  const treeState = useTree(useTreeProps);
//   return <div className={`custom-tree ${className || ''}`}>{children(treeState)}</div>;

  return <>{children(treeState)}</>;
}