// src/core/components/Tree/Tree.tsx
import React from 'react';
import { useTree } from './useTree';
import { TreeProps } from './Tree.types';

function Tree<T>({
  nodes,
  renderNode,
  onNodeClick,
  defaultExpanded,
  expandAll,
  className = '',
}: TreeProps<T>) {
  const {
    expandedNodes,
    handleNodeClick,
    isExpanded,
    getNodeId,
    getNodeChildren,
    getNodeLabel,
  } = useTree({
    nodes,
    defaultExpanded,
    expandAll,
    onNodeClick,
  });

  const renderNodeRecursive = (node: T, level: number = 0): React.ReactNode => {
    const nodeId = getNodeId(node);
    const children = getNodeChildren(node);
    const hasChildren = children && children.length > 0;
    const expanded = isExpanded(nodeId);
    const label = getNodeLabel(node);   // استفاده از تابع استخراج برچسب

    return (
      <div key={nodeId} className="tree-node">
        <div
          className={`tree-node-content flex items-center py-1 px-2 hover:bg-gray-100 cursor-pointer ${level > 0 ? 'mr-4' : ''}`}
          style={{ paddingLeft: `${level * 1.5}rem` }}
          onClick={() => handleNodeClick(node)}
        >
          {hasChildren && (
            <span className="tree-expand-icon w-4 inline-block ml-1">
              {expanded ? '▼' : '▶'}
            </span>
          )}
          {!hasChildren && <span className="w-4 inline-block ml-1" />}
          {renderNode ? (
            renderNode(node, level, expanded)
          ) : (
            <span className="tree-label">{label}</span>
          )}
        </div>
        {hasChildren && expanded && (
          <div className="tree-children">
            {children.map(child => renderNodeRecursive(child, level + 1))}
          </div>
        )}
      </div>
    );
  };

  return <div className={`tree ${className}`}>{nodes.map(node => renderNodeRecursive(node))}</div>;
}

export default Tree;