// src/modules/HR/hooks/usePostContactManagement.ts
import { useState, useEffect, useMemo, useRef } from "react";
import * as XLSX from "xlsx";
import { postContactApi } from "../../api/PostContactApi";
import { PostContactInfoView } from "../../models/postContactInfoView";
import { UpdatePostContactCommand } from "../../models/postContactCommand";

export interface FlattenedNode {
  node: PostContactInfoView;
  depth: number;
  hasChildren: boolean;
  isExpanded: boolean;
  isModified: boolean;
}

// تابع کمکی برای تبدیل آرایه یا رشته به یک رشته قابل سرچ
const toSearchableString = (val: string | string[] | null | undefined): string => {
  if (!val) return "";
  if (Array.isArray(val)) return val.join(" ");
  return String(val);
};

// تابع کمکی برای مقایسه دو آرایه تگ جهت تشخیص تغییرات
const areArraysEqual = (a?: string[] | null, b?: string[] | null): boolean => {
  const arrA = a || [];
  const arrB = b || [];
  if (arrA.length !== arrB.length) return false;
  return arrA.every((v, i) => v === arrB[i]);
};

// تبدیل ورودی اکسل (چه رشته متصل با ویرگول چه آرایه) به آرایه تگ‌ها
const parseExcelTagArray = (val: any): string[] => {
  if (!val) return [];
  if (Array.isArray(val)) return val.map(String);
  return String(val)
    .split(/[,،]/)
    .map((s) => s.trim())
    .filter(Boolean);
};

