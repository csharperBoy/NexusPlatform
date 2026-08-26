// src/modules/PhoneBook/pages/Post/PhoneBookPage.tsx

import React, { useEffect, useState, useMemo } from "react";
import logo from "../../../../assets/LOGO2.png";
import { phonebookApi } from "../../api/PhoneBookApi";
import {
  PhoneBookEmploymentDto,
  ContactTypeEnum,
  ContactSourceEnum,
  ContactDetailDto,
} from "../../models/PhoneBookEmploymentDto";

// --- Helper Functions ---
const getContactTypeBadge = (type?: ContactTypeEnum | null) => {
  switch (type) {
    case ContactTypeEnum.Mobile: return { label: "موبایل", color: "bg-blue-100 text-blue-800 border-blue-200" };
    case ContactTypeEnum.OrganizationMobile: return { label: "موبایل", color: "bg-blue-100 text-blue-800 border-blue-200" };
    case ContactTypeEnum.Phone: return { label: "تلفن ثابت", color: "bg-green-100 text-green-800 border-green-200" };
    case ContactTypeEnum.OfficePhone: return { label: "تلفن ثابت", color: "bg-green-100 text-green-800 border-green-200" };
    case ContactTypeEnum.Email: return { label: "ایمیل", color: "bg-purple-100 text-purple-800 border-purple-200" };
    case ContactTypeEnum.Fax: return { label: "فکس", color: "bg-orange-100 text-orange-800 border-orange-200" };
    case ContactTypeEnum.Address: return { label: "آدرس", color: "bg-gray-100 text-gray-800 border-gray-200" };
    default: return { label: "تماس", color: "bg-gray-100 text-gray-700 border-gray-200" };
  }
};

const getSourceBadge = (source?: ContactSourceEnum | null) => {
  switch (source) {
    case ContactSourceEnum.Personal: return { label: "شخصی", color: "bg-gray-200 text-gray-600" };
    case ContactSourceEnum.post: return { label: "سازمانی", color: "bg-amber-50 text-amber-700 border border-amber-200" };
    case ContactSourceEnum.employment: return { label: "سازمانی", color: "bg-amber-50 text-amber-700 border border-amber-200" };
    case ContactSourceEnum.location: return { label: "محل استقرار", color: "bg-blue-200 text-gray-600" };
   default: return { label: "سازمانی", color: "bg-gray-100 text-gray-700 border-gray-200" };
  }
};

// تابع بیرون کشیده شده برای آیکون سورت (جلوگیری از re-render)
const SortIcon = ({ column, sortConfig }: { column: string, sortConfig: SortConfig }) => {
  if (sortConfig.column !== column) return <span className="text-gray-300 mr-1 text-[10px]">↕</span>;
  if (sortConfig.direction === "asc") return <span className="text-blue-600 mr-1 text-[10px]">▲</span>;
  if (sortConfig.direction === "desc") return <span className="text-blue-600 mr-1 text-[10px]">▼</span>;
  return <span className="text-gray-300 mr-1 text-[10px]">↕</span>;
};

// --- Types ---
type GroupByOption = "none" | "headOfOrganizationUnitsName" | "jobTitleName" | "locationTitle";
type SortDirection = "asc" | "desc" | null;
interface SortConfig {
  column: string;
  direction: SortDirection;
}

