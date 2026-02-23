// src/core/components/Tree/Tree.types.ts

export interface TreeNodeBase {
  id: string;
  label: string;
  children?: this[];
  [key: string]: any;
}

export interface TreeProps<T = TreeNodeBase> {
  nodes: T[];
  renderNode?: (node: T, level: number, isExpanded: boolean) => React.ReactNode;
  onNodeClick?: (node: T) => void;
  defaultExpanded?: boolean | string[];
  expandAll?: boolean;          // prop ورودی
  className?: string;
}

export interface UseTreeProps<T> {
  nodes: T[];
  defaultExpanded?: boolean | string[];
  expandAll?: boolean;          // prop ورودی (تغییر نام ندادیم، ولی در هوک با نام دیگر استفاده می‌کنیم)
  onNodeClick?: (node: T) => void;
  getNodeId?: (node: T) => string;
  getNodeChildren?: (node: T) => T[] | undefined;
  getNodeLabel?: (node: T) => string;  // تابع استخراج برچسب
}

export interface UseTreeReturn<T> {
  nodes: T[];
  expandedNodes: Set<string>;
  toggleNode: (nodeId: string) => void;
  expandAll: () => void;        // تابع بازگشتی
  collapseAll: () => void;
  isExpanded: (nodeId: string) => boolean;
  handleNodeClick: (node: T) => void;
  getNodeId: (node: T) => string;
  getNodeChildren: (node: T) => T[] | undefined;
  getNodeLabel: (node: T) => string;   // تابع استخراج برچسب
}