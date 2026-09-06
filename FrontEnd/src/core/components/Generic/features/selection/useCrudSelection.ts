import { useCrudStore } from '../../core/useCrudStore';

export function useCrudSelection() {
  const selectedIds = useCrudStore((state) => state.selectedIds);
  const toggleSelection = useCrudStore((state) => state.toggleSelection);
  const clearSelection = useCrudStore((state) => state.clearSelection);

  const hasSelection = selectedIds.length > 0;
  const selectionCount = selectedIds.length;

  return { selectedIds, toggleSelection, clearSelection, hasSelection, selectionCount };
}