export const PhoneBookPage: React.FC = () => {
  const [data, setData] = useState<PhoneBookEmploymentDto[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  // --- States ---
  const [globalSearch, setGlobalSearch] = useState<string>("");
  const [columnSearch, setColumnSearch] = useState<Record<string, string>>({});
  const [sortConfig, setSortConfig] = useState<SortConfig>({ column: "", direction: null });
  const [groupBy, setGroupBy] = useState<GroupByOption>("headOfOrganizationUnitsName");

  // Setهایی برای مدیریت باز و بسته بودن کرکره‌ها
  const [collapsedGroups, setCollapsedGroups] = useState<Set<string>>(new Set());
  const [expandedRows, setExpandedRows] = useState<Set<string>>(new Set());

  useEffect(() => {
    fetchPhoneBook();
  }, []);

  const fetchPhoneBook = async () => {
    try {
      setLoading(true);
      const result = await phonebookApi.GetList();
      setData(result || []);
    } catch (err: any) {
      setError(err?.message || "خطا در دریافت اطلاعات دفترچه تلفن");
    } finally {
      setLoading(false);
    }
  };

  // --- Handlers ---
  const toggleGroup = (groupName: string) => {
    setCollapsedGroups((prev) => {
      const next = new Set(prev);
      if (next.has(groupName)) next.delete(groupName);
      else next.add(groupName);
      return next;
    });
  };

  const toggleRowExpand = (employmentCode: string, hasMultiple: boolean) => {
    if (!hasMultiple) return;
    setExpandedRows((prev) => {
      const next = new Set(prev);
      if (next.has(employmentCode)) next.delete(employmentCode);
      else next.add(employmentCode);
      return next;
    });
  };

  const handleSort = (column: string) => {
    let direction: SortDirection = "asc";
    if (sortConfig.column === column) {
      if (sortConfig.direction === "asc") direction = "desc";
      else if (sortConfig.direction === "desc") direction = null;
    }
    setSortConfig({ column, direction });
  };

  const handleColumnSearch = (column: string, value: string) => {
    setColumnSearch((prev) => ({ ...prev, [column]: value }));
  };

  // --- Data Pipeline (Filter -> Sort -> Group) ---
  const processedData = useMemo(() => {
  let result = [...data];

  // ۱. فیلتر ستون‌ها
  Object.entries(columnSearch).forEach(([key, term]) => {
    if (term.trim()) {
      result = result.filter((emp) => {
        const q = term.toLowerCase();

        if (key === "fullName") {
          const full = emp.fullName || `${emp.firstName || ""} ${emp.lastName || ""}`;
          return full.toLowerCase().includes(q);
        }

        const value = emp[key as keyof PhoneBookEmploymentDto];
        
        // اگر undefined یا null باشه
        if (value == null) return false;

        // اگر آرایه باشه
        if (Array.isArray(value)) {
          return value.some(item => {
            if (typeof item !== 'string') return false;
            return item.toLowerCase().includes(q);
          });
        }

        // اگر رشته باشه
        if (typeof value === 'string') {
          return value.toLowerCase().includes(q);
        }

        return false;
      });
    }
  });

  // ۲. فیلتر گلوبال
  if (globalSearch.trim()) {
    const q = globalSearch.toLowerCase();
    result = result.filter((emp) => {
      const searchInString = (value?: string | null): boolean => {
        if (!value) return false;
        return value.toLowerCase().includes(q);
      };

      const searchInArray = (arr?: string[] | null): boolean => {
        if (!arr || arr.length === 0) return false;
        return arr.some(item => {
          if (!item) return false;
          return item.toLowerCase().includes(q);
        });
      };

      return (
        searchInString(emp.firstName) ||
        searchInString(emp.lastName) ||
        searchInArray(emp.headOfOrganizationUnitsName) ||
        searchInArray(emp.jobTitleName) ||
        searchInArray(emp.locationTitle) ||
        searchInString(emp.contactSummary)
      );
    });
  }

  // ۳. سورت
  if (sortConfig.direction && sortConfig.column) {
    result.sort((a, b) => {
      const col = sortConfig.column as keyof PhoneBookEmploymentDto;
      
      const getStringValue = (obj: PhoneBookEmploymentDto, column: keyof PhoneBookEmploymentDto): string => {
        if (column === "fullName") {
          return obj.fullName || `${obj.firstName || ""} ${obj.lastName || ""}`;
        }
        
        const val = obj[column];
        if (val == null) return "";
        if (Array.isArray(val)) {
          return val.filter(v => v != null).join(" - ");
        }
        return val.toString();
      };

      const aVal = getStringValue(a, col);
      const bVal = getStringValue(b, col);
      
      const compareResult = aVal.localeCompare(bVal, undefined, { numeric: true, sensitivity: 'base' });
      return sortConfig.direction === "asc" ? compareResult : -compareResult;
    });
  }

  // ۴. گروه‌بندی
  if (groupBy === "none") return { "همه اعضا": result };

  const groups: Record<string, PhoneBookEmploymentDto[]> = {};
  result.forEach(emp => {
    let groupValues: string[] = [];
    switch (groupBy) {
      case "headOfOrganizationUnitsName":
        groupValues = emp.headOfOrganizationUnitsName || [];
        break;
      case "jobTitleName":
        groupValues = emp.jobTitleName || [];
        break;
      case "locationTitle":
        groupValues = emp.locationTitle || [];
        break;
      default:
        groupValues = [];
    }

    // حذف مقادیر null/undefined و خالی
    const validGroupValues = groupValues.filter(v => v && v.trim().length > 0);
    
    if (validGroupValues.length === 0) {
      validGroupValues.push("تعریف نشده");
    }

    validGroupValues.forEach(value => {
      if (!groups[value]) groups[value] = [];
      groups[value].push(emp);
    });
  });

  return groups;
}, [data, globalSearch, columnSearch, sortConfig, groupBy]);


  if (loading) return <div className="p-8 text-center text-gray-500">در حال دریافت...</div>;
  if (error) return <div className="p-4 bg-red-50 text-red-700 rounded m-6">{error}</div>;

  return (
    <div className="p-6 dir-rtl text-right font-sans">
      {/* هدر و کنترل‌های اصلی */}
<div className="flex flex-wrap items-end justify-between gap-4 mb-6 p-4 rounded-xl border border-gray-200 shadow-sm"
     style={{ backgroundColor: 'rgb(0, 48, 111)' }}>
  
  {/* بخش لوگو و عنوان */}
  <div className="flex items-center gap-4 mb-3">
    <img 
      src={logo} 
      alt="لوگو سازمان" 
      className="h-16 md:h-20 w-auto object-contain drop-shadow-sm transition-transform duration-200 hover:scale-105"
    />
    <div className="h-10 md:h-12 w-[1.5px] bg-gray-300 rounded-full"></div>
    <div className="flex flex-col">
      <h1 className="font-black text-xl md:text-2xl text-white tracking-wide">
        سامانه جامع اطلاعات تماس همکاران
      </h1>
      <span className="text-xs text-blue-200 font-medium mt-0.5">
        دفترچه تلفن و راهنمای ارتباطات درون‌سازمانی شرکت فولاد امیرکبیر کاشان
      </span>
    </div>
  </div>

  {/* بخش کنترل‌ها (جستجو و گروه‌بندی) - بدون تغییر */}
  <div className="flex items-center gap-4">
    <div className="flex flex-col">
      <label className="text-xs text-blue-200 mb-1">جستجوی کلی</label>
      <input
        type="text"
        placeholder="جستجو در تمام فیلدها..."
        value={globalSearch}
        onChange={(e) => setGlobalSearch(e.target.value)}
        className="px-4 py-2 border border-gray-300 rounded-lg text-sm w-64 focus:ring-2 focus:ring-blue-500 outline-none bg-white/90 backdrop-blur-sm"
      />
    </div>

    <div className="flex flex-col">
      <label className="text-xs text-blue-200 mb-1">گروه‌بندی بر اساس</label>
      <select
        value={groupBy}
        onChange={(e) => setGroupBy(e.target.value as GroupByOption)}
        className="px-4 py-2 border border-gray-300 rounded-lg text-sm bg-white/90 backdrop-blur-sm focus:ring-2 focus:ring-blue-500 outline-none"
      >
        <option value="headOfOrganizationUnitsName">واحد سازمانی</option>
        <option value="jobTitleName">عنوان شغلی</option>
        <option value="locationTitle">محل استقرار</option>
        <option value="none">بدون گروه‌بندی</option>
      </select>
    </div>
  </div>
</div>

      {/* جدول یکپارچه */}
      <div className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden overflow-x-auto">
        <table className="w-full text-right border-collapse">
          <thead>
            {/* ردیف اول: عنوان ستون‌ها و دکمه سورت */}
            <tr className="bg-gray-100 border-b border-gray-200 text-gray-700 text-sm">
              <th className="py-3 px-4 w-12"></th>
              
              <th className="py-3 px-4 font-semibold cursor-pointer hover:bg-gray-200" onClick={() => handleSort("fullName")}>
                نام و نام خانوادگی <SortIcon column="fullName" sortConfig={sortConfig} />
              </th>
              <th className="py-3 px-4 font-semibold cursor-pointer hover:bg-gray-200" onClick={() => handleSort("headOfOrganizationUnitsName")}>
                واحد سازمانی <SortIcon column="headOfOrganizationUnitsName" sortConfig={sortConfig} />
              </th>
              <th className="py-3 px-4 font-semibold cursor-pointer hover:bg-gray-200" onClick={() => handleSort("jobTitleName")}>
                عنوان شغلی <SortIcon column="jobTitleName" sortConfig={sortConfig} />
              </th>
              <th className="py-3 px-4 font-semibold cursor-pointer hover:bg-gray-200" onClick={() => handleSort("locationTitle")}>
                محل استقرار <SortIcon column="locationTitle" sortConfig={sortConfig} />
              </th>
              <th className="py-3 px-4 font-semibold cursor-pointer hover:bg-gray-200" onClick={() => handleSort("contactSummary")}>
                اطلاعات تماس <SortIcon column="contactSummary" sortConfig={sortConfig} />
              </th>
            </tr>
            {/* ردیف دوم: باکس‌های جستجو زیر هر ستون */}
            <tr className="bg-gray-50 border-b border-gray-200">
              <th className="py-2 px-2"></th>
              
              <th className="py-2 px-2 align-top">
                <input
                  type="text"
                  placeholder="جستجو نام..."
                  value={columnSearch["fullName"] || ""}
                  onChange={(e) => handleColumnSearch("fullName", e.target.value)}
                  className="w-full mt-2 px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                />
              </th>
              <th className="py-2 px-2 align-top">
                <input
                  type="text"
                  placeholder="جستجو واحد..."
                  value={columnSearch["headOfOrganizationUnitsName"] || ""}
                  onChange={(e) => handleColumnSearch("headOfOrganizationUnitsName", e.target.value)}
                  className="w-full mt-2 px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                />
              </th>
              <th className="py-2 px-2 align-top">
                <input
                  type="text"
                  placeholder="جستجو سمت..."
                  value={columnSearch["jobTitleName"] || ""}
                  onChange={(e) => handleColumnSearch("jobTitleName", e.target.value)}
                  className="w-full mt-2 px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                />
              </th>
              <th className="py-2 px-2 align-top">
                <input
                  type="text"
                  placeholder="جستجو محل..."
                  value={columnSearch["locationTitle"] || ""}
                  onChange={(e) => handleColumnSearch("locationTitle", e.target.value)}
                  className="w-full mt-2 px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                />
              </th>
              <th className="py-2 px-2 align-top">
                <input
                  type="text"
                  placeholder="جستجو تماس..."
                  value={columnSearch["contactSummary"] || ""}
                  onChange={(e) => handleColumnSearch("contactSummary", e.target.value)}
                  className="w-full mt-2 px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                />
              </th>
            </tr>
          </thead>

          <tbody className="divide-y divide-gray-100">
            {Object.keys(processedData).length === 0 ? (
              <tr>
                <td colSpan={6} className="text-center py-12 text-gray-500">رکوردی یافت نشد.</td>
              </tr>
            ) : (
              Object.entries(processedData).map(([groupName, employments]) => {
                const isGroupCollapsed = collapsedGroups.has(groupName);

                return (
                  <React.Fragment key={groupName}>
                    {/* ردیف هدر گروه (فقط اگر گروه‌بندی فعال باشد) */}
                    {groupBy !== "none" && (
                      <tr 
                        className="bg-blue-50/50 hover:bg-blue-50 cursor-pointer border-t-2 border-t-blue-100"
                        onClick={() => toggleGroup(groupName)}
                      >
                        <td colSpan={6} className="py-3 px-4">
                          <div className="flex items-center justify-between w-full">
                            <div className="flex items-center gap-3">
                              <span className={`transform transition-transform duration-200 inline-block text-blue-600 text-xs ${isGroupCollapsed ? "rotate-90" : "rotate-0"}`}>
                                ▼
                              </span>
                              <span className="font-bold text-gray-800">{groupName}</span>
                            </div>
                            <span className="text-xs bg-white text-blue-800 border border-blue-200 px-3 py-1 rounded-full shadow-sm">
                              {employments.length} نفر
                            </span>
                          </div>
                        </td>
                      </tr>
                    )}

                    {/* ردیف‌های کارمندان داخل این گروه */}
                    {!isGroupCollapsed && employments.map((emp) => {
                      const empCode = emp.employmentCode;
                      const isExpanded = expandedRows.has(empCode);
                      const hasMultiple = emp.hasMultipleContacts ?? (emp.contacts && emp.contacts.length > 1);

                      return (
                        <React.Fragment key={empCode}>
                          {/* سطر اصلی */}
                          <tr
                            onClick={() => toggleRowExpand(empCode, !!hasMultiple)}
                            className={`transition-colors text-sm ${hasMultiple ? "cursor-pointer hover:bg-gray-50" : ""} ${isExpanded ? "bg-gray-50" : ""}`}
                          >
                            <td className="py-3 px-4 text-center">
                              {hasMultiple ? (
                                <span className={`text-gray-400 font-bold text-[10px] inline-block transition-transform duration-200 ${isExpanded ? "rotate-[-90deg]" : "rotate-0"}`}>
                                  ◀
                                </span>
                              ) : null}
                            </td>
                            <td className="py-3 px-4 font-medium text-gray-800">
                              {emp.fullName || `${emp.firstName || ""} ${emp.lastName || ""}`}
                            </td>
                            <td className="py-3 px-4 text-gray-600">{emp.headOfOrganizationUnitsName?.join(" - ") || "-"}</td>
                            <td className="py-3 px-4 text-gray-600">{emp.jobTitleName?.join(" - ") || "-"}</td>
                            <td className="py-3 px-4 text-gray-600"> {emp.locationTitle?.join(" - ") || "-"}</td>
                            <td className="py-3 px-4 font-mono text-gray-700 text-left dir-ltr">
                              {emp.contactSummary || "-"}
                            </td>
                          </tr>

                          {/* زیر‌جدول راه‌های ارتباطی */}
                          {hasMultiple && isExpanded && (
                            <tr className="bg-gray-50">
                              <td colSpan={6} className="p-4 px-12 border-b border-gray-200">
                                <div className="bg-white border border-gray-200 rounded-lg p-4 shadow-inner">
                                  <h4 className="text-xs font-bold text-gray-500 mb-3 border-b pb-2">
                                    جزییات تماس
                                  </h4>
                                  <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
                                    {emp.contacts?.map((contact: ContactDetailDto, index: number) => {
                                      const typeBadge = getContactTypeBadge(contact.type);
                                      const sourceBadge =  getSourceBadge(contact.source);
                                      // const isOrg = contact.source === ContactSourceEnum.post;

                                      return (
                                        <div key={index} className="flex flex-col p-2.5 bg-gray-50 rounded-md border border-gray-100">
                                          <div className="flex justify-between items-center mb-1">
                                            <span className={`text-[10px] px-1.5 py-0.5 rounded border ${typeBadge.color}`}>
                                              {typeBadge.label}
                                            </span>
                                            <span className={`text-[10px] px-1.5 py-0.5 rounded  ${sourceBadge.color}`}>
                                              {sourceBadge.label}
                                            </span>
                                          </div>
                                          <div className="flex justify-between items-center mt-1">
                                            <span className="text-xs text-gray-500">{contact.title}</span>
                                            <span className="font-mono text-sm font-semibold text-gray-800 dir-ltr">{contact.value}</span>
                                          </div>
                                        </div>
                                      );
                                    })}
                                  </div>
                                </div>
                              </td>
                            </tr>
                          )}
                        </React.Fragment>
                      );
                    })}
                  </React.Fragment>
                );
              })
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default PhoneBookPage;