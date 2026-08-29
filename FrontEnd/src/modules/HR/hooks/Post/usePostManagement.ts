// src/modules/HR/hooks/Post/usePostManagement.ts

import React, { useEffect, useState, useMemo, useRef } from "react";
import * as XLSX from "xlsx";
import { postApi } from "../../api/PostApi";
import { PostInfoDto } from "../../models/postInfoDto";
import { UpdatePostCommand } from "../../models/postCommand";
import { SelectionListDto } from "@/core/models/SelectionListDto";
import { locationApi } from "../../api/LocationApi";
import { employmentApi } from "../../api/EmploymentApi";

interface FlattenedNode {
  node: PostInfoDto;
  depth: number;
  hasChildren: boolean;
  isExpanded: boolean;
  isModified: boolean;
}

// ✅ فیلدهای قابل ویرایش - اضافه کردن "locations"
type EditableField =
  | "employmentId"
  | "fkJobTitleId"
  | "fkOrganizationUnitId"
  | "fkJobLevelId"
  | "fkGradeId"
  | "locations"; // ← اینجا رو به "locations" تغییر دادم

export const usePostManagement = () => {
  // --- State‌ها ---
  const [posts, setPosts] = useState<PostInfoDto[]>([]);
  const [initialPosts, setInitialPosts] = useState<PostInfoDto[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [saving, setSaving] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
// --- استیت‌های مودال حذف ---
    const [deleteTarget, setDeleteTarget] = useState<{ id: string; title: string; isModified: boolean } | null>(null);
    const [isDeleting, setIsDeleting] = useState<boolean>(false);
  
  const [locations, setLocations] = useState<SelectionListDto[]>([]);
  const [employments, setEmployments] = useState<SelectionListDto[]>([]);
  const [jobTitles, setJobTitles] = useState<SelectionListDto[]>([]);
  const [orgUnits, setOrgUnits] = useState<SelectionListDto[]>([]);
  const [jobLevels, setJobLevels] = useState<SelectionListDto[]>([]);
  const [grades, setGrades] = useState<SelectionListDto[]>([]);

  // مپ‌ها با ایمن‌سازی در هنگام استفاده
  const locationMap = useMemo(() => new Map(locations.map(l => [l.value, l.display || l.label])), [locations]);
  const employmentMap = useMemo(() => new Map(employments.map(e => [e.value, e.display || e.label])), [employments]);
  const jobTitleMap = useMemo(() => new Map(jobTitles.map(j => [j.value, j.display || j.label])), [jobTitles]);
  const orgUnitMap = useMemo(() => new Map(orgUnits.map(o => [o.value, o.display || o.label])), [orgUnits]);
  const jobLevelMap = useMemo(() => new Map(jobLevels.map(l => [l.value, l.display || l.label])), [jobLevels]);
  const gradeMap = useMemo(() => new Map(grades.map(g => [g.value, g.display || g.label])), [grades]);

  // ... سایر state‌ها (جستجو، انتخاب، درگ) بدون تغییر ...

  const [globalSearch, setGlobalSearch] = useState<string>("");
  const [columnSearch, setColumnSearch] = useState<Record<string, string>>({});
  const [expandedIds, setExpandedIds] = useState<Set<string>>(new Set());
  const [modifiedIds, setModifiedIds] = useState<Set<string>>(new Set());
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [lastSelectedId, setLastSelectedId] = useState<string | null>(null);
  const [draggedIds, setDraggedIds] = useState<string[]>([]);
  const [dragOverId, setDragOverId] = useState<string | null>(null);
  const [isOverRootZone, setIsOverRootZone] = useState<boolean>(false);

  const fileInputRef = useRef<HTMLInputElement | null>(null);
  const draggedIdsRef = useRef<string[]>([]);
  draggedIdsRef.current = draggedIds;

  const initialPostsMap = useMemo(() => {
    const map = new Map<string, PostInfoDto>();
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

      const [
        locList,
        empList,
        jobTitleList,
        orgUnitList,
        jobLevelList,
        gradeList,
        data
      ] = await Promise.all([
        locationApi.getSelectionList(),
        employmentApi.getSelectionList(),
        postApi.GetJobTitleSelectionList(),
        postApi.GetOrganizationUnitSelectionList(),
        postApi.GetJobLevelSelectionList(),
        postApi.GetGradeSelectionList(),
        postApi.gtList() // ← فرض می‌کنیم این متد الان PostInfoDto[] برمی‌گرداند
      ]);

      // ✅ cast امن با as unknown
       const list = (data || []) as unknown as PostInfoDto[];
    //  const list = Array.isArray(data) ? data : [];
      setPosts(list);
      setInitialPosts(JSON.parse(JSON.stringify(list)));
      setLocations(locList || []);
      setEmployments(empList || []);
      setJobTitles(jobTitleList || []);
      setOrgUnits(orgUnitList || []);
      setJobLevels(jobLevelList || []);
      setGrades(gradeList || []);

      const parentIds = new Set<string>();
      list.forEach((p) => {
        if (p.fkParentId) parentIds.add(p.fkParentId);
      });
      setExpandedIds(parentIds);
      setModifiedIds(new Set());
      setSelectedIds(new Set());
      setLastSelectedId(null);
    } catch (err: any) {
      setError(err?.message || "خطا در دریافت اطلاعات");
    } finally {
      setLoading(false);
    }
  };

  // --- 2. ویرایش درجا ---
  const handleFieldChange = (id: string, field: EditableField, value: string | string[]) => {
    setPosts((prev) =>
      prev.map((item) => {
        if (item.id === id) {
          if (field === "locations" && Array.isArray(value)) {
            // value آرایه‌ای از شناسه‌های لوکیشن است
            const newLocations = value.map(locId => {
              const found = locations.find(l => l.value === locId);
              return { id: locId, title: found?.display || found?.label || locId };
            });
            return { ...item, locations: newLocations };
          }
          // سایر فیلدها
          return { ...item, [field]: value };
        }
        return item;
      })
    );
    setModifiedIds((prev) => new Set(prev).add(id));
  };

  // --- 3. جستجو ---
  const handleGlobalSearch = (value: string) => setGlobalSearch(value);
  const handleColumnSearch = (column: string, value: string) =>
    setColumnSearch((prev) => ({ ...prev, [column]: value }));

  // --- 4. ساختار درخت و فیلتر ---
  const { flattenedTree, postsMap } = useMemo(() => {
    const map = new Map<string, PostInfoDto>();
    const childrenMap = new Map<string | null, PostInfoDto[]>();

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

        // ✅ ایمن‌سازی با ?? ""
        const jobTitleDisplay = jobTitleMap.get(child.fkJobTitleId) ?? "";
        const orgUnitDisplay = orgUnitMap.get(child.fkOrganizationUnitId ?? "") ?? "";
        const jobLevelDisplay = jobLevelMap.get(child.fkJobLevelId ?? "") ?? "";
        const gradeDisplay = gradeMap.get(child.fkGradeId ?? "") ?? "";
        const locationNames = child.locations?.map(loc => locationMap.get(loc.id) ?? loc.title ?? "").filter(Boolean).join("، ") ?? "";
        const occupantName = `${child.firstName || ""} ${child.lastName || ""} ${child.employmentCode || ""}`.trim();

        const initJobTitleDisplay = initChild ? (jobTitleMap.get(initChild.fkJobTitleId) ?? "") : jobTitleDisplay;
        const initOrgUnitDisplay = initChild ? (orgUnitMap.get(initChild.fkOrganizationUnitId ?? "") ?? "") : orgUnitDisplay;
        const initJobLevelDisplay = initChild ? (jobLevelMap.get(initChild.fkJobLevelId ?? "") ?? "") : jobLevelDisplay;
        const initGradeDisplay = initChild ? (gradeMap.get(initChild.fkGradeId ?? "") ?? "") : gradeDisplay;
        const initLocationNames = initChild
          ? (initChild.locations?.map(loc => locationMap.get(loc.id) ?? loc.title ?? "").filter(Boolean).join("، ") ?? "")
          : locationNames;
        const initOccupantName = initChild
          ? `${initChild.firstName || ""} ${initChild.lastName || ""} ${initChild.employmentCode || ""}`.trim()
          : occupantName;

        // جستجوی سراسری
        const matchesGlobal =
          !normalizedGlobal ||
          jobTitleDisplay.toLowerCase().includes(normalizedGlobal) ||
          orgUnitDisplay.toLowerCase().includes(normalizedGlobal) ||
          occupantName.toLowerCase().includes(normalizedGlobal) ||
          locationNames.toLowerCase().includes(normalizedGlobal) ||
          `${jobLevelDisplay} ${gradeDisplay}`.toLowerCase().includes(normalizedGlobal) ||
          (initChild &&
            (initJobTitleDisplay.toLowerCase().includes(normalizedGlobal) ||
              initOrgUnitDisplay.toLowerCase().includes(normalizedGlobal) ||
              initOccupantName.toLowerCase().includes(normalizedGlobal) ||
              initLocationNames.toLowerCase().includes(normalizedGlobal) ||
              `${initJobLevelDisplay} ${initGradeDisplay}`.toLowerCase().includes(normalizedGlobal)));

        // جستجوی ستونی
        let matchesColumns = true;
        for (const [col, term] of Object.entries(columnSearch)) {
          if (!term.trim()) continue;
          const q = term.toLowerCase();

          if (col === "jobTitle") {
            const matchCur = jobTitleDisplay.toLowerCase().includes(q);
            const matchInit = initChild ? initJobTitleDisplay.toLowerCase().includes(q) : false;
            if (!matchCur && !matchInit) matchesColumns = false;
          }
          if (col === "unit") {
            const matchCur = orgUnitDisplay.toLowerCase().includes(q);
            const matchInit = initChild ? initOrgUnitDisplay.toLowerCase().includes(q) : false;
            if (!matchCur && !matchInit) matchesColumns = false;
          }
          if (col === "occupant") {
            const matchCur = occupantName.toLowerCase().includes(q);
            const matchInit = initChild ? initOccupantName.toLowerCase().includes(q) : false;
            if (!matchCur && !matchInit) matchesColumns = false;
          }
          if (col === "levelGrade") {
            const combined = `${jobLevelDisplay} ${gradeDisplay}`.toLowerCase();
            const initCombined = initChild ? `${initJobLevelDisplay} ${initGradeDisplay}`.toLowerCase() : "";
            if (!combined.includes(q) && !initCombined.includes(q)) matchesColumns = false;
          }
          if (col === "location") {
            const matchCur = locationNames.toLowerCase().includes(q);
            const matchInit = initChild ? initLocationNames.toLowerCase().includes(q) : false;
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
  }, [posts, expandedIds, globalSearch, columnSearch, modifiedIds, initialPostsMap, locationMap, jobTitleMap, orgUnitMap, jobLevelMap, gradeMap]);

  // --- 5. انتخاب چندگانه (بدون تغییر) ---
  const handleRowClick = (e: React.MouseEvent, id: string) => {
    const targetTag = (e.target as HTMLElement).tagName;
    if (targetTag === "INPUT" || targetTag === "BUTTON") return;

    if (e.ctrlKey || e.metaKey) {
      setSelectedIds((prev) => {
        const next = new Set(prev);
        if (next.has(id)) next.delete(id);
        else next.add(id);
        return next;
      });
      setLastSelectedId(id);
    } else if (e.shiftKey && lastSelectedId) {
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
      setSelectedIds(new Set([id]));
      setLastSelectedId(id);
    }
  };

  // --- 6. مدیریت درخت (بدون تغییر) ---
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
  const collapseAll = () => setExpandedIds(new Set());


  // --- 7. درگ اند دراپ ---
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
    if (selectedIds.has(id)) {
      idsToMove = Array.from(selectedIds);
    } else {
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
    const isInvalid = currentDragged.some((dId) => isDescendant(targetId, dId));
    if (isInvalid) {
      e.dataTransfer.dropEffect = "none";
      return;
    }
    e.dataTransfer.dropEffect = "move";
    if (dragOverId !== targetId) setDragOverId(targetId);
  };

  const handleDropOnRow = (e: React.DragEvent, targetParentId: string) => {
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

    if (hasCyclicError) alert("امکان انتقال والد به زیرمجموعه‌های خودش وجود ندارد!");
    if (validIdsToMove.length > 0) updateNodesParent(validIdsToMove, targetParentId);
    setDraggedIds([]);
  };

  const handleDropOnRoot = (e: React.DragEvent) => {
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

    const validIdsToMove = idsToMove.filter((id) => {
      const node = postsMap.get(id);
      return node && node.fkParentId !== null;
    });
    if (validIdsToMove.length > 0) updateNodesParent(validIdsToMove, null);
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

  // --- 8. اکسل (با اصلاح نام فیلد به "locations") ---
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
          alert("فایل اکسل خالی است یا فرمت معتبری ندارد.");
          return;
        }

        let updatedCount = 0;
        const newModifiedIds = new Set(modifiedIds);

        setPosts((prevPosts) => {
          const postByEmpCodeMap = new Map<string, PostInfoDto>();
          prevPosts.forEach((p) => {
            if (p.employmentCode) {
              postByEmpCodeMap.set(String(p.employmentCode).trim(), p);
            }
          });

          const nextPosts = prevPosts.map((p) => ({ ...p }));

          const columnMappings = [
            { key: "کد پرسنلی", field: "employmentCode" },
            { key: "کدپرسنلی", field: "employmentCode" },
            { key: "employmentcode", field: "employmentCode" },
            { key: "empcode", field: "employmentCode" },
            { key: "کد", field: "employmentCode" },
            { key: "عنوان شغل", field: "fkJobTitleId" },
            { key: "واحد سازمانی", field: "fkOrganizationUnitId" },
            { key: "سطح شغلی", field: "fkJobLevelId" },
            { key: "رده", field: "fkGradeId" },
            { key: "محل استقرار", field: "locations" }, // ← اینجا هم از "locations" استفاده شد
            { key: "مکان", field: "locations" },
            { key: "شاغل", field: "employmentId" },
          ];

          const findId = (list: SelectionListDto[], raw: string): string | null => {
            const trimmed = String(raw).trim();
            const found = list.find(
              (item) =>
                item.value.toLowerCase() === trimmed.toLowerCase() ||
                item.display?.toLowerCase() === trimmed.toLowerCase() ||
                item.label?.toLowerCase() === trimmed.toLowerCase()
            );
            return found ? found.value : null;
          };

          excelRows.forEach((row) => {
            const empCodeKey = Object.keys(row).find(
              (k) =>
                ["کد پرسنلی", "کدپرسنلی", "employmentcode", "empcode", "کد"].includes(
                  k.trim().toLowerCase()
                )
            );
            if (!empCodeKey) return;
            const rawEmpCode = row[empCodeKey];
            if (rawEmpCode === undefined || rawEmpCode === null) return;
            const empCodeStr = String(rawEmpCode).trim();
            const matchedPost = postByEmpCodeMap.get(empCodeStr);
            if (!matchedPost) return;

            const targetIndex = nextPosts.findIndex((p) => p.id === matchedPost.id);
            if (targetIndex === -1) return;

            let isRowChanged = false;

            const processColumn = (field: EditableField, possibleKeys: string[], convert?: (val: any) => any) => {
              const key = Object.keys(row).find(
                (k) => possibleKeys.some((pk) => k.trim().toLowerCase() === pk.toLowerCase())
              );
              if (!key) return;
              const raw = row[key];
              if (raw === undefined || raw === null) return;
              let newValue: any = String(raw).trim();

              if (field === "fkJobTitleId") {
                const id = findId(jobTitles, newValue);
                if (id && (nextPosts[targetIndex] as any)[field] !== id) {
                  (nextPosts[targetIndex] as any)[field] = id;
                  isRowChanged = true;
                }
              } else if (field === "fkOrganizationUnitId") {
                const id = findId(orgUnits, newValue);
                if (id && (nextPosts[targetIndex] as any)[field] !== id) {
                  (nextPosts[targetIndex] as any)[field] = id;
                  isRowChanged = true;
                }
              } else if (field === "fkJobLevelId") {
                const id = findId(jobLevels, newValue);
                if (id && (nextPosts[targetIndex] as any)[field] !== id) {
                  (nextPosts[targetIndex] as any)[field] = id;
                  isRowChanged = true;
                }
              } else if (field === "fkGradeId") {
                const id = findId(grades, newValue);
                if (id && (nextPosts[targetIndex] as any)[field] !== id) {
                  (nextPosts[targetIndex] as any)[field] = id;
                  isRowChanged = true;
                }
              } else if (field === "employmentId") {
                const id = findId(employments, newValue);
                if (id && (nextPosts[targetIndex] as any)[field] !== id) {
                  (nextPosts[targetIndex] as any)[field] = id;
                  isRowChanged = true;
                }
              } else if (field === "locations") {
                // چند لوکیشن
                const locIds = newValue.split(/[،,;؛]/).map((s: string) => s.trim()).filter(Boolean);
                const matchedLocIds = locIds.map((loc: string) => findId(locations, loc)).filter(Boolean) as string[];
                if (matchedLocIds.length > 0) {
                  const currentLocIds = (nextPosts[targetIndex].locations || []).map(l => l.id);
                  if (JSON.stringify(currentLocIds.sort()) !== JSON.stringify(matchedLocIds.sort())) {
                    (nextPosts[targetIndex] as any).locations = matchedLocIds.map(id => ({ id, title: locationMap.get(id) ?? id }));
                    isRowChanged = true;
                  }
                }
              }
            };

            processColumn("fkJobTitleId", ["عنوان شغل", "jobtitle"]);
            processColumn("fkOrganizationUnitId", ["واحد سازمانی", "organizationunit", "واحد"]);
            processColumn("fkJobLevelId", ["سطح شغلی", "joblevel", "سطح"]);
            processColumn("fkGradeId", ["رده", "grade", "درجه"]);
            processColumn("employmentId", ["شاغل", "employment", "شاغل فعلی"]);
            processColumn("locations", ["محل استقرار", "مکان", "location"]);

            if (isRowChanged) {
              updatedCount++;
              newModifiedIds.add(matchedPost.id);
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

  // --- 9. بازنشانی و ذخیره ---
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
          employmentId: post.employmentId,
          locationsId: post.locations?.map(l => l.id) || [],
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
// --- 6. مودال حذف  ---
    const handleOpenDeleteModal = (post: PostInfoDto) => {
      setDeleteTarget({
        id: post.id,
        title: `${jobTitleMap.get(post.fkJobTitleId?.trim())}` || "بدون عنوان",
        isModified: modifiedIds.has(post.id),
      });
    };
  
    const handleCloseDeleteModal = () => {
      if (isDeleting) return;
      setDeleteTarget(null);
    };
  
    const handleConfirmDelete = async () => {
      if (!deleteTarget) return;
  
      try {
        setIsDeleting(true);
        setError(null);
  
        await postApi.delete(deleteTarget.id);
  
        setSuccessMessage(`مکان "${deleteTarget.title}" با موفقیت حذف شد.`);
        setDeleteTarget(null);
  
        await loadData();
        setTimeout(() => setSuccessMessage(null), 4000);
      } catch (err: any) {
        setError(err?.message || "خطا در حذف مکان");
        setDeleteTarget(null);
      } finally {
        setIsDeleting(false);
      }
    };

  // --- خروجی ---
  return {
    posts,
    loading,
    initialPosts,
    locations,
    employments,
    jobTitles,
    orgUnits,
    jobLevels,
    grades,
    saving,
    draggedIds,
    dragOverId,
    isOverRootZone,
    error,
    successMessage,
    globalSearch,
    selectedIds,
    lastSelectedId,
    columnSearch,
    modifiedIds,
    fileInputRef,
    locationMap,
    employmentMap,
    jobTitleMap,
    orgUnitMap,
    jobLevelMap,
    gradeMap,
    loadData,
    expandedIds,
    draggedIdsRef,
    handleFieldChange,
    handleColumnSearch,
    handleExcelImport,
    initialPostsMap,
    flattenedTree,
    postsMap,
    handleResetChanges,
    handleSaveChanges,
    handleRowClick,
    toggleExpand,
    expandAll,
    collapseAll,
    isDescendant,
    handleDragStart,
    handleDragOverRow,
    handleDropOnRow,
    handleDropOnRoot,
    updateNodesParent,
    handleGlobalSearch,
    handleIsOverRootZone: setIsOverRootZone,
    handleDragOverId: setDragOverId,
    
    deleteTarget,isDeleting,
    handleOpenDeleteModal,handleCloseDeleteModal,handleConfirmDelete
  };
};