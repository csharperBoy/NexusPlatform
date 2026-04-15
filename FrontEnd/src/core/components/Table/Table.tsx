// src/core/components/Table/Table.tsx
import React from "react";
import { useTable } from "./useTable";
import { UseTableProps as TableProps } from "./Table.types";

function Table<T>({
  data,
  columns,
  onEdit,
  onDelete,
  pageSize,
  defaultPage,
  controlledPage,
  onPageChange,

  defaultSort,
  sort,
  onSortChange,

  selectable = false,
  cascadeSelection = false,
  selected,
  defaultSelected,
  onSelectionChange,

  getRowId,
   dir = "rtl",
   className = "",
}: TableProps<T>) {
  const {
    paginatedData,
    page,
    setPage,
    totalPages,
    sort: sortState,
    toggleSort,
    selectedSet,
    toggleSelect,
    isSelected,
  } = useTable({
    data,
    columns,
    pageSize,
    defaultPage,
    controlledPage,
    onPageChange,

    defaultSort,
    sort,
    onSortChange,

    selectable,
    cascadeSelection,
    selected,
    defaultSelected,
    onSelectionChange,

    getRowId,
  });

  return (
    <div className={`table-container ${className}`} dir={dir}>
      <table className="w-full border-collapse">
        <thead>
          <tr>
            {selectable && <th className="p-2 w-8"></th>}

            {columns.map(col => {
              const isActive = sortState?.columnId === col.id;
              const arrow = isActive
                ? sortState!.direction === "asc"
                  ? "▲"
                  : "▼"
                : "";

              return (
                <th
                  key={col.id}
                  className="p-2 border text-sm cursor-pointer select-none"
                  style={{ width: col.width }}
                  onClick={() => col.sortable && toggleSort(col.id)}
                >
                  {col.label} {arrow}
                </th>
              );
            })}
          </tr>
        </thead>

        <tbody>
          {paginatedData.map(row => {
            const id = getRowId ? getRowId(row) : (row as any).id;
            const checked = isSelected(row);

            return (
              <tr key={id} className="border">
                {selectable && (
                  <td className="p-2 text-center">
                    <input
                      type="checkbox"
                      checked={checked}
                      onChange={() => toggleSelect(row)}
                    />
                  </td>
                )}
                    {columns.map((col) => (
                    <td key={col.id}>
                    {col.id === 'actions' ? (
                    // اگر ستون، ستون اکشن ها بود
                    <>
                    {/* استفاده از props پاس داده شده */}
                    {onEdit && <button onClick={() => onEdit(id)}>ویرایش</button>}
                    {onDelete && <button onClick={
                        async () => {
                    if (!confirm(`آیا از حذف مطمون هستید ?`)) return;

                    try {
                      await onDelete(id);
                    } catch (e) {
                      alert(e);
                    }
                  }}>حذف</button>}
                    </>
                    ) : col.accessor ? (
                    // اگر accessor تعریف شده بود
                    col.accessor(row)
                    ) : col.render ? (
                    // اگر render تعریف شده بود (برای ستون های خاص)
                    col.render(row)
                    ) : null}
                    </td>
                    ))}
                {/* {columns.map(col => {
                  const content = 
                  
                      col.render
                    ? col.render(row)
                    : col.accessor
                    ? col.accessor(row)
                    : (row as any)[col.id];

                  return (
                    <td key={col.id} className="p-2 border">
                      {content}
                    </td>
                  );
                })} */}
              </tr>
            );
          })}
        </tbody>
      </table>

      {/* Pagination */}
      <div className="flex justify-center gap-3 mt-4">
        <button
          onClick={() => setPage(page - 1)}
          disabled={page <= 1}
          className="px-3 py-1 border rounded"
        >
          قبلی
        </button>

        <span>
          صفحه {page} از {totalPages}
        </span>

        <button
          onClick={() => setPage(page + 1)}
          disabled={page >= totalPages}
          className="px-3 py-1 border rounded"
        >
          بعدی
        </button>
      </div>
    </div>
  );
}

export default Table;
