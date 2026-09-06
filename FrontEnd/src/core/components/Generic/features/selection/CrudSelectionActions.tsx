import React from 'react';
import { useCrudSelection } from './useCrudSelection';

export function CrudSelectionActions() {
  const { hasSelection, selectionCount, clearSelection } = useCrudSelection();

  if (!hasSelection) return null;

  return (
    <div className="bg-blue-50 p-2 rounded flex items-center justify-between">
      <span className="text-blue-700 text-sm font-medium">
        {selectionCount} مورد انتخاب شده است
      </span>
      <div className="flex gap-2">
        <button className="text-sm bg-red-500 text-white px-3 py-1 rounded">
          حذف گروهی
        </button>
        <button onClick={clearSelection} className="text-sm text-gray-500 px-3 py-1">
          لغو انتخاب
        </button>
      </div>
    </div>
  );
}