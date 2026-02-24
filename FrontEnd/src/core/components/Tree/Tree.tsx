import React from 'react';
import { useTree } from './useTree';
import { TreeProps } from './Tree.types';

function Tree<T>({
  nodes,
  renderNode,
  onNodeClick,
  defaultExpanded,
  expandAll,
  getNodeId,
  getNodeChildren,
  getNodeLabel,
  selectable = false,
  selected,
  defaultSelected,
  onSelectionChange,
  cascadeSelection = false,
  className = '',
  dir = 'rtl',
}: TreeProps<T>) {
  const {
    expandedNodes,
    handleNodeClick,
    isExpanded,
    getNodeId: _getNodeId,
    getNodeChildren: _getNodeChildren,
    getNodeLabel: _getNodeLabel,
    selectedNodes,
    toggleSelect,
    isSelected,
  } = useTree({
    nodes,
    defaultExpanded,
    expandAll,
    onNodeClick,
    getNodeId,
    getNodeChildren,
    getNodeLabel,
    selectable,
    selected,
    defaultSelected,
    onSelectionChange,
    cascadeSelection,
  });

  const renderNodeRecursive = (node: T, level: number = 0): React.ReactNode => {
    const nodeId = _getNodeId(node);
    const children = _getNodeChildren(node);
    const hasChildren = children && children.length > 0;
    const expanded = isExpanded(nodeId);
    const label = _getNodeLabel(node);
    const selected = isSelected(nodeId);

    const handleCheckboxChange = (e: React.ChangeEvent<HTMLInputElement>) => {
      e.stopPropagation();
      toggleSelect(nodeId, node);
    };

    // استایل padding بر اساس جهت صفحه
    const paddingStyle = dir === 'rtl' 
      ? { paddingRight: `${level * 1.5}rem` } 
      : { paddingLeft: `${level * 1.5}rem` };

    return (
      <div key={nodeId} className="tree-node">
        <div
          className="tree-node-content flex items-center py-1 px-2 hover:bg-gray-100 cursor-pointer"
          style={paddingStyle}
          onClick={() => handleNodeClick(node)}
        >
          {/* چک‌باکس */}
          {selectable && (
            <input
              type="checkbox"
              checked={selected}
              onChange={handleCheckboxChange}
              onClick={(e) => e.stopPropagation()}
              className={dir === 'rtl' ? 'ml-2' : 'mr-2'}
            />
          )}

          {/* آیکون باز/بسته */}
          {hasChildren && (
            <span className={`tree-expand-icon w-4 inline-block ${dir === 'rtl' ? 'ml-1' : 'mr-1'}`}>
              {expanded ? '▼' : '▶'}
            </span>
          )}
          {!hasChildren && (
            <span className={`w-4 inline-block ${dir === 'rtl' ? 'ml-1' : 'mr-1'}`} />
          )}

          {/* محتوای گره */}
          {renderNode ? (
            renderNode(node, level, expanded, selected)
          ) : (
            <span className="tree-label">{label}</span>
          )}
        </div>

        {/* فرزندان */}
        {hasChildren && expanded && (
          <div className="tree-children">
            {children.map(child => renderNodeRecursive(child, level + 1))}
          </div>
        )}
      </div>
    );
  };

  return (
    <div className={`tree ${className}`} dir={dir}>
      {nodes.map(node => renderNodeRecursive(node))}
    </div>
  );
}

export default Tree;