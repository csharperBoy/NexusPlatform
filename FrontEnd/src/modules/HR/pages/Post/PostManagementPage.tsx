// src/modules/HR/pages/Post/PostManagementPage.tsx

import React, { useEffect, useState, useMemo, useRef } from "react";
import * as XLSX from "xlsx";
import { postApi } from "../../api/PostApi";
import { PostInfoView } from "../../models/postInfoView";
import { UpdatePostCommand } from "../../models/postCommand";

interface FlattenedNode {
  node: PostInfoView;
  depth: number;
  hasChildren: boolean;
  isExpanded: boolean;
  isModified: boolean;
}

export const PostManagementPage: React.FC = () => {
  // --- States ---
  const [posts, setPosts] = useState<PostInfoView[]>([]);
  const [initialPosts, setInitialPosts] = useState<PostInfoView[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [saving, setSaving] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // استیت‌های سرچ
  const [globalSearch, setGlobalSearch] = useState<string>("");
  const [columnSearch, setColumnSearch] = useState<Record<string, string>>({});

  // مدیریت باز/بسته بودن و تغییرات
  const [expandedIds, setExpandedIds] = useState<Set<string>>(new Set());
  const [modifiedIds, setModifiedIds] = useState<Set<string>>(new Set());

  // --- استیت‌های جدید: مدیریت انتخاب چندگانه (Ctrl / Shift) ---
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [lastSelectedId, setLastSelectedId] = useState<string | null>(null);

  // --- استیت‌های درگ اند دراپ گروهی ---
  const [draggedIds, setDraggedIds] = useState<string[]>([]);
  const [dragOverId, setDragOverId] = useState<string | null>(null);
  const [isOverRootZone, setIsOverRootZone] = useState<boolean>(false);

  // ریف مربوط به آپلود فایل اکسل
  const fileInputRef = useRef<HTMLInputElement | null>(null);

  const draggedIdsRef = useRef<string[]>([]);
  draggedIdsRef.current = draggedIds;

  const initialPostsMap = useMemo(() => {
    const map = new Map<string, PostInfoView>();
    initialPosts.forEach((p) => map.set(p.id, p));
    return map;
  }, [initialPosts]);

  // --- 1. دریافت اطلاعات اولیه ---
  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await postApi.GetList();
      const list = data || [];
      setPosts(list);
      setInitialPosts(JSON.parse(JSON.stringify(list)));

      const parentIds = new Set<string>();
      list.forEach((p) => {
        if (p.fkParentId) parentIds.add(p.fkParentId);
      });
      setExpandedIds(parentIds);
      setModifiedIds(new Set());
      setSelectedIds(new Set());
      setLastSelectedId(null);
    } catch (err: any) {
      setError(err?.message || "خطا در دریافت لیست چارت سازمانی");
    } finally {
      setLoading(false);
    }
  };

  // --- 2. مدیریت ویرایش درجا ---
  const handleFieldChange = (id: string, field: "officePhone" | "orgMobile", value: string) => {
    setPosts((prev) =>
      prev.map((item) => {
        if (item.id === id) {
          return { ...item, [field]: value };
        }
        return item;
      })
    );
    setModifiedIds((prev) => new Set(prev).add(id));
  };

  const handleColumnSearch = (column: string, value: string) => {
    setColumnSearch((prev) => ({ ...prev, [column]: value }));
  };

  // --- مدیریت بارگذاری فایل اکسل ---
  const handleExcelImport = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = (evt) => {
      try {
        setError(null);
        const data = evt.target?.result;
        const workbook = XLSX.read(data, { type: "array" });
        const firstSheetName = workbook.SheetNames[0];
        const worksheet = workbook.Sheets[firstSheetName];

        const excelRows = XLSX.utils.sheet_to_json<Record<string, any>>(worksheet);

        if (!excelRows || excelRows.length === 0) {
          alert("فایل اکسل انتخاب شده خالی است یا فرمت معتبری ندارد.");
          return;
        }

        let updatedCount = 0;
        const newModifiedIds = new Set(modifiedIds);

        setPosts((prevPosts) => {
          const postByEmpCodeMap = new Map<string, PostInfoView>();
          prevPosts.forEach((p) => {
            if (p.employmentCode) {
              postByEmpCodeMap.set(String(p.employmentCode).trim(), p);
            }
          });

          const nextPosts = prevPosts.map((p) => ({ ...p }));

          excelRows.forEach((row) => {
            const empCodeKey = Object.keys(row).find((k) =>
              ["کد پرسنلی", "کدپرسنلی", "employmentcode", "empcode", "کد"].includes(
                k.trim().toLowerCase()
              )
            );
            const officePhoneKey = Object.keys(row).find((k) =>
              ["تلفن داخلی", "داخلی", "officephone", "phone"].includes(
                k.trim().toLowerCase()
              )
            );
            const orgMobileKey = Object.keys(row).find((k) =>
              ["موبایل سازمانی", "موبایل", "orgmobile", "mobile"].includes(
                k.trim().toLowerCase()
              )
            );

            if (!empCodeKey) return;

            const rawEmpCode = row[empCodeKey];
            if (rawEmpCode === undefined || rawEmpCode === null) return;
            const empCodeStr = String(rawEmpCode).trim();

            const matchedPost = postByEmpCodeMap.get(empCodeStr);
            if (matchedPost) {
              const targetIndex = nextPosts.findIndex((p) => p.id === matchedPost.id);
              if (targetIndex === -1) return;

              let isRowChanged = false;

              if (officePhoneKey && row[officePhoneKey] !== undefined) {
                const newPhone = String(row[officePhoneKey] ?? "").trim();
                if (nextPosts[targetIndex].officePhone !== newPhone) {
                  nextPosts[targetIndex].officePhone = newPhone;
                  isRowChanged = true;
                }
              }

              if (orgMobileKey && row[orgMobileKey] !== undefined) {
                const newMobile = String(row[orgMobileKey] ?? "").trim();
                if (nextPosts[targetIndex].orgMobile !== newMobile) {
                  nextPosts[targetIndex].orgMobile = newMobile;
                  isRowChanged = true;
                }
              }

              if (isRowChanged) {
                updatedCount++;
                newModifiedIds.add(matchedPost.id);
              }
            }
          });

          setModifiedIds(newModifiedIds);

          if (updatedCount > 0) {
            setSuccessMessage(`اطلاعات ${updatedCount} پست با موفقیت از فایل اکسل اعمال شد.`);
            setTimeout(() => setSuccessMessage(null), 4000);
          } else {
            alert("هیچ رکوردی تغییر نکرد یا کد پرسنلی منطبقی پیدا نشد.");
          }

          return nextPosts;
        });
      } catch (err: any) {
        setError("خطا در پردازش فایل اکسل: " + (err?.message || "فرمت فایل نامعتبر است"));
      } finally {
        e.target.value = "";
      }
    };

    reader.readAsArrayBuffer(file);
  };

  // --- 3. ساختار درخت و فیلتر هوشمند ---
  const { flattenedTree, postsMap } = useMemo(() => {
    const map = new Map<string, PostInfoView>();
    const childrenMap = new Map<string | null, PostInfoView[]>();

    posts.forEach((p) => map.set(p.id, p));

    posts.forEach((p) => {
      const parentId = p.fkParentId && map.has(p.fkParentId) ? p.fkParentId : null;
      if (!childrenMap.has(parentId)) {
        childrenMap.set(parentId, []);
      }
      childrenMap.get(parentId)!.push(p);
    });

    const normalizedGlobal = globalSearch.trim().toLowerCase();
    const flattened: FlattenedNode[] = [];

    const traverse = (parentId: string | null, depth: number) => {
      const children = childrenMap.get(parentId) || [];

      for (const child of children) {
        const childChildren = childrenMap.get(child.id) || [];
        const hasChildren = childChildren.length > 0;
        const isExpanded = expandedIds.has(child.id);
        const isModified = modifiedIds.has(child.id);

        const initChild = initialPostsMap.get(child.id);

        const fullJobTitle = `${child.jobTitleName || ""} ${child.postCode ? `(${child.postCode})` : ""}`;
        const occupantName = `${child.firstName || ""} ${child.lastName || ""} ${child.employmentCode || ""}`;
        const levelGrade = `${child.jobLevelTitle || ""} ${child.gradeTitle || ""}`;

        const initFullJobTitle = initChild
          ? `${initChild.jobTitleName || ""} ${initChild.postCode ? `(${initChild.postCode})` : ""}`
          : fullJobTitle;
        const initOccupantName = initChild
          ? `${initChild.firstName || ""} ${initChild.lastName || ""} ${initChild.employmentCode || ""}`
          : occupantName;
        const initLevelGrade = initChild
          ? `${initChild.jobLevelTitle || ""} ${initChild.gradeTitle || ""}`
          : levelGrade;

        const matchesGlobal =
          !normalizedGlobal ||
          fullJobTitle.toLowerCase().includes(normalizedGlobal) ||
          (child.organizationUnitsName || "").toLowerCase().includes(normalizedGlobal) ||
          occupantName.toLowerCase().includes(normalizedGlobal) ||
          (child.officePhone || "").toLowerCase().includes(normalizedGlobal) ||
          (child.orgMobile || "").toLowerCase().includes(normalizedGlobal) ||
          (initChild &&
            (initFullJobTitle.toLowerCase().includes(normalizedGlobal) ||
              (initChild.organizationUnitsName || "").toLowerCase().includes(normalizedGlobal) ||
              initOccupantName.toLowerCase().includes(normalizedGlobal) ||
              (initChild.officePhone || "").toLowerCase().includes(normalizedGlobal) ||
              (initChild.orgMobile || "").toLowerCase().includes(normalizedGlobal)));

        let matchesColumns = true;
        for (const [col, term] of Object.entries(columnSearch)) {
          if (!term.trim()) continue;
          const q = term.toLowerCase();

          if (col === "jobTitle") {
            const matchCur = fullJobTitle.toLowerCase().includes(q);
            const matchInit = initFullJobTitle.toLowerCase().includes(q);
            if (!matchCur && !matchInit) matchesColumns = false;
          }
          if (col === "unit") {
            const matchCur = (child.organizationUnitsName || "").toLowerCase().includes(q);
            const matchInit = (initChild?.organizationUnitsName || "").toLowerCase().includes(q);
            if (!matchCur && !matchInit) matchesColumns = false;
          }
          if (col === "occupant") {
            const matchCur = occupantName.toLowerCase().includes(q);
            const matchInit = initOccupantName.toLowerCase().includes(q);
            if (!matchCur && !matchInit) matchesColumns = false;
          }
          if (col === "officePhone") {
            const matchCur = (child.officePhone || "").toLowerCase().includes(q);
            const matchInit = (initChild?.officePhone || "").toLowerCase().includes(q);
            if (!matchCur && !matchInit) matchesColumns = false;
          }
          if (col === "orgMobile") {
            const matchCur = (child.orgMobile || "").toLowerCase().includes(q);
            const matchInit = (initChild?.orgMobile || "").toLowerCase().includes(q);
            if (!matchCur && !matchInit) matchesColumns = false;
          }
          if (col === "levelGrade") {
            const matchCur = levelGrade.toLowerCase().includes(q);
            const matchInit = initLevelGrade.toLowerCase().includes(q);
            if (!matchCur && !matchInit) matchesColumns = false;
          }
        }

        const isSearching =
          normalizedGlobal !== "" || Object.values(columnSearch).some((v) => v.trim() !== "");

        if (matchesGlobal && matchesColumns) {
          flattened.push({
            node: child,
            depth,
            hasChildren,
            isExpanded,
            isModified,
          });
        }

        if ((isExpanded || isSearching) && hasChildren) {
          traverse(child.id, depth + 1);
        }
      }
    };

    traverse(null, 0);

    return { flattenedTree: flattened, postsMap: map };
  }, [posts, expandedIds, globalSearch, columnSearch, modifiedIds, initialPostsMap]);

  // --- 4. مدیریت انتخاب چندگانه (Ctrl / Shift) ---
  const handleRowClick = (e: React.MouseEvent, id: string) => {
    // جلوگیری از تغییر انتخاب هنگام فوکوس یا تایپ در اینپوت‌ها و دکمه‌ها
    const targetTag = (e.target as HTMLElement).tagName;
    if (targetTag === "INPUT" || targetTag === "BUTTON") return;

    if (e.ctrlKey || e.metaKey) {
      // 1. کلیک با کنترل (Toggle)
      setSelectedIds((prev) => {
        const next = new Set(prev);
        if (next.has(id)) next.delete(id);
        else next.add(id);
        return next;
      });
      setLastSelectedId(id);
    } else if (e.shiftKey && lastSelectedId) {
      // 2. کلیک با شیفت (Range Selection)
      const flatIds = flattenedTree.map((item) => item.node.id);
      const lastIndex = flatIds.indexOf(lastSelectedId);
      const currentIndex = flatIds.indexOf(id);

      if (lastIndex !== -1 && currentIndex !== -1) {
        const start = Math.min(lastIndex, currentIndex);
        const end = Math.max(lastIndex, currentIndex);
        const rangeIds = flatIds.slice(start, end + 1);

        setSelectedIds((prev) => {
          const next = new Set(prev);
          rangeIds.forEach((rId) => next.add(rId));
          return next;
        });
      }
    } else {
      // 3. کلیک معمولی (تک انتخابی)
      setSelectedIds(new Set([id]));
      setLastSelectedId(id);
    }
  };

  // --- 5. متدهای مدیریت درخت ---
  const toggleExpand = (id: string) => {
    setExpandedIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  };

  const expandAll = () => {
    const allParentIds = new Set<string>();
    posts.forEach((p) => {
      if (posts.some((child) => child.fkParentId === p.id)) {
        allParentIds.add(p.id);
      }
    });
    setExpandedIds(allParentIds);
  };

  const collapseAll = () => {
    setExpandedIds(new Set());
  };

  // --- 6. منطق Drag and Drop گروهی ---
  const isDescendant = (targetId: string, ancestorId: string): boolean => {
    let currentId: string | null | undefined = targetId;
    while (currentId) {
      if (currentId === ancestorId) return true;
      const node = postsMap.get(currentId);
      currentId = node?.fkParentId;
    }
    return false;
  };

  const handleDragStart = (e: React.DragEvent, id: string) => {
    let idsToMove: string[];

    // اگر آیتمی که درگ می‌شود خودش جزو گزینه‌های انتخاب‌شده باشد، همه انتخاب‌شده‌ها منتقل می‌شوند
    if (selectedIds.has(id)) {
      idsToMove = Array.from(selectedIds);
    } else {
      // اگر روی آیتم غیرانتخابی درگ شروع شد، انتخاب‌ها به همان تک آیتم تغییر می‌یابد
      idsToMove = [id];
      setSelectedIds(new Set([id]));
      setLastSelectedId(id);
    }

    e.dataTransfer.setData("text/plain", JSON.stringify(idsToMove));
    e.dataTransfer.effectAllowed = "move";
    setDraggedIds(idsToMove);
  };

  const handleDragOverRow = (e: React.DragEvent, targetId: string) => {
    e.preventDefault();
    const currentDragged = draggedIdsRef.current;
    if (!currentDragged.length || currentDragged.includes(targetId)) return;

    // اگر مقصد یکی از فرزندان هرکدام از ردیف‌های درگ‌شده باشد، درگ غیرمجاز است
    const isInvalid = currentDragged.some((dId) => isDescendant(targetId, dId));
    if (isInvalid) {
      e.dataTransfer.dropEffect = "none";
      return;
    }

    e.dataTransfer.dropEffect = "move";
    if (dragOverId !== targetId) {
      setDragOverId(targetId);
    }
  };

  const handleDropOnRow = (e: React.DragEvent, targetParentId: string) => {
    e.preventDefault();
    setDragOverId(null);
    setIsOverRootZone(false);

    let idsToMove: string[] = [];
    try {
      const rawData = e.dataTransfer.getData("text/plain");
      idsToMove = JSON.parse(rawData);
    } catch {
      idsToMove = draggedIds;
    }

    if (!idsToMove.length) return;

    // فیلتر ردیف‌های نامعتبر (انتقال والد به زیرمجموعه یا انتقال به والد فعلی)
    let hasCyclicError = false;
    const validIdsToMove = idsToMove.filter((id) => {
      if (id === targetParentId) return false;
      if (isDescendant(targetParentId, id)) {
        hasCyclicError = true;
        return false;
      }
      const node = postsMap.get(id);
      if (!node || node.fkParentId === targetParentId) return false;
      return true;
    });

    if (hasCyclicError) {
      alert("امکان انتقال والد به زیرمجموعه‌های خودش وجود ندارد!");
    }

    if (validIdsToMove.length > 0) {
      updateNodesParent(validIdsToMove, targetParentId);
    }

    setDraggedIds([]);
  };

  const handleDropOnRoot = (e: React.DragEvent) => {
    e.preventDefault();
    setIsOverRootZone(false);
    setDragOverId(null);

    let idsToMove: string[] = [];
    try {
      const rawData = e.dataTransfer.getData("text/plain");
      idsToMove = JSON.parse(rawData);
    } catch {
      idsToMove = draggedIds;
    }

    if (!idsToMove.length) return;

    const validIdsToMove = idsToMove.filter((id) => {
      const node = postsMap.get(id);
      return node && node.fkParentId !== null;
    });

    if (validIdsToMove.length > 0) {
      updateNodesParent(validIdsToMove, null);
    }

    setDraggedIds([]);
  };

  const updateNodesParent = (nodeIds: string[], newParentId: string | null) => {
    const idSet = new Set(nodeIds);

    setPosts((prev) =>
      prev.map((item) => {
        if (idSet.has(item.id)) {
          return { ...item, fkParentId: newParentId };
        }
        return item;
      })
    );

    setModifiedIds((prev) => {
      const next = new Set(prev);
      nodeIds.forEach((id) => next.add(id));
      return next;
    });

    if (newParentId) {
      setExpandedIds((prev) => new Set(prev).add(newParentId));
    }
  };

  // --- 7. بازنشانی و ذخیره تغییرات ---
  const handleResetChanges = () => {
    if (window.confirm("آیا از لغو تمام تغییرات اعمال شده اطمینان دارید؟")) {
      setPosts(JSON.parse(JSON.stringify(initialPosts)));
      setModifiedIds(new Set());
      setSelectedIds(new Set());
    }
  };

  const handleSaveChanges = async () => {
    if (modifiedIds.size === 0) return;

    try {
      setSaving(true);
      setError(null);
      setSuccessMessage(null);

      const commands: UpdatePostCommand[] = Array.from(modifiedIds).map((id) => {
        const post = postsMap.get(id)!;
        return {
          id: post.id,
          code: post.postCode,
          organizationUnitId: post.fkOrganizationUnitId,
          jobTitleId: post.fkJobTitleId,
          jobLevelId: post.fkJobLevelId,
          gradeId: post.fkGradeId,
          costCenterId: post.fkCostCenterId,
          reportsToPostId: post.fkParentId,
          officePhone: post.officePhone,
          orgEmail: post.orgEmail,
          orgMobile: post.orgMobile,
          assignType: post.assignmentsAssigneeType,
          isActive: true,
        };
      });

      await postApi.batchUpdatePosts(commands);

      setSuccessMessage(`تعداد ${commands.length} تغییر با موفقیت ذخیره شد.`);
      setInitialPosts(JSON.parse(JSON.stringify(posts)));
      setModifiedIds(new Set());

      setTimeout(() => setSuccessMessage(null), 4000);
    } catch (err: any) {
      setError(err?.message || "خطا در ذخیره تغییرات چارت");
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[400px] text-gray-500 font-sans">
        در حال دریافت اطلاعات چارت سازمانی...
      </div>
    );
  }

  return (
    <div className="p-6 dir-rtl text-right font-sans bg-gray-50/50 min-h-screen select-none">
      {/* هدر اصلی */}
      <div className="bg-white p-5 rounded-xl border border-gray-200 shadow-sm mb-5 select-text">
        <div className="flex flex-wrap items-center justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold text-gray-800 mb-1">مدیریت ساختار چارت سازمانی</h1>
            <p className="text-sm text-gray-500">
              کل پست‌ها: <span className="font-semibold text-gray-700">{posts.length}</span>
              {selectedIds.size > 0 && (
                <span className="mr-3 text-blue-600 bg-blue-50 px-2 py-0.5 rounded border border-blue-200 text-xs font-medium">
                  {selectedIds.size} رکورد انتخاب شده
                </span>
              )}
              {modifiedIds.size > 0 && (
                <span className="mr-3 text-amber-600 bg-amber-50 px-2 py-0.5 rounded border border-amber-200 text-xs font-medium">
                  {modifiedIds.size} تغییر ذخیره‌نشده
                </span>
              )}
            </p>
          </div>

          <div className="flex items-center gap-3">
            <input
              type="file"
              ref={fileInputRef}
              onChange={handleExcelImport}
              accept=".xlsx, .xls"
              className="hidden"
            />

            <button
              type="button"
              onClick={() => fileInputRef.current?.click()}
              disabled={saving}
              className="px-4 py-2 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg text-sm font-medium transition-colors disabled:opacity-50 flex items-center gap-2 cursor-pointer shadow-sm"
              title="بارگذاری اکسل جهت به‌روزرسانی تلفن داخلی و موبایل سازمانی"
            >
              📊 بارگذاری از اکسل
            </button>

            {modifiedIds.size > 0 && (
              <button
                onClick={handleResetChanges}
                disabled={saving}
                className="px-4 py-2 border border-gray-300 text-gray-700 rounded-lg text-sm hover:bg-gray-100 transition-colors disabled:opacity-50"
              >
                انصراف و بازنشانی
              </button>
            )}

            <button
              onClick={handleSaveChanges}
              disabled={modifiedIds.size === 0 || saving}
              className={`px-5 py-2 rounded-lg text-sm font-medium shadow-sm transition-all flex items-center gap-2 ${
                modifiedIds.size > 0
                  ? "bg-blue-600 hover:bg-blue-700 text-white cursor-pointer"
                  : "bg-gray-200 text-gray-400 cursor-not-allowed"
              }`}
            >
              {saving ? "در حال ذخیره..." : "ذخیره تغییرات"}
            </button>
          </div>
        </div>

        {error && (
          <div className="mt-4 p-3 bg-red-50 border border-red-200 text-red-700 text-sm rounded-lg">
            {error}
          </div>
        )}
        {successMessage && (
          <div className="mt-4 p-3 bg-green-50 border border-green-200 text-green-700 text-sm rounded-lg">
            {successMessage}
          </div>
        )}

        {/* سرچ اصلی و کنترل‌های درخت */}
        <div className="flex flex-wrap items-center justify-between gap-4 mt-5 pt-4 border-t border-gray-100">
          <div className="w-72">
            <input
              type="text"
              placeholder="جستجوی کلی در تمام فیلدها..."
              value={globalSearch}
              onChange={(e) => setGlobalSearch(e.target.value)}
              className="w-full px-3 py-1.5 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 outline-none"
            />
          </div>

          <div className="flex items-center gap-2 text-xs text-gray-500">
            <span>💡 راهنما: برای انتخاب چندگانه از کلیدهای Ctrl و Shift استفاده کنید.</span>
            <button
              onClick={expandAll}
              className="px-3 py-1.5 text-xs text-gray-600 bg-gray-100 hover:bg-gray-200 rounded border border-gray-300 mr-2"
            >
              گسترش همه
            </button>
            <button
              onClick={collapseAll}
              className="px-3 py-1.5 text-xs text-gray-600 bg-gray-100 hover:bg-gray-200 rounded border border-gray-300"
            >
              جمع‌کردن همه
            </button>
          </div>
        </div>
      </div>

      {/* منطقه رهاسازی ریشه */}
      <div
        onDragOver={(e) => {
          e.preventDefault();
          if (draggedIds.length > 0) setIsOverRootZone(true);
        }}
        onDragLeave={() => setIsOverRootZone(false)}
        onDrop={handleDropOnRoot}
        className={`sticky top-0 z-30 mb-2 h-[34px] px-3 border border-dashed rounded-lg text-center text-xs transition-all shadow-sm backdrop-blur-md flex items-center justify-center ${
          isOverRootZone
            ? "border-blue-500 bg-blue-100/95 text-blue-800 font-bold scale-[1.005]"
            : "border-gray-300 bg-white/95 text-gray-600 hover:border-gray-400"
        }`}
      >
        📌 جهت انتقال موارد انتخاب‌شده به بالاترین سطح چارت (بدون والد)، آن‌ها را اینجا رها کنید.
      </div>

      {/* جدول چارت */}
      <div className="bg-white rounded-xl border border-gray-200 shadow-sm">
        <table className="w-full text-right border-collapse">
          <thead>
            {/* ردیف اول: عناوین ستون‌ها */}
            <tr className="border-b border-gray-200 text-gray-700 text-xs font-semibold">
              <th className="sticky top-[34px] z-20 bg-gray-100 py-2 px-3 w-10 text-center border-b border-gray-200 shadow-sm">
                جابه‌جایی
              </th>
              <th className="sticky top-[34px] z-20 bg-gray-100 py-2 px-4 border-b border-gray-200 shadow-sm">
                عنوان شغل (کد پست)
              </th>
              <th className="sticky top-[34px] z-20 bg-gray-100 py-2 px-4 border-b border-gray-200 shadow-sm">
                واحد سازمانی
              </th>
              <th className="sticky top-[34px] z-20 bg-gray-100 py-2 px-4 border-b border-gray-200 shadow-sm">
                شاغل فعلی
              </th>
              <th className="sticky top-[34px] z-20 bg-gray-100 py-2 px-4 w-36 border-b border-gray-200 shadow-sm">
                تلفن داخلی
              </th>
              <th className="sticky top-[34px] z-20 bg-gray-100 py-2 px-4 w-40 border-b border-gray-200 shadow-sm">
                موبایل سازمانی
              </th>
              <th className="sticky top-[34px] z-20 bg-gray-100 py-2 px-4 border-b border-gray-200 shadow-sm">
                رده / سطح شغلی
              </th>
              <th className="sticky top-[34px] z-20 bg-gray-100 py-2 px-4 text-center w-24 border-b border-gray-200 shadow-sm">
                وضعیت
              </th>
            </tr>

            {/* ردیف دوم: اینپوت‌های سرچ */}
            <tr className="border-b border-gray-200">
              <th className="sticky top-[70px] z-20 bg-gray-50 py-1.5 px-2 border-b border-gray-200 shadow-sm"></th>
              <th className="sticky top-[70px] z-20 bg-gray-50 py-1.5 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ شغل / کد..."
                  value={columnSearch["jobTitle"] || ""}
                  onChange={(e) => handleColumnSearch("jobTitle", e.target.value)}
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                />
              </th>
              <th className="sticky top-[70px] z-20 bg-gray-50 py-1.5 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ واحد..."
                  value={columnSearch["unit"] || ""}
                  onChange={(e) => handleColumnSearch("unit", e.target.value)}
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                />
              </th>
              <th className="sticky top-[70px] z-20 bg-gray-50 py-1.5 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ شاغل..."
                  value={columnSearch["occupant"] || ""}
                  onChange={(e) => handleColumnSearch("occupant", e.target.value)}
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                />
              </th>
              <th className="sticky top-[70px] z-20 bg-gray-50 py-1.5 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ داخلی..."
                  value={columnSearch["officePhone"] || ""}
                  onChange={(e) => handleColumnSearch("officePhone", e.target.value)}
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                />
              </th>
              <th className="sticky top-[70px] z-20 bg-gray-50 py-1.5 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ موبایل..."
                  value={columnSearch["orgMobile"] || ""}
                  onChange={(e) => handleColumnSearch("orgMobile", e.target.value)}
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                />
              </th>
              <th className="sticky top-[70px] z-20 bg-gray-50 py-1.5 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ رده..."
                  value={columnSearch["levelGrade"] || ""}
                  onChange={(e) => handleColumnSearch("levelGrade", e.target.value)}
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                />
              </th>
              <th className="sticky top-[70px] z-20 bg-gray-50 py-1.5 px-2 border-b border-gray-200 shadow-sm"></th>
            </tr>
          </thead>

          <tbody className="divide-y divide-gray-100 text-sm">
            {flattenedTree.length === 0 ? (
              <tr>
                <td colSpan={8} className="text-center py-12 text-gray-400">
                  هیچ پستی یافت نشد.
                </td>
              </tr>
            ) : (
              flattenedTree.map(({ node, depth, hasChildren, isExpanded, isModified }) => {
                const isSelected = selectedIds.has(node.id);
                const isBeingDragged = draggedIds.includes(node.id);
                const isTarget = dragOverId === node.id;
                const occupantName =
                  node.firstName || node.lastName
                    ? `${node.firstName || ""} ${node.lastName || ""}`
                    : "-";

                return (
                  <tr
                    key={node.id}
                    onClick={(e) => handleRowClick(e, node.id)}
                    onDragOver={(e) => handleDragOverRow(e, node.id)}
                    onDragLeave={() => dragOverId === node.id && setDragOverId(null)}
                    onDrop={(e) => handleDropOnRow(e, node.id)}
                    className={`transition-colors cursor-pointer ${
                      isSelected ? "bg-blue-100/70 border-blue-300 font-medium" : ""
                    } ${isBeingDragged ? "opacity-30 bg-gray-200" : ""} ${
                      isTarget ? "bg-blue-200 border-y-2 border-blue-600" : "hover:bg-gray-50/80"
                    } ${isModified && !isSelected ? "bg-amber-50/40" : ""}`}
                  >
                    <td className="py-3 px-2 text-center align-middle">
                      <div
                        draggable
                        onDragStart={(e) => handleDragStart(e, node.id)}
                        className="cursor-grab active:cursor-grabbing text-gray-400 hover:text-gray-700 text-lg leading-none inline-block p-1"
                        title="جهت تغییر والد، کشیده و رها کنید (امکان انتخاب گروهی)"
                      >
                        ☰
                      </div>
                    </td>

                    <td className="py-3 px-4 font-medium text-gray-800">
                      <div
                        className="flex items-center gap-2"
                        style={{ paddingRight: `${depth * 24}px` }}
                      >
                        {hasChildren ? (
                          <button
                            type="button"
                            onClick={(e) => {
                              e.stopPropagation();
                              toggleExpand(node.id);
                            }}
                            className="w-5 h-5 flex items-center justify-center rounded text-gray-500 hover:bg-gray-200 text-xs"
                          >
                            {isExpanded ? "▼" : "◀"}
                          </button>
                        ) : (
                          <span className="w-5 text-center text-gray-300">•</span>
                        )}
                        <span>
                          {node.jobTitleName || "بدون عنوان شغل"}{" "}
                          {node.postCode && (
                            <span className="text-gray-500 text-xs font-mono font-normal">
                              ({node.postCode})
                            </span>
                          )}
                        </span>
                      </div>
                    </td>

                    <td className="py-3 px-4 text-gray-600 text-xs">
                      {node.organizationUnitsName || "-"}
                    </td>

                    <td className="py-3 px-4 text-gray-700 text-xs">
                      <div className="flex flex-col">
                        <span className="font-medium">{occupantName}</span>
                        {node.employmentCode && (
                          <span className="text-[10px] text-gray-400 font-mono">
                            کد: {node.employmentCode}
                          </span>
                        )}
                      </div>
                    </td>

                    <td className="py-2 px-3">
                      <input
                        type="text"
                        value={node.officePhone || ""}
                        onChange={(e) => handleFieldChange(node.id, "officePhone", e.target.value)}
                        placeholder="داخلی..."
                        className="w-full px-2 py-1 text-xs border border-gray-300 rounded focus:ring-1 focus:ring-blue-500 font-mono text-center dir-ltr outline-none bg-white hover:border-gray-400 transition-colors"
                      />
                    </td>

                    <td className="py-2 px-3">
                      <input
                        type="text"
                        value={node.orgMobile || ""}
                        onChange={(e) => handleFieldChange(node.id, "orgMobile", e.target.value)}
                        placeholder="موبایل..."
                        className="w-full px-2 py-1 text-xs border border-gray-300 rounded focus:ring-1 focus:ring-blue-500 font-mono text-center dir-ltr outline-none bg-white hover:border-gray-400 transition-colors"
                      />
                    </td>

                    <td className="py-3 px-4 text-gray-500 text-xs">
                      {node.jobLevelTitle || node.gradeTitle ? (
                        <span>
                          {node.jobLevelTitle || ""} {node.gradeTitle ? `(${node.gradeTitle})` : ""}
                        </span>
                      ) : (
                        "-"
                      )}
                    </td>

                    <td className="py-3 px-4 text-center">
                      {isModified ? (
                        <span className="inline-block text-[10px] bg-amber-100 text-amber-800 border border-amber-300 px-2 py-0.5 rounded-full font-medium">
                          تغییر یافته
                        </span>
                      ) : (
                        <span className="text-gray-300 text-xs">-</span>
                      )}
                    </td>
                  </tr>
                );
              })
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default PostManagementPage;