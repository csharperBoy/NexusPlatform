import { useState, useEffect, useMemo, useCallback, useRef } from "react";
import * as XLSX from "xlsx";
import { SelectionListDto } from "@/core/models/SelectionListDto";
import { ApiOptions } from "@/core/api/apiOptions";
import {
  FlattenedTreeNode,
  TreeColumnDef,
  TreeDeleteTarget,
  UseGenericTreeCrudOptions,
} from "../types";
import { HierarchicalEntity } from "@/core/models/HierarchicalEntity";

// ─────────────────────────────────────────────────────────────────────
//  Helpers: طبق قرارداد HierarchicalEntity، فیلدها همیشه id و parentId هستن.
// ─────────────────────────────────────────────────────────────────────

const normalizeParentId = (v: unknown): string | null =>
  v == null || v === "" ? null : String(v);

export function useGenericTreeCrud<
  T extends HierarchicalEntity,
  TCreateCmd,
  TUpdateCmd
>({
  api,
  apiOptions,
  columns,
  selectionApis,
  getCreateDefaults,
  mapToUpdateCommand,
  mapToCreateCommand,
  transformApiData,
  excelMatchKey,
  getDisplayTitle,
  isItemModified,
  sortChildren,
  tableFeatures,
  pageFeatures,
}: UseGenericTreeCrudOptions<T, TCreateCmd, TUpdateCmd>) {
  // ─── feature flags ───
  const enableDragDrop = tableFeatures?.enableDragDrop !== false;
  const enableMultiSelect = tableFeatures?.enableMultiSelect !== false;
  const showStatusColumn = tableFeatures?.enableStatusColumn !== false;
  const enableInlineAddChild = tableFeatures?.enableInlineAddChild === true;

  // ─── state ───
  const [items, setItems] = useState<T[]>([]);
  const [initialItems, setInitialItems] = useState<T[]>([]);
  const [newItemIds, setNewItemIds] = useState<Set<string>>(new Set());
  const [selectionLists, setSelectionLists] = useState<
    Record<string, SelectionListDto[]>
  >({});
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const [deleteTarget, setDeleteTarget] = useState<TreeDeleteTarget<T> | null>(
    null
  );
  const [isDeleting, setIsDeleting] = useState(false);

  const [globalSearch, setGlobalSearch] = useState("");
  const [columnFilters, setColumnFilters] = useState<Record<string, string>>({});
  const [expandedIds, setExpandedIds] = useState<Set<string>>(new Set());
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [lastSelectedId, setLastSelectedId] = useState<string | null>(null);
  const [draggedIds, setDraggedIds] = useState<string[]>([]);
  const [dragOverId, setDragOverId] = useState<string | null>(null);
  const [isOverRootZone, setIsOverRootZone] = useState(false);

  const fileInputRef = useRef<HTMLInputElement | null>(null);
  const draggedIdsRef = useRef<string[]>([]);
  draggedIdsRef.current = draggedIds;

  // ─── helpers ───
 const getItemId = useCallback((item: T): string => item.id, []);
 
  const getParentId = useCallback(
    (item: T): string | null => normalizeParentId(item.parentId),
    []
  );

  const itemsMap = useMemo(() => {
    const m = new Map<string, T>();
    items.forEach((it) => m.set(getItemId(it), it));
    return m;
  }, [items, getItemId]);

  const initialItemsMap = useMemo(() => {
    const m = new Map<string, T>();
    initialItems.forEach((it) => m.set(getItemId(it), it));
    return m;
  }, [initialItems, getItemId]);

  // ─── fetch ───
  const fetchData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const listPromise = api.getList();
      const selectionPromises = selectionApis
        ? Object.entries(selectionApis).map(async ([key, fetcher]) => {
            const res = await fetcher();
            return { key, data: res as SelectionListDto[] };
          })
        : [];

      const [listData, ...selections] = await Promise.all([
        listPromise,
        ...selectionPromises,
      ]);

      const processed = transformApiData
        ? transformApiData(listData || [])
        : listData || [];

      setItems(processed);
      setInitialItems(JSON.parse(JSON.stringify(processed)));
      setNewItemIds(new Set());

      const selObj: Record<string, SelectionListDto[]> = {};
      selections.forEach((s: any) => (selObj[s.key] = s.data));
      setSelectionLists(selObj);

      const parentIds = new Set<string>();
      processed.forEach((p) => {
        const pid = getParentId(p);
        if (pid) parentIds.add(pid);
      });
      setExpandedIds(parentIds);
      setSelectedIds(new Set());
    } catch (err: any) {
      setError(err?.message || "خطا در دریافت اطلاعات");
    } finally {
      setLoading(false);
    }
  }, [api, selectionApis, transformApiData, getParentId]);

  // ─── detect modified ───
  const compareItems = useCallback(
    (current: T, initial: T): boolean => {
      if (isItemModified) return isItemModified(current, initial);
      return JSON.stringify(current) !== JSON.stringify(initial);
    },
    [isItemModified]
  );

  const modifiedIds = useMemo(() => {
    const ids = new Set<string>();
    items.forEach((item) => {
      const id = getItemId(item);
      if (newItemIds.has(id)) return;
      const init = initialItemsMap.get(id);
      if (!init) return;
      if (compareItems(item, init)) ids.add(id);
    });
    return ids;
  }, [items, initialItemsMap, compareItems, getItemId, newItemIds]);

  const hasChanges = modifiedIds.size > 0 || newItemIds.size > 0;
  const totalChanges = modifiedIds.size + newItemIds.size;

  useEffect(() => {
    fetchData();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // ─── searchable text ───
  const getColumnSearchText = useCallback(
    (item: T, col: TreeColumnDef<T>): string => {
      if (col.getFilterValue) return col.getFilterValue(item) || "";
      const raw = item[col.key as keyof T];
      if (raw == null) return "";
      if (col.selectionKey && selectionLists[col.selectionKey]) {
        const list = selectionLists[col.selectionKey];
        if (Array.isArray(raw)) {
          return (raw as any[])
            .map((v) => {
              const opt = list.find((o) => String(o.value) === String(v));
              return opt ? opt.display || opt.label || "" : String(v);
            })
            .join(" ");
        }
        const opt = list.find((o) => String(o.value) === String(raw));
        return opt ? opt.display || opt.label || "" : String(raw);
      }
      if (Array.isArray(raw)) return (raw as any[]).join(" ");
      return String(raw);
    },
    [selectionLists]
  );

  const matchesFilters = useCallback(
    (item: T): boolean => {
      if (globalSearch.trim()) {
        const q = globalSearch.toLowerCase();
        const haystack = columns
          .map((c) => getColumnSearchText(item, c))
          .join(" ")
          .toLowerCase();
        if (!haystack.includes(q)) return false;
      }
      for (const [colKey, term] of Object.entries(columnFilters)) {
        if (!term.trim()) continue;
        const col = columns.find((c) => String(c.key) === colKey);
        if (!col) continue;
        const text = getColumnSearchText(item, col).toLowerCase();
        if (!text.includes(term.toLowerCase())) return false;
      }
      return true;
    },
    [globalSearch, columnFilters, columns, getColumnSearchText]
  );

  // ─── flatten tree ───
  const flattenedTree: FlattenedTreeNode<T>[] = useMemo(() => {
    const childrenMap = new Map<string | null, T[]>();
    items.forEach((item) => {
      const pid = getParentId(item);
      const parentKey = pid && itemsMap.has(pid) ? pid : null;
      if (!childrenMap.has(parentKey)) childrenMap.set(parentKey, []);
      childrenMap.get(parentKey)!.push(item);
    });
    if (sortChildren) childrenMap.forEach((arr) => arr.sort(sortChildren));

    const isSearching =
      globalSearch.trim() !== "" ||
      Object.values(columnFilters).some((v) => v.trim() !== "");

    const visibleSet = new Set<string>();
    const matchedSet = new Set<string>();
    if (isSearching) {
      items.forEach((it) => {
        if (matchesFilters(it)) {
          const id = getItemId(it);
          matchedSet.add(id);
          visibleSet.add(id);
          let cur: T | undefined = it;
          while (cur) {
            const pid = getParentId(cur);
            if (!pid) break;
            visibleSet.add(pid);
            cur = itemsMap.get(pid);
          }
        }
      });
    }

    const result: FlattenedTreeNode<T>[] = [];
    const traverse = (parentId: string | null, depth: number) => {
      const children = childrenMap.get(parentId) || [];
      for (const child of children) {
        const id = getItemId(child);
        if (isSearching && !visibleSet.has(id)) continue;

        const grandChildren = childrenMap.get(id) || [];
        const hasChildren = grandChildren.length > 0;
        const isExpanded = expandedIds.has(id);

        result.push({
          node: child,
          depth,
          hasChildren,
          isExpanded,
          isModified: modifiedIds.has(id),
          isSelected: selectedIds.has(id),
          isDragging: draggedIds.includes(id),
          isDragOver: dragOverId === id,
          isNew: newItemIds.has(id),
          matchesSearch: !isSearching || matchedSet.has(id),
        });

        if ((isExpanded || isSearching) && hasChildren) {
          traverse(id, depth + 1);
        }
      }
    };
    traverse(null, 0);
    return result;
  }, [
    items,
    itemsMap,
    expandedIds,
    modifiedIds,
    selectedIds,
    draggedIds,
    dragOverId,
    matchesFilters,
    globalSearch,
    columnFilters,
    getItemId,
    getParentId,
    sortChildren,
    newItemIds,
  ]);

  // ─── edit ───
  const handleFieldChange = useCallback(
    (id: string, field: keyof T | string, value: any) => {
      setItems((prev) =>
        prev.map((it) =>
          getItemId(it) === id ? ({ ...it, [field]: value } as T) : it
        )
      );
    },
    [getItemId]
  );

  // ─── inline add child ───
  const handleAddChild = useCallback(
    (parentId: string | null): string => {
      const tempId = `temp-${Date.now()}-${Math.random()
        .toString(36)
        .slice(2, 9)}`;

      const draft: Record<string, any> = {
        id: tempId,
        parentId: parentId,
      };

      columns.forEach((col) => {
        if (col.type === "multi-select" || col.type === "taginput") {
          draft[col.key as string] = [];
        } else if (col.type === "boolean") {
          draft[col.key as string] = false;
        } else {
          draft[col.key as string] = "";
        }
      });

      if (getCreateDefaults) {
        Object.assign(draft, getCreateDefaults());
      }

      setItems((prev) => [draft as unknown as T, ...prev]);
      setNewItemIds((prev) => new Set(prev).add(tempId));
      if (parentId) {
        setExpandedIds((prev) => new Set(prev).add(parentId));
      }
      setSelectedIds(new Set([tempId]));
      setLastSelectedId(tempId);
      return tempId;
    },
    [columns, getCreateDefaults]
  );

  // ─── discard new ───
  const handleDiscardNew = useCallback(
    (id: string) => {
      setItems((prev) => prev.filter((it) => getItemId(it) !== id));
      setNewItemIds((prev) => {
        const next = new Set(prev);
        next.delete(id);
        return next;
      });
      setSelectedIds((prev) => {
        const next = new Set(prev);
        next.delete(id);
        return next;
      });
      setLastSelectedId((prev) => (prev === id ? null : prev));
    },
    [getItemId]
  );

  // ─── selection ───
  const handleRowClick = useCallback(
    (e: React.MouseEvent, id: string) => {
      const tag = (e.target as HTMLElement).tagName;
      if (tag === "INPUT" || tag === "BUTTON" || tag === "SELECT") return;

      if (!enableMultiSelect) {
        setSelectedIds(new Set([id]));
        setLastSelectedId(id);
        return;
      }

      if (e.ctrlKey || e.metaKey) {
        setSelectedIds((prev) => {
          const next = new Set(prev);
          next.has(id) ? next.delete(id) : next.add(id);
          return next;
        });
        setLastSelectedId(id);
      } else if (e.shiftKey && lastSelectedId) {
        const flatIds = flattenedTree.map((x) => getItemId(x.node));
        const a = flatIds.indexOf(lastSelectedId);
        const b = flatIds.indexOf(id);
        if (a !== -1 && b !== -1) {
          const [s, e2] = a < b ? [a, b] : [b, a];
          setSelectedIds((prev) => {
            const next = new Set(prev);
            flatIds.slice(s, e2 + 1).forEach((x) => next.add(x));
            return next;
          });
        }
      } else {
        setSelectedIds(new Set([id]));
        setLastSelectedId(id);
      }
    },
    [enableMultiSelect, lastSelectedId, flattenedTree, getItemId]
  );

  // ─── expand/collapse ───
  const toggleExpand = useCallback((id: string) => {
    setExpandedIds((prev) => {
      const next = new Set(prev);
      next.has(id) ? next.delete(id) : next.add(id);
      return next;
    });
  }, []);

  const expandAll = useCallback(() => {
    const parents = new Set<string>();
    items.forEach((p) => {
      if (items.some((c) => getParentId(c) === getItemId(p)))
        parents.add(getItemId(p));
    });
    setExpandedIds(parents);
  }, [items, getItemId, getParentId]);

  const collapseAll = useCallback(() => setExpandedIds(new Set()), []);

  // ─── drag & drop ───
  const isDescendant = useCallback(
    (targetId: string, ancestorId: string): boolean => {
      let cur: string | null | undefined = targetId;
      while (cur) {
        if (cur === ancestorId) return true;
        const node = itemsMap.get(cur);
        cur = node ? getParentId(node) : null;
      }
      return false;
    },
    [itemsMap, getParentId]
  );

  const handleDragStart = useCallback(
    (e: React.DragEvent, id: string) => {
      if (!enableDragDrop) return;
      let idsToMove: string[];
      if (selectedIds.has(id)) idsToMove = Array.from(selectedIds);
      else {
        idsToMove = [id];
        setSelectedIds(new Set([id]));
        setLastSelectedId(id);
      }
      e.dataTransfer.setData("text/plain", JSON.stringify(idsToMove));
      e.dataTransfer.effectAllowed = "move";
      setDraggedIds(idsToMove);
    },
    [enableDragDrop, selectedIds]
  );

  const handleDragOverRow = useCallback(
    (e: React.DragEvent, targetId: string) => {
      if (!enableDragDrop) return;
      e.preventDefault();
      const cur = draggedIdsRef.current;
      if (!cur.length || cur.includes(targetId)) return;
      const invalid = cur.some((d) => isDescendant(targetId, d));
      if (invalid) {
        e.dataTransfer.dropEffect = "none";
        return;
      }
      e.dataTransfer.dropEffect = "move";
      if (dragOverId !== targetId) setDragOverId(targetId);
    },
    [enableDragDrop, isDescendant, dragOverId]
  );

  const updateNodesParent = useCallback(
    (nodeIds: string[], newParentId: string | null) => {
      const set = new Set(nodeIds);
      setItems((prev) =>
        prev.map((it) =>
          set.has(getItemId(it))
            ? ({ ...it, parentId: newParentId } as T)
            : it
        )
      );
      if (newParentId) {
        setExpandedIds((prev) => new Set(prev).add(newParentId));
      }
    },
    [getItemId]
  );

  const handleDropOnRow = useCallback(
    (e: React.DragEvent, targetParentId: string) => {
      if (!enableDragDrop) return;
      e.preventDefault();
      setDragOverId(null);
      setIsOverRootZone(false);

      let idsToMove: string[] = [];
      try {
        idsToMove = JSON.parse(e.dataTransfer.getData("text/plain"));
      } catch {
        idsToMove = draggedIds;
      }
      if (!idsToMove.length) return;

      let cyclic = false;
      const valid = idsToMove.filter((id) => {
        if (id === targetParentId) return false;
        if (isDescendant(targetParentId, id)) {
          cyclic = true;
          return false;
        }
        const node = itemsMap.get(id);
        return node && getParentId(node) !== targetParentId;
      });

      if (cyclic) alert("امکان انتقال والد به زیرمجموعه‌های خودش وجود ندارد!");
      if (valid.length) updateNodesParent(valid, targetParentId);
      setDraggedIds([]);
    },
    [enableDragDrop, draggedIds, isDescendant, itemsMap, getParentId, updateNodesParent]
  );

  const handleDropOnRoot = useCallback(
    (e: React.DragEvent) => {
      if (!enableDragDrop) return;
      e.preventDefault();
      setIsOverRootZone(false);
      setDragOverId(null);

      let idsToMove: string[] = [];
      try {
        idsToMove = JSON.parse(e.dataTransfer.getData("text/plain"));
      } catch {
        idsToMove = draggedIds;
      }
      if (!idsToMove.length) return;
      const valid = idsToMove.filter((id) => {
        const n = itemsMap.get(id);
        return n && getParentId(n) !== null;
      });
      if (valid.length) updateNodesParent(valid, null);
      setDraggedIds([]);
    },
    [enableDragDrop, draggedIds, itemsMap, getParentId, updateNodesParent]
  );

  // ─── save all ───
  const handleSaveChanges = useCallback(
    async (options?: ApiOptions) => {
      if (!hasChanges) return;
      setSaving(true);
      setError(null);
      setSuccessMessage(null);
      try {
        const merged = { ...apiOptions, ...options };

        const updateCmds: TUpdateCmd[] = Array.from(modifiedIds).map((id) => {
          const item = itemsMap.get(id)!;
          return mapToUpdateCommand
            ? mapToUpdateCommand(item)
            : (item as unknown as TUpdateCmd);
        });

        const createCmds: TCreateCmd[] = Array.from(newItemIds).map((id) => {
          const item = itemsMap.get(id)!;
          if (mapToCreateCommand) {
            return mapToCreateCommand(
              item as unknown as Record<string, any>,
              getParentId(item)
            );
          }
          return item as unknown as TCreateCmd;
        });

        let offlineQueued = false;
        let createdCount = 0;
        let updatedCount = 0;

        if (createCmds.length) {
          const results = await Promise.all(
            createCmds.map((cmd) => api.create(cmd, merged))
          );
          createdCount = results.length;
          if (results.some((r: any) => r && r._isOfflineQueued))
            offlineQueued = true;
        }

        if (updateCmds.length) {
          const res = await api.batchUpdate(updateCmds, merged);
          updatedCount = updateCmds.length;
          if (res && (res as any)._isOfflineQueued) offlineQueued = true;
        }

        if (offlineQueued) {
          alert("ارتباط قطع است. تغییرات در صف ذخیره شد.");
          setInitialItems(JSON.parse(JSON.stringify(items)));
          setNewItemIds(new Set());
        } else {
          setSuccessMessage(
            `${createdCount} رکورد جدید و ${updatedCount} تغییر با موفقیت ذخیره شد.`
          );
          await fetchData();
          setTimeout(() => setSuccessMessage(null), 4000);
        }
      } catch (err: any) {
        setError(err?.message || "خطا در ذخیره تغییرات");
      } finally {
        setSaving(false);
      }
    },
    [
      hasChanges,
      modifiedIds,
      newItemIds,
      itemsMap,
      mapToUpdateCommand,
      mapToCreateCommand,
      getParentId,
      api,
      apiOptions,
      items,
      fetchData,
    ]
  );

  // ─── reset ───
  const handleResetChanges = useCallback(() => {
    if (!window.confirm("آیا از لغو تمام تغییرات اعتماد دارید؟")) return;
    setItems(JSON.parse(JSON.stringify(initialItems)));
    setNewItemIds(new Set());
    setSelectedIds(new Set());
  }, [initialItems]);

  // ─── delete ───
  const handleOpenDeleteModal = useCallback(
    (item: T) => {
      const id = getItemId(item);
      if (newItemIds.has(id)) {
        handleDiscardNew(id);
        return;
      }
      const title = getDisplayTitle ? getDisplayTitle(item) : `#${id}`;
      setDeleteTarget({ item, title, isModified: modifiedIds.has(id) });
    },
    [getItemId, getDisplayTitle, modifiedIds, newItemIds, handleDiscardNew]
  );

  const handleCloseDeleteModal = useCallback(() => {
    if (isDeleting) return;
    setDeleteTarget(null);
  }, [isDeleting]);

  const handleConfirmDelete = useCallback(
    async (options?: ApiOptions) => {
      if (!deleteTarget) return;
      setIsDeleting(true);
      try {
        const merged = { ...apiOptions, ...options };
        const res = await api.delete(deleteTarget.item.id, merged);
        const id = getItemId(deleteTarget.item);

        if (res && (res as any)._isOfflineQueued) {
          setItems((prev) => prev.filter((i) => getItemId(i) !== id));
          setInitialItems((prev) => prev.filter((i) => getItemId(i) !== id));
          alert("حذف در صف قرار گرفت.");
        } else {
          setSuccessMessage(`«${deleteTarget.title}» حذف شد.`);
          await fetchData();
          setTimeout(() => setSuccessMessage(null), 4000);
        }
        setDeleteTarget(null);
      } catch (err: any) {
        setError(err?.message || "خطا در حذف");
        setDeleteTarget(null);
      } finally {
        setIsDeleting(false);
      }
    },
    [deleteTarget, getItemId, api, apiOptions, fetchData]
  );

  // ─── excel import ───
  const handleExcelImport = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      const file = e.target.files?.[0];
      if (!file) return;
      const reader = new FileReader();
      reader.onload = (evt) => {
        try {
          const wb = XLSX.read(evt.target?.result, { type: "array" });
          const ws = wb.Sheets[wb.SheetNames[0]];
          const rows = XLSX.utils.sheet_to_json<Record<string, any>>(ws);
          if (!rows?.length) {
            alert("فایل خالی است.");
            return;
          }

          let updated = 0;
          setItems((prev) => {
            const next = prev.map((p) => ({ ...p }));
            const byMatchKey = new Map<string, number>();
            if (excelMatchKey) {
              next.forEach((p, idx) => {
                const v = (p as any)[excelMatchKey];
                if (v != null) byMatchKey.set(String(v).trim(), idx);
              });
            }

            const findId = (list: SelectionListDto[], raw: string) => {
              const t = String(raw).trim();
              const found = list.find(
                (o) =>
                  o.value.toLowerCase() === t.toLowerCase() ||
                  o.display?.toLowerCase() === t.toLowerCase() ||
                  o.label?.toLowerCase() === t.toLowerCase()
              );
              return found ? found.value : null;
            };

            rows.forEach((row) => {
              if (!excelMatchKey) return;
              const mkKey = Object.keys(row).find((k) => {
                const kk = k.trim().toLowerCase();
                const col = columns.find(
                  (c) => String(c.key) === String(excelMatchKey)
                );
                const labels = [
                  col?.label?.toLowerCase(),
                  ...(col?.excelHeaders || []).map((h) => h.toLowerCase()),
                  String(excelMatchKey).toLowerCase(),
                ].filter(Boolean);
                return labels.includes(kk);
              });
              if (!mkKey) return;
              const matchVal = String(row[mkKey]).trim();
              const idx = byMatchKey.get(matchVal);
              if (idx == null) return;

              let changed = false;
              columns.forEach((col) => {
                if (String(col.key) === String(excelMatchKey)) return;
                if (col.editable === false) return;

                const headers = [
                  col.label.toLowerCase(),
                  ...(col.excelHeaders || []).map((h) => h.toLowerCase()),
                  String(col.key).toLowerCase(),
                ];
                const key = Object.keys(row).find((k) =>
                  headers.includes(k.trim().toLowerCase())
                );
                if (!key) return;
                const raw = row[key];
                if (raw == null) return;

                const target = next[idx] as any;
                const isMulti = col.type === "multi-select";
                const isSelect = col.type === "select" || isMulti;
                const sep = col.excelSeparator || /[،,;؛]/;

                if (isSelect && col.selectionKey) {
                  const list = selectionLists[col.selectionKey] || [];
                  if (isMulti) {
                    const parts = String(raw)
                      .split(sep)
                      .map((s) => s.trim())
                      .filter(Boolean);
                    const ids = parts
                      .map((p) => findId(list, p))
                      .filter(Boolean) as string[];
                    if (ids.length) {
                      const cur = Array.isArray(target[col.key])
                        ? target[col.key]
                        : [];
                      const curIds = cur
                        .map((x: any) => String(x.id ?? x))
                        .sort();
                      if (JSON.stringify(curIds) !== JSON.stringify(ids.sort())) {
                        target[col.key] = ids.map((id) => ({
                          id,
                          title:
                            list.find((o) => o.value === id)?.display ||
                            list.find((o) => o.value === id)?.label ||
                            id,
                        }));
                        changed = true;
                      }
                    }
                  } else {
                    const id = findId(list, String(raw));
                    if (id && target[col.key] !== id) {
                      target[col.key] = id;
                      changed = true;
                    }
                  }
                } else if (col.type === "taginput") {
                  const arr = String(raw)
                    .split(sep)
                    .map((s) => s.trim())
                    .filter(Boolean);
                  const current = Array.isArray(target[col.key])
                    ? target[col.key]
                    : [];
                  if (JSON.stringify(current) !== JSON.stringify(arr)) {
                    target[col.key] = arr;
                    changed = true;
                  }
                } else {
                  const v =
                    col.type === "number" ? Number(raw) : String(raw).trim();
                  if (target[col.key] !== v) {
                    target[col.key] = v;
                    changed = true;
                  }
                }
              });
              if (changed) updated++;
            });

            if (updated > 0)
              setSuccessMessage(`${updated} رکورد از اکسل اعمال شد.`);
            else alert("رکورد منطبقی یافت نشد یا هیچ تغییری اعمال نشد.");
            return next;
          });
        } catch (err: any) {
          setError("خطا در پردازش اکسل: " + (err?.message || ""));
        } finally {
          e.target.value = "";
        }
      };
      reader.readAsArrayBuffer(file);
    },
    [columns, excelMatchKey, selectionLists]
  );

  return {
    items,
    flattenedTree,
    itemsMap,
    selectionLists,
    loading,
    saving,
    isDeleting,
    error,
    successMessage,

    globalSearch,
    setGlobalSearch,
    columnFilters,
    setColumnFilters,
    handleColumnFilterChange: (k: string, v: string) =>
      setColumnFilters((p) => ({ ...p, [k]: v })),

    expandedIds,
    selectedIds,
    lastSelectedId,
    modifiedIds,
    newItemIds,
    hasChanges,
    totalChanges,
    modifiedCount: modifiedIds.size,
    newCount: newItemIds.size,
    draggedIds,
    dragOverId,
    isOverRootZone,

    toggleExpand,
    expandAll,
    collapseAll,
    handleFieldChange,
    handleRowClick,
    isDescendant,
    handleDragStart,
    handleDragOverRow,
    handleDragStartOverRootZone: (v: boolean) => setIsOverRootZone(v),
    handleDragOverId: (id: string | null) => setDragOverId(id),
    handleDropOnRow,
    handleDropOnRoot,
    updateNodesParent,

    handleAddChild,
    handleDiscardNew,

    handleSaveChanges,
    handleResetChanges,

    deleteTarget,
    handleOpenDeleteModal,
    handleCloseDeleteModal,
    handleConfirmDelete,

    fileInputRef,
    handleExcelImport,

    refresh: fetchData,
  };
}