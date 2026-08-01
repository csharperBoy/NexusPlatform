// src/modules/HR/pages/Post/PostManagementPage.tsx

import React, { useEffect, useState, useMemo, useRef } from "react";
import * as XLSX from "xlsx"; // اضافه شده جهت خواندن فایل اکسل
import { postApi } from "../../api/PostApi";
import { PostInfoView } from "../../models/postInfoView";
import { UpdatePostCommand } from "../../models/postCommand";

// تایپ ردیف‌های تخت شده‌ی درخت
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

  // استیت‌های درگ اند دراپ
  const [draggedId, setDraggedId] = useState<string | null>(null);
  const [dragOverId, setDragOverId] = useState<string | null>(null);
  const [isOverRootZone, setIsOverRootZone] = useState<boolean>(false);

  // ریف مربوط به آپلود فایل اکسل
  const fileInputRef = useRef<HTMLInputElement | null>(null);

  const draggedIdRef = useRef<string | null>(null);
  draggedIdRef.current = draggedId;

  // مپ مقادیر اولیه برای جلوگیری از محو شدن سطر در حال ویرایش هنگام سرچ
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

      // باز نگه‌داشتن گره‌های دارای فرزند
      const parentIds = new Set<string>();
      list.forEach((p) => {
        if (p.fkParentId) parentIds.add(p.fkParentId);
      });
      setExpandedIds(parentIds);
      setModifiedIds(new Set());
    } catch (err: any) {
      setError(err?.message || "خطا در دریافت لیست چارت سازمانی");
    } finally {
      setLoading(false);
    }
  };

  // --- 2. مدیریت ویرایش درجا (تلفن و موبایل) ---
  const handleFieldChange = (id: string, field: "officePhone" | "orgMobile", value: string) => {
    setPosts((prev) =>
      prev.map((item) => {
        if (item.id === id) {
          return { ...item, [field]: value };
        }
        return item;
      })
    );
    // علامت‌گذاری به عنوان تغییر یافته
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

        // تبدیل شیت به JSON
        const excelRows = XLSX.utils.sheet_to_json<Record<string, any>>(worksheet);

        if (!excelRows || excelRows.length === 0) {
          alert("فایل اکسل انتخاب شده خالی است یا فرمت معتبری ندارد.");
          return;
        }

        let updatedCount = 0;
        const newModifiedIds = new Set(modifiedIds);

        setPosts((prevPosts) => {
          // ایجاد یک مپ بر اساس کد پرسنلی جهت جستجوی سریع
          const postByEmpCodeMap = new Map<string, PostInfoView>();
          prevPosts.forEach((p) => {
            if (p.employeeCode) {
              postByEmpCodeMap.set(String(p.employeeCode).trim(), p);
            }
          });

          const nextPosts = prevPosts.map((p) => ({ ...p }));

          excelRows.forEach((row) => {
            // پیدا کردن کلیدهای ستون‌ها فارغ از کوچک/بزرگ بودن حروف یا فاصله‌ها
            const empCodeKey = Object.keys(row).find((k) =>
              ["کد پرسنلی", "کدپرسنلی", "employeecode", "empcode", "کد"].includes(
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

              // بررسی تغییر تلفن داخلی
              if (officePhoneKey && row[officePhoneKey] !== undefined) {
                const newPhone = String(row[officePhoneKey] ?? "").trim();
                if (nextPosts[targetIndex].officePhone !== newPhone) {
                  nextPosts[targetIndex].officePhone = newPhone;
                  isRowChanged = true;
                }
              }

              // بررسی تغییر موبایل سازمانی
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
        // ریست کردن ورودی فایل جهت امکان آپلود مجدد همان فایل
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
        const occupantName = `${child.firstName || ""} ${child.lastName || ""} ${child.employeeCode || ""}`;
        const levelGrade = `${child.jobLevelTitle || ""} ${child.gradeTitle || ""}`;

        const initFullJobTitle = initChild
          ? `${initChild.jobTitleName || ""} ${initChild.postCode ? `(${initChild.postCode})` : ""}`
          : fullJobTitle;
        const initOccupantName = initChild
          ? `${initChild.firstName || ""} ${initChild.lastName || ""} ${initChild.employeeCode || ""}`
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

  // --- 4. متدهای مدیریت درخت ---
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

  // --- 5. منطق Drag and Drop ---
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
    e.dataTransfer.setData("text/plain", id);
    e.dataTransfer.effectAllowed = "move";
    setDraggedId(id);
  };

  const handleDragOverRow = (e: React.DragEvent, targetId: string) => {
    e.preventDefault();
    const currentDragged = draggedIdRef.current;
    if (!currentDragged || currentDragged === targetId) return;

    if (isDescendant(targetId, currentDragged)) {
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

    const draggedNodeId = e.dataTransfer.getData("text/plain") || draggedId;
    if (!draggedNodeId || draggedNodeId === targetParentId) return;

    if (isDescendant(targetParentId, draggedNodeId)) {
      alert("امکان انتقال یک والد به زیرمجموعه‌های خودش وجود ندارد!");
      setDraggedId(null);
      return;
    }

    const draggedNode = postsMap.get(draggedNodeId);
    if (!draggedNode || draggedNode.fkParentId === targetParentId) {
      setDraggedId(null);
      return;
    }

    updateNodeParent(draggedNodeId, targetParentId);
    setDraggedId(null);
  };

  const handleDropOnRoot = (e: React.DragEvent) => {
    e.preventDefault();
    setIsOverRootZone(false);
    setDragOverId(null);

    const draggedNodeId = e.dataTransfer.getData("text/plain") || draggedId;
    if (!draggedNodeId) return;

    const draggedNode = postsMap.get(draggedNodeId);
    if (!draggedNode || draggedNode.fkParentId === null) {
      setDraggedId(null);
      return;
    }

    updateNodeParent(draggedNodeId, null);
    setDraggedId(null);
  };

  const updateNodeParent = (nodeId: string, newParentId: string | null) => {
    setPosts((prev) =>
      prev.map((item) => {
        if (item.id === nodeId) {
          return { ...item, fkParentId: newParentId };
        }
        return item;
      })
    );

    setModifiedIds((prev) => new Set(prev).add(nodeId));

    if (newParentId) {
      setExpandedIds((prev) => new Set(prev).add(newParentId));
    }
  };

  // --- 6. بازنشانی و ذخیره تغییرات ---
  const handleResetChanges = () => {
    if (window.confirm("آیا از لغو تمام تغییرات اعمال شده اطمینان دارید؟")) {
      setPosts(JSON.parse(JSON.stringify(initialPosts)));
      setModifiedIds(new Set());
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
    <div className="p-6 dir-rtl text-right font-sans bg-gray-50/50 min-h-screen">
      {/* هدر اصلی */}
      <div className="bg-white p-5 rounded-xl border border-gray-200 shadow-sm mb-6">
        <div className="flex flex-wrap items-center justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold text-gray-800 mb-1">مدیریت ساختار چارت سازمانی</h1>
            <p className="text-sm text-gray-500">
              کل پست‌ها: <span className="font-semibold text-gray-700">{posts.length}</span>
              {modifiedIds.size > 0 && (
                <span className="mr-3 text-amber-600 bg-amber-50 px-2 py-0.5 rounded border border-amber-200 text-xs font-medium">
                  {modifiedIds.size} تغییر ذخیره‌نشده
                </span>
              )}
            </p>
          </div>

          <div className="flex items-center gap-3">
            {/* اینپوت مخفی فایل اکسل */}
            <input
              type="file"
              ref={fileInputRef}
              onChange={handleExcelImport}
              accept=".xlsx, .xls"
              className="hidden"
            />

            {/* دکمه بارگذاری اکسل */}
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

          <div className="flex items-center gap-2">
            <button
              onClick={expandAll}
              className="px-3 py-1.5 text-xs text-gray-600 bg-gray-100 hover:bg-gray-200 rounded border border-gray-300"
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

      {/* منطقه رهاسازی ریشه - فیکس روی مرورگر (top-0) */}
      <div
        onDragOver={(e) => {
          e.preventDefault();
          if (draggedId) setIsOverRootZone(true);
        }}
        onDragLeave={() => setIsOverRootZone(false)}
        onDrop={handleDropOnRoot}
        className={`sticky top-0 z-30 mb-4 py-2.5 px-3 border-2 border-dashed rounded-xl text-center text-xs transition-all shadow-md backdrop-blur-md ${
          isOverRootZone
            ? "border-blue-500 bg-blue-100/95 text-blue-800 font-bold scale-[1.01]"
            : "border-gray-300 bg-white/95 text-gray-600 hover:border-gray-400"
        }`}
      >
        📌 جهت انتقال پست به بالاترین سطح چارت (بدون والد)، آن را اینجا رها کنید.
      </div>

      {/* جدول چارت با اسکرول کامل صفحه */}
      <div className="bg-white rounded-xl border border-gray-200 shadow-sm">
        <table className="w-full text-right border-collapse">
          <thead>
            {/* ردیف اول: عناوین ستون‌ها */}
            <tr className="border-b border-gray-200 text-gray-700 text-xs font-semibold">
              <th className="sticky top-[48px] z-20 bg-gray-100 py-3 px-3 w-10 text-center border-b border-gray-200 shadow-sm">
                جابه‌جایی
              </th>
              <th className="sticky top-[48px] z-20 bg-gray-100 py-3 px-4 border-b border-gray-200 shadow-sm">
                عنوان شغل (کد پست)
              </th>
              <th className="sticky top-[48px] z-20 bg-gray-100 py-3 px-4 border-b border-gray-200 shadow-sm">
                واحد سازمانی
              </th>
              <th className="sticky top-[48px] z-20 bg-gray-100 py-3 px-4 border-b border-gray-200 shadow-sm">
                شاغل فعلی
              </th>
              <th className="sticky top-[48px] z-20 bg-gray-100 py-3 px-4 w-36 border-b border-gray-200 shadow-sm">
                تلفن داخلی
              </th>
              <th className="sticky top-[48px] z-20 bg-gray-100 py-3 px-4 w-40 border-b border-gray-200 shadow-sm">
                موبایل سازمانی
              </th>
              <th className="sticky top-[48px] z-20 bg-gray-100 py-3 px-4 border-b border-gray-200 shadow-sm">
                رده / سطح شغلی
              </th>
              <th className="sticky top-[48px] z-20 bg-gray-100 py-3 px-4 text-center w-24 border-b border-gray-200 shadow-sm">
                وضعیت
              </th>
            </tr>

            {/* ردیف دوم: اینپوت‌های سرچ */}
            <tr className="border-b border-gray-200">
              <th className="sticky top-[89px] z-20 bg-gray-50 py-2 px-2 border-b border-gray-200 shadow-sm"></th>
              <th className="sticky top-[89px] z-20 bg-gray-50 py-2 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ شغل / کد..."
                  value={columnSearch["jobTitle"] || ""}
                  onChange={(e) => handleColumnSearch("jobTitle", e.target.value)}
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                />
              </th>
              <th className="sticky top-[89px] z-20 bg-gray-50 py-2 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ واحد..."
                  value={columnSearch["unit"] || ""}
                  onChange={(e) => handleColumnSearch("unit", e.target.value)}
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                />
              </th>
              <th className="sticky top-[89px] z-20 bg-gray-50 py-2 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ شاغل..."
                  value={columnSearch["occupant"] || ""}
                  onChange={(e) => handleColumnSearch("occupant", e.target.value)}
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                />
              </th>
              <th className="sticky top-[89px] z-20 bg-gray-50 py-2 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ داخلی..."
                  value={columnSearch["officePhone"] || ""}
                  onChange={(e) => handleColumnSearch("officePhone", e.target.value)}
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                />
              </th>
              <th className="sticky top-[89px] z-20 bg-gray-50 py-2 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ موبایل..."
                  value={columnSearch["orgMobile"] || ""}
                  onChange={(e) => handleColumnSearch("orgMobile", e.target.value)}
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                />
              </th>
              <th className="sticky top-[89px] z-20 bg-gray-50 py-2 px-2 align-top border-b border-gray-200 shadow-sm">
                <input
                  type="text"
                  placeholder="سرچ رده..."
                  value={columnSearch["levelGrade"] || ""}
                  onChange={(e) => handleColumnSearch("levelGrade", e.target.value)}
                  className="w-full px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                />
              </th>
              <th className="sticky top-[89px] z-20 bg-gray-50 py-2 px-2 border-b border-gray-200 shadow-sm"></th>
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
                const isBeingDragged = draggedId === node.id;
                const isTarget = dragOverId === node.id;
                const occupantName =
                  node.firstName || node.lastName
                    ? `${node.firstName || ""} ${node.lastName || ""}`
                    : "-";

                return (
                  <tr
                    key={node.id}
                    onDragOver={(e) => handleDragOverRow(e, node.id)}
                    onDragLeave={() => dragOverId === node.id && setDragOverId(null)}
                    onDrop={(e) => handleDropOnRow(e, node.id)}
                    className={`transition-colors ${
                      isBeingDragged ? "opacity-30 bg-gray-100" : ""
                    } ${
                      isTarget ? "bg-blue-50 border-y-2 border-blue-500" : "hover:bg-gray-50/80"
                    } ${isModified ? "bg-amber-50/40" : ""}`}
                  >
                    {/* Handle برای Drag & Drop */}
                    <td className="py-3 px-2 text-center align-middle">
                      <div
                        draggable
                        onDragStart={(e) => handleDragStart(e, node.id)}
                        className="cursor-grab active:cursor-grabbing text-gray-400 hover:text-gray-700 text-lg leading-none inline-block p-1"
                        title="جهت تغییر والد، کشیده و رها کنید"
                      >
                        ☰
                      </div>
                    </td>

                    {/* عنوان شغل + کد پست */}
                    <td className="py-3 px-4 font-medium text-gray-800">
                      <div
                        className="flex items-center gap-2"
                        style={{ paddingRight: `${depth * 24}px` }}
                      >
                        {hasChildren ? (
                          <button
                            type="button"
                            onClick={() => toggleExpand(node.id)}
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

                    {/* واحد سازمانی */}
                    <td className="py-3 px-4 text-gray-600 text-xs">
                      {node.organizationUnitsName || "-"}
                    </td>

                    {/* شاغل پست */}
                    <td className="py-3 px-4 text-gray-700 text-xs">
                      <div className="flex flex-col">
                        <span className="font-medium">{occupantName}</span>
                        {node.employeeCode && (
                          <span className="text-[10px] text-gray-400 font-mono">
                            کد: {node.employeeCode}
                          </span>
                        )}
                      </div>
                    </td>

                    {/* تلفن داخلی (قابل ویرایش درجا) */}
                    <td className="py-2 px-3">
                      <input
                        type="text"
                        value={node.officePhone || ""}
                        onChange={(e) => handleFieldChange(node.id, "officePhone", e.target.value)}
                        placeholder="داخلی..."
                        className="w-full px-2 py-1 text-xs border border-gray-300 rounded focus:ring-1 focus:ring-blue-500 font-mono text-center dir-ltr outline-none bg-white hover:border-gray-400 transition-colors"
                      />
                    </td>

                    {/* موبایل سازمانی (قابل ویرایش درجا) */}
                    <td className="py-2 px-3">
                      <input
                        type="text"
                        value={node.orgMobile || ""}
                        onChange={(e) => handleFieldChange(node.id, "orgMobile", e.target.value)}
                        placeholder="موبایل..."
                        className="w-full px-2 py-1 text-xs border border-gray-300 rounded focus:ring-1 focus:ring-blue-500 font-mono text-center dir-ltr outline-none bg-white hover:border-gray-400 transition-colors"
                      />
                    </td>

                    {/* رده / سطح شغلی */}
                    <td className="py-3 px-4 text-gray-500 text-xs">
                      {node.jobLevelTitle || node.gradeTitle ? (
                        <span>
                          {node.jobLevelTitle || ""} {node.gradeTitle ? `(${node.gradeTitle})` : ""}
                        </span>
                      ) : (
                        "-"
                      )}
                    </td>

                    {/* وضعیت تغییر */}
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