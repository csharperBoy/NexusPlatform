// src/core/components/Tree/Tree.types.ts

export interface TreeNodeBase {
  id: string;
  label: string;
  children?: this[];
  [key: string]: any;
}

export interface TreeProps<T = TreeNodeBase> extends UseTreeProps<T> {
  renderNode?: (node: T, level: number, isExpanded: boolean, isSelected: boolean) => React.ReactNode;
  className?: string;
  dir?: 'ltr' | 'rtl';
}

export interface UseTreeProps<T> {
  nodes: T[];
  defaultExpanded?: boolean | string[];
  expandAll?: boolean;
  onNodeClick?: (node: T) => void;
  getNodeId?: (node: T) => string;
  getNodeChildren?: (node: T) => T[] | undefined;
  getNodeLabel?: (node: T) => string;
  // قابلیت انتخاب با چک‌باکس
  selectable?: boolean;
  selected?: string[];            // برای کنترل خارجی
  defaultSelected?: string[];     // برای کنترل داخلی
  onSelectionChange?: (selectedIds: string[], selectedNodes: T[]) => void;
  cascadeSelection?: boolean;     // اگر true، انتخاب والد همه فرزندان را نیز انتخاب می‌کند
}

export interface UseTreeReturn<T> {
  nodes: T[];
  expandedNodes: Set<string>;
  toggleNode: (nodeId: string) => void;
  expandAll: () => void;
  collapseAll: () => void;
  isExpanded: (nodeId: string) => boolean;
  handleNodeClick: (node: T) => void;
  getNodeId: (node: T) => string;
  getNodeChildren: (node: T) => T[] | undefined;
  getNodeLabel: (node: T) => string;
  // قابلیت انتخاب
  selectedNodes: Set<string>;
  toggleSelect: (nodeId: string, node: T) => void;
  isSelected: (nodeId: string) => boolean;
  selectAll: () => void;
  clearSelection: () => void;
}