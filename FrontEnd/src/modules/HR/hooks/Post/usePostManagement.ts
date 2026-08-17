//src
import React, { useEffect, useState, useMemo, useRef } from "react";
import * as XLSX from "xlsx";
import { postApi } from "../../api/PostApi";
import { PostInfoView } from "../../models/postInfoView";
import { UpdatePostCommand } from "../../models/postCommand";
import { SelectionListDto } from "@/core/models/SelectionListDto";
import { locationApi } from "../../api/LocationApi";
import { employmentApi } from "../../api/EmploymentApi";
interface FlattenedNode {
  node: PostInfoView;
  depth: number;
  hasChildren: boolean;
  isExpanded: boolean;
  isModified: boolean;
}
type EditableField =
  | "employmentId"
  | "locationId"; // افزودن فیلد locationId

export const usePostManagement = () => {
  // --- States ---
  const [posts, setPosts] = useState<PostInfoView[]>([]);
  const [initialPosts, setInitialPosts] = useState<PostInfoView[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [saving, setSaving] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
 const [locations, setLocations] = useState<SelectionListDto[]>([]); 
 
 const [employments, setEmployments] = useState<SelectionListDto[]>([]); 
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

  // مپ برای دسترسی سریع به عنوان مکان‌ها بر اساس Value/ID
  const locationMap = useMemo(() => {
    const map = new Map<string, string>();
    locations.forEach((loc) => map.set(loc.value, loc.display || loc.label));
    return map;
  }, [locations]);

  const employmentMap = useMemo(() => {
    const map = new Map<string, string>();
    employments.forEach((loc) => map.set(loc.value, loc.display || loc.label));
    return map;
  }, [employments]);
  // --- 1. دریافت اطلاعات اولیه ---
  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      setError(null);
      const locList = await locationApi.GetSelectionList();
      const empList = await employmentApi.GetSelectionList();
      const data = await postApi.GetList();
      const list = data || [];
      // مپ کردن اولیه locationsId به locationId برای کار ساده‌تر با Dropdown
      const normalizedList = list.map((post: any) => ({
        ...post,
        locationId: post.locationId || (Array.isArray(post.locationsId) && post.locationsId.length > 0 ? post.locationsId[0] : ""),
      }));

      setPosts(normalizedList);
      setInitialPosts(JSON.parse(JSON.stringify(normalizedList)));
      setLocations(locList || []);
      setEmployments(empList || []);
      const parentIds = new Set<string>();
      normalizedList.forEach((p) => {
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
  const handleFieldChange = (id: string, field: EditableField, value: string) => {
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
            
            const locationKey = Object.keys(row).find((k) =>
              ["محل استقرار", "مکان", "محل_استقرار", "location", "locationid"].includes(
                k.trim().toLowerCase()
              )
            );
            const empKey = Object.keys(row).find((k) =>
              ["شاغل فعلی", "شاغل", "شاغل_فعلی", "employment", "employmentId"].includes(
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

              if (locationKey && row[locationKey] !== undefined) {
                const rawLoc = String(row[locationKey] ?? "").trim();
                const matchedLocation = locations.find(
                  (l) =>
                    l.value.toLowerCase() === rawLoc.toLowerCase() ||
                    l.display.toLowerCase() === rawLoc.toLowerCase() ||
                    l.label.toLowerCase() === rawLoc.toLowerCase()
                );

                const newLocId = matchedLocation ? matchedLocation.value : rawLoc;
                if ((nextPosts[targetIndex] as any).locationId !== newLocId) {
                  (nextPosts[targetIndex] as any).locationId = newLocId;
                  isRowChanged = true;
                }
              }

              if (empKey && row[empKey] !== undefined) {
                const rawEmp = String(row[empKey] ?? "").trim();
                const matchedEmp = employments.find(
                  (l) =>
                    l.value.toLowerCase() === rawEmp.toLowerCase() ||
                    l.display.toLowerCase() === rawEmp.toLowerCase() ||
                    l.label.toLowerCase() === rawEmp.toLowerCase()
                );

                const newEmpId = matchedEmp ? matchedEmp.value : rawEmp;
                if ((nextPosts[targetIndex] as any).employmentId !== newEmpId) {
                  (nextPosts[targetIndex] as any).employmentId = newEmpId;
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
 
     
        
      const postLocationName = (locationMap.get(child.locationId || "") || "").toLowerCase();
      const initLocationName = (locationMap.get(initChild?.locationId || "") || "").toLowerCase();


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
        postLocationName.includes(normalizedGlobal) ||
          (initChild &&
            (initFullJobTitle.toLowerCase().includes(normalizedGlobal) ||
              (initChild.organizationUnitsName || "").toLowerCase().includes(normalizedGlobal) ||
              initOccupantName.toLowerCase().includes(normalizedGlobal) ||
            initLocationName.includes(normalizedGlobal)));

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
         
          if (col === "levelGrade") {
            const matchCur = levelGrade.toLowerCase().includes(q);
            const matchInit = initLevelGrade.toLowerCase().includes(q);
            if (!matchCur && !matchInit) matchesColumns = false;
          }
          if (col === "locationId") {
          const matchCur = postLocationName.includes(q);
          const matchInit = initLocationName.includes(q);
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
  }, [posts, expandedIds, globalSearch, columnSearch, modifiedIds, initialPostsMap,locationMap]);

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
const handleGlobalSearch = ( value: string) => {
    setGlobalSearch( value);
  };

  const handleIsOverRootZone = ( value: boolean) => {
    setIsOverRootZone( value);
  };
const handleDragOverId = ( value?: string | null) => {
    setDragOverId(value||null);
  };

  const handleSaveChanges = async () => {
    if (modifiedIds.size === 0) return;

    try {
      setSaving(true);
      setError(null);
      setSuccessMessage(null);

      const commands: UpdatePostCommand[] = Array.from(modifiedIds).map((id) => {
        const post = postsMap.get(id)!;

         const selectedLocationId = post.locationId || null;
        const locationsIdList = selectedLocationId ? [selectedLocationId] : [];

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
          locationsId: locationsIdList,
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
}
     return {
        posts,
        loading,
        initialPosts,
        locations,
        employments,
        saving,
        draggedIds,
        dragOverId,
        isOverRootZone,
        error,handleIsOverRootZone,handleDragOverId,
        successMessage,
        globalSearch,selectedIds,
        handleGlobalSearch,
        lastSelectedId,
        columnSearch,
        modifiedIds,
        fileInputRef,
        locationMap,
        employmentMap,
        loadData,
        expandedIds,draggedIdsRef,
        handleFieldChange,
        handleColumnSearch,
        handleExcelImport,
        initialPostsMap,
        flattenedTree,
        postsMap,
        handleResetChanges,
        handleSaveChanges,handleRowClick,toggleExpand,expandAll,collapseAll,isDescendant,handleDragStart,handleDragOverRow,handleDropOnRow,handleDropOnRoot,updateNodesParent
    };
  };


