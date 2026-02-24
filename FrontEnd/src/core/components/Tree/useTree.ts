// src/core/components/Tree/useTree.ts
import { useState, useCallback, useEffect } from 'react';
import { UseTreeProps, UseTreeReturn } from './Tree.types';

export function useTree<T>({
  nodes,
  defaultExpanded = false,
  expandAll: expandAllProp = false,
  onNodeClick,
  getNodeId = (node: any) => node.id,
  getNodeChildren = (node: any) => node.children,
  getNodeLabel = (node: any) => node.label || node.id,
  selectable = false,
  selected: controlledSelected,
  defaultSelected = [],
  onSelectionChange,
  cascadeSelection = false,
}: UseTreeProps<T>): UseTreeReturn<T> {
  // ---------- مدیریت expanded ----------
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

  // ---------- مدیریت selected ----------
  const isControlled = controlledSelected !== undefined;
  const [internalSelected, setInternalSelected] = useState<Set<string>>(
    () => new Set(defaultSelected)
  );

  const selectedSet = isControlled ? new Set(controlledSelected) : internalSelected;

  const updateSelection = useCallback((newSet: Set<string>, changedNode?: T) => {
    if (!isControlled) {
      setInternalSelected(newSet);
    }
    const selectedArray = Array.from(newSet);
    // برای یافتن گره‌های انتخاب‌شده، نیاز به جستجو در کل درخت داریم – برای سادگی فقط idها را برمی‌گردانیم
    onSelectionChange?.(selectedArray, []);
  }, [isControlled, onSelectionChange]);

  const toggleSelect = useCallback((nodeId: string, node: T) => {
    if (!selectable) return;

    const newSet = new Set(selectedSet);
    if (newSet.has(nodeId)) {
      newSet.delete(nodeId);
      // اگر cascade فعال باشد، فرزندان را نیز حذف می‌کنیم
      if (cascadeSelection) {
        const children = getNodeChildren(node) || [];
        const childIds = collectAllNodeIds(children);
        childIds.forEach(id => newSet.delete(id));
      }
    } else {
      newSet.add(nodeId);
      // اگر cascade فعال باشد، فرزندان را نیز اضافه می‌کنیم
      if (cascadeSelection) {
        const children = getNodeChildren(node) || [];
        const childIds = collectAllNodeIds(children);
        childIds.forEach(id => newSet.add(id));
      }
    }
    updateSelection(newSet, node);
  }, [selectable, selectedSet, getNodeChildren, collectAllNodeIds, cascadeSelection, updateSelection]);

  const isSelected = useCallback((nodeId: string) => selectedSet.has(nodeId), [selectedSet]);

  const selectAll = useCallback(() => {
    if (!selectable) return;
    const allIds = collectAllNodeIds(nodes);
    const newSet = new Set(allIds);
    updateSelection(newSet);
  }, [selectable, nodes, collectAllNodeIds, updateSelection]);

  const clearSelection = useCallback(() => {
    if (!selectable) return;
    updateSelection(new Set());
  }, [selectable, updateSelection]);

  // اگر `nodes` تغییر کند و در حالت controlled نیستیم، می‌توانیم انتخاب‌های نامعتبر را پاک کنیم (اختیاری)
  useEffect(() => {
    if (!isControlled && selectable) {
      // حذف idهایی که دیگر در درخت وجود ندارند (اختیاری)
    }
  }, [nodes, isControlled, selectable]);

  return {
    nodes,
    expandedNodes,
    toggleNode,
    expandAll,
    collapseAll,
    isExpanded,
    handleNodeClick,
    getNodeId,
    getNodeChildren,
    getNodeLabel,
    selectedNodes: selectedSet,
    toggleSelect,
    isSelected,
    selectAll,
    clearSelection,
  };
}