// src/core/components/Tree/useTree.ts
import { useState, useCallback } from 'react';
import { UseTreeProps, UseTreeReturn } from './Tree.types';

export function useTree<T>({
  nodes,
  defaultExpanded = false,
  expandAll: expandAllProp = false,   // تغییر نام prop ورودی
  onNodeClick,
  getNodeId = (node: any) => node.id,
  getNodeChildren = (node: any) => node.children,
  getNodeLabel = (node: any) => node.label || node.id,  // پیش‌فرض
}: UseTreeProps<T>): UseTreeReturn<T> {
  // تابع بازگشتی برای جمع‌آوری همه شناسه‌ها
  const collectAllNodeIds = useCallback((nodes: T[]): string[] => {
    let ids: string[] = [];
    nodes.forEach(node => {
      const nodeId = getNodeId(node);
      if (nodeId) ids.push(nodeId);
      const children = getNodeChildren(node);
      if (children) ids = ids.concat(collectAllNodeIds(children));
    });
    return ids;
  }, [getNodeId, getNodeChildren]);

  const [expandedNodes, setExpandedNodes] = useState<Set<string>>(() => {
    if (expandAllProp) {
      return new Set(collectAllNodeIds(nodes));
    }
    if (Array.isArray(defaultExpanded)) {
      return new Set(defaultExpanded);
    }
    return new Set<string>();
  });

  const toggleNode = useCallback((nodeId: string) => {
    setExpandedNodes(prev => {
      const next = new Set(prev);
      if (next.has(nodeId)) next.delete(nodeId);
      else next.add(nodeId);
      return next;
    });
  }, []);

  const expandAll = useCallback(() => {
    setExpandedNodes(new Set(collectAllNodeIds(nodes)));
  }, [nodes, collectAllNodeIds]);

  const collapseAll = useCallback(() => {
    setExpandedNodes(new Set());
  }, []);

  const isExpanded = useCallback((nodeId: string) => expandedNodes.has(nodeId), [expandedNodes]);

  const handleNodeClick = useCallback((node: T) => {
    const nodeId = getNodeId(node);
    if (nodeId) toggleNode(nodeId);
    onNodeClick?.(node);
  }, [getNodeId, toggleNode, onNodeClick]);

  return {
    nodes,
    expandedNodes,
    toggleNode,
    expandAll,         // تابع بازگشتی
    collapseAll,
    isExpanded,
    handleNodeClick,
    getNodeId,
    getNodeChildren,
    getNodeLabel,
  };
}