export const usePostContactManagement = () => {
  // --- States ---
  const [postContacts, setPostContacts] = useState<PostContactInfoView[]>([]);
  const [initialPostContacts, setInitialPostContacts] = useState<PostContactInfoView[]>([]);
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

  // انتخاب و درگ اند دراپ
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [lastSelectedId, setLastSelectedId] = useState<string | null>(null);
  const [draggedIds, setDraggedIds] = useState<string[]>([]);
  const [dragOverId, setDragOverId] = useState<string | null>(null);

  const fileInputRef = useRef<HTMLInputElement | null>(null);

  const initialPostContactsMap = useMemo(() => {
    const map = new Map<string, PostContactInfoView>();
    initialPostContacts.forEach((p) => map.set(p.id, p));
    return map;
  }, [initialPostContacts]);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await postContactApi.GetList();
      const list = data || [];
      setPostContacts(list);
      setInitialPostContacts(JSON.parse(JSON.stringify(list)));

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

  // --- تغییر مستقیم مقادیر به صورت string[] ---
  const handleFieldChange = (
    id: string,
    field: "officePhone" | "orgMobile",
    newTags: string[]
  ) => {
    setPostContacts((prev) =>
      prev.map((item) => {
        if (item.id === id) {
          return { ...item, [field]: newTags };
        }
        return item;
      })
    );
    setModifiedIds((prev) => new Set(prev).add(id));
  };

  const handleColumnSearch = (column: string, value: string) => {
    setColumnSearch((prev) => ({ ...prev, [column]: value }));
  };

  // --- بارگذاری اکسل ---
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

        setPostContacts((prevPostContacts) => {
          const postContactByEmpCodeMap = new Map<string, PostContactInfoView>();
          prevPostContacts.forEach((p) => {
            if (p.employmentCode) {
              postContactByEmpCodeMap.set(String(p.employmentCode).trim(), p);
            }
          });

          const nextPostContacts = prevPostContacts.map((p) => ({ ...p }));

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

            const matchedPostContact = postContactByEmpCodeMap.get(empCodeStr);
            if (matchedPostContact) {
              const targetIndex = nextPostContacts.findIndex((p) => p.id === matchedPostContact.id);
              if (targetIndex === -1) return;

              let isRowChanged = false;

              if (officePhoneKey && row[officePhoneKey] !== undefined) {
                const newPhoneTags = parseExcelTagArray(row[officePhoneKey]);
                if (!areArraysEqual(nextPostContacts[targetIndex].officePhone, newPhoneTags)) {
                  nextPostContacts[targetIndex].officePhone = newPhoneTags;
                  isRowChanged = true;
                }
              }

              if (orgMobileKey && row[orgMobileKey] !== undefined) {
                const newMobileTags = parseExcelTagArray(row[orgMobileKey]);
                if (!areArraysEqual(nextPostContacts[targetIndex].orgMobile, newMobileTags)) {
                  nextPostContacts[targetIndex].orgMobile = newMobileTags;
                  isRowChanged = true;
                }
              }

              if (isRowChanged) {
                updatedCount++;
                newModifiedIds.add(matchedPostContact.id);
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

          return nextPostContacts;
        });
      } catch (err: any) {
        setError("خطا در پردازش فایل اکسل: " + (err?.message || "فرمت فایل نامعتبر است"));
      } finally {
        e.target.value = "";
      }
    };

    reader.readAsArrayBuffer(file);
  };

  // --- ساختار درخت و فیلتر ---
  const { flattenedTree, postContactsMap } = useMemo(() => {
    const map = new Map<string, PostContactInfoView>();
    const childrenMap = new Map<string | null, PostContactInfoView[]>();

    postContacts.forEach((p) => map.set(p.id, p));

    postContacts.forEach((p) => {
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

        const initChild = initialPostContactsMap.get(child.id);

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

        const officePhoneStr = toSearchableString(child.officePhone);
        const orgMobileStr = toSearchableString(child.orgMobile);
        const initOfficePhoneStr = toSearchableString(initChild?.officePhone);
        const initOrgMobileStr = toSearchableString(initChild?.orgMobile);

        const matchesGlobal =
          !normalizedGlobal ||
          fullJobTitle.toLowerCase().includes(normalizedGlobal) ||
          (child.organizationUnitsName || "").toLowerCase().includes(normalizedGlobal) ||
          occupantName.toLowerCase().includes(normalizedGlobal) ||
          officePhoneStr.toLowerCase().includes(normalizedGlobal) ||
          orgMobileStr.toLowerCase().includes(normalizedGlobal) ||
          (initChild &&
            (initFullJobTitle.toLowerCase().includes(normalizedGlobal) ||
              (initChild.organizationUnitsName || "").toLowerCase().includes(normalizedGlobal) ||
              initOccupantName.toLowerCase().includes(normalizedGlobal) ||
              initOfficePhoneStr.toLowerCase().includes(normalizedGlobal) ||
              initOrgMobileStr.toLowerCase().includes(normalizedGlobal)));

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
            const matchCur = officePhoneStr.toLowerCase().includes(q);
            const matchInit = initOfficePhoneStr.toLowerCase().includes(q);
            if (!matchCur && !matchInit) matchesColumns = false;
          }
          if (col === "orgMobile") {
            const matchCur = orgMobileStr.toLowerCase().includes(q);
            const matchInit = initOrgMobileStr.toLowerCase().includes(q);
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

    return { flattenedTree: flattened, postContactsMap: map };
  }, [postContacts, expandedIds, globalSearch, columnSearch, modifiedIds, initialPostContactsMap]);

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
    postContacts.forEach((p) => {
      if (postContacts.some((child) => child.fkParentId === p.id)) {
        allParentIds.add(p.id);
      }
    });
    setExpandedIds(allParentIds);
  };

  const collapseAll = () => {
    setExpandedIds(new Set());
  };

  const handleResetChanges = () => {
    if (window.confirm("آیا از لغو تمام تغییرات اعمال شده اطمینان دارید؟")) {
      setPostContacts(JSON.parse(JSON.stringify(initialPostContacts)));
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

      const commands: UpdatePostContactCommand[] = Array.from(modifiedIds).map((id) => {
        const postContact = postContactsMap.get(id)!;
        return {
          id: postContact.id,
        //   code: postContact.postCode,
        //   reportsToPostContactId: postContact.fkParentId,
          officePhone: postContact.officePhone || [],
        //   orgEmail: postContact.orgEmail,
          orgMobile: postContact.orgMobile || [],
        //   isActive: true,
        };
      });

      await postContactApi.batchUpdatePostsContact(commands);

      setSuccessMessage(`تعداد ${commands.length} تغییر با موفقیت ذخیره شد.`);
      setInitialPostContacts(JSON.parse(JSON.stringify(postContacts)));
      setModifiedIds(new Set());

      setTimeout(() => setSuccessMessage(null), 4000);
    } catch (err: any) {
      setError(err?.message || "خطا در ذخیره تغییرات چارت");
    } finally {
      setSaving(false);
    }
  };

  return {
    postContacts,
    flattenedTree,
    loading,
    saving,
    error,
    successMessage,
    globalSearch,
    setGlobalSearch,
    columnSearch,
    handleColumnSearch,
    selectedIds,
    modifiedIds,
    draggedIds,
    dragOverId,
    fileInputRef,
    handleExcelImport,
    handleFieldChange,
    toggleExpand,
    expandAll,
    collapseAll,
    handleResetChanges,
    handleSaveChanges,
  };
};