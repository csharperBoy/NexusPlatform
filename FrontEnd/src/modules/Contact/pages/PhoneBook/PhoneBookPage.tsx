// src/modules/PhoneBook/pages/Post/PhoneBookPage.tsx

import React, { useEffect, useState, useMemo } from "react";
import logo from "../../../../assets/LOGO3.png";
import { phonebookApi } from "../../api/PhoneBookApi";
import {
  PhoneBookEmploymentDto,
  ContactTypeEnum,
  ContactSourceEnum,
  ContactDetailDto,
  GenderEnum,
  ContactOwnershipEnum, // اضافه شده
} from "../../models/PhoneBookEmploymentDto";
import { FaUser } from 'react-icons/fa';
// آیکون‌های متناسب با نوع تماس
import { 
  FaMobileAlt, FaPhone, FaEnvelope, FaFax, FaGlobe, 
  FaWhatsapp, FaInstagram, FaTelegram, FaLinkedin, 
  FaMapMarkerAlt, FaMailBulk, FaHashtag, FaAddressCard,
  FaArrowLeft
} from 'react-icons/fa';
import { FaLink } from 'react-icons/fa';
// --- Helper Functions ---

const getContactIcon = (type?: ContactTypeEnum | null) => {
  switch (type) {
    case ContactTypeEnum.Mobile:
    case ContactTypeEnum.OrganizationMobile:
      return <FaMobileAlt className="text-blue-500" />;
    case ContactTypeEnum.Phone:
    case ContactTypeEnum.OfficePhone:
      return <FaPhone className="text-green-500" />;
    case ContactTypeEnum.Email:
      return <FaEnvelope className="text-purple-500" />;
    case ContactTypeEnum.Fax:
      return <FaFax className="text-orange-500" />;
    case ContactTypeEnum.Website:
      return <FaGlobe className="text-teal-500" />;
    case ContactTypeEnum.WhatsApp:
      return <FaWhatsapp className="text-green-600" />;
    case ContactTypeEnum.Instagram:
      return <FaInstagram className="text-pink-600" />;
    case ContactTypeEnum.Telegram:
      return <FaTelegram className="text-cyan-600" />;
    case ContactTypeEnum.LinkedIn:
      return <FaLinkedin className="text-indigo-600" />;
    case ContactTypeEnum.Address:
      return <FaMapMarkerAlt className="text-gray-600" />;
    case ContactTypeEnum.PostalCode:
      return <FaHashtag className="text-gray-500" />;
    default:
      return <FaAddressCard className="text-gray-400" />;
  }
};

const getGenderIcon = (gender?: GenderEnum | null) => {
  switch (gender) {
    case GenderEnum.Male:
      return <FaUser className="text-blue-600 text-lg" />;
    case GenderEnum.Female:
      return <FaUser className="text-pink-500 text-lg" />;
    default:
      return <FaUser className="text-gray-400 text-lg" />;
  }
};

const getGenderIconLarge = (gender?: GenderEnum | null) => {
  switch (gender) {
    case GenderEnum.Male:
      return <FaUser className="text-blue-600 text-5xl" />;
    case GenderEnum.Female:
      return <FaUser className="text-pink-500 text-5xl" />;
    default:
      return <FaUser className="text-gray-400 text-5xl" />;
  }
};

const getContactTypeBadge = (type?: ContactTypeEnum | null) => {
  switch (type) {
    case ContactTypeEnum.Mobile:
    case ContactTypeEnum.OrganizationMobile:
      return { label: "تلفن همراه", color: "bg-blue-100 text-blue-800 border-blue-300" };
    case ContactTypeEnum.Phone:
    case ContactTypeEnum.OfficePhone:
      return { label: "تلفن ثابت", color: "bg-green-100 text-green-800 border-green-300" };
    case ContactTypeEnum.Fax:
      return { label: "فکس", color: "bg-red-100 text-red-800 border-red-300" };
    case ContactTypeEnum.Email:
      return { label: "ایمیل", color: "bg-purple-100 text-purple-800 border-purple-300" };
    case ContactTypeEnum.Website:
      return { label: "وب‌سایت", color: "bg-teal-100 text-teal-800 border-teal-300" };
    case ContactTypeEnum.WhatsApp:
      return { label: "واتس‌اپ", color: "bg-yellow-100 text-yellow-800 border-yellow-300" };
    case ContactTypeEnum.Instagram:
      return { label: "اینستاگرام", color: "bg-pink-100 text-pink-800 border-pink-300" };
    case ContactTypeEnum.Telegram:
      return { label: "تلگرام", color: "bg-cyan-100 text-cyan-800 border-cyan-300" };
    case ContactTypeEnum.LinkedIn:
      return { label: "لینکدین", color: "bg-indigo-100 text-indigo-800 border-indigo-300" };
    case ContactTypeEnum.Address:
      return { label: "آدرس", color: "bg-sky-100 text-sky-800 border-sky-300" };
    case ContactTypeEnum.PostalCode:
      return { label: "کد پستی", color: "bg-lime-100 text-lime-800 border-lime-300" };
    default:
      return { label: "تماس", color: "bg-gray-100 text-gray-800 border-gray-300" };
  }
};

const getSourceBadge = (source?: ContactSourceEnum | null) => {
  switch (source) {
    case ContactSourceEnum.Personal:
      return { label: "فرد", color: "bg-purple-100 text-purple-800 border-purple-300" };
    case ContactSourceEnum.post:
       return { label: "پست", color: "bg-amber-100 text-amber-800 border-amber-300" };
    case ContactSourceEnum.location:
      return { label: "محل استقرار", color: "bg-red-100 text-red-800 border-red-300" };
    case ContactSourceEnum.employment:
      return { label: "کارمند", color: "bg-orange-100 text-orange-800  border-orange-300" };
    default:
      return { label: "نامشخص", color: "bg-gray-100 text-gray-800  border-gray-300" };
  }
};

// تابع جدید برای نمایش مالکیت (Owner)
const getOwnershipBadge = (ownership?: ContactOwnershipEnum | null) => {
  switch (ownership) {
    case ContactOwnershipEnum.Personal:
      return { label: "شخصی", color: "bg-cyan-100 text-cyan-800 border-cyan-300" };
    case ContactOwnershipEnum.Organizational:
      return { label: "سازمانی", color: "bg-sky-100 text-sky-800 border-sky-300" };
    default:
      return { label: "نامشخص", color: "bg-gray-100 text-gray-800 border-gray-300" };
  }
};

// ---------- کامپوننت ContactItem ----------
const ContactItem: React.FC<{ contact: ContactDetailDto }> = ({ contact }) => {
  const [isOpen, setIsOpen] = useState(false);

  const typeBadge = getContactTypeBadge(contact.type);
  const sourceBadge = getSourceBadge(contact.source);
  const ownershipBadge = getOwnershipBadge(contact.ownership);
  const icon = getContactIcon(contact.type);

  const relativeContacts = contact.relativeContact || [];
  const hasRelativeContacts = relativeContacts.length > 0;

  return (
    <div className="flex items-stretch gap-2 transition-all">
      {/* ---------- کارت اصلی ---------- */}
<div className="flex-1 bg-white border border-gray-200 rounded-xl p-3 shadow-sm hover:shadow-md transition-all flex items-center gap-3 relative">
  {/* آیکون نوع تماس */}
  <div className="flex-shrink-0 w-8 h-8 rounded-full bg-white border-2 border-gray-200 flex items-center justify-center shadow-sm">
    {icon}
  </div>

  {/* محتوای سمت راست: فقط برچسب‌ها و عنوان */}
  <div className="flex-1 min-w-0">
    <div className="flex flex-wrap items-center gap-1.5 mb-1">
      <span className={`text-[10px] px-2 py-0.5 rounded-full border ${typeBadge.color}`}>
        {typeBadge.label}
      </span>
      <span className={`text-[10px] px-2 py-0.5 rounded-full border ${ownershipBadge.color}`}>
        {ownershipBadge.label}
      </span>
      <span className={`text-[10px] px-2 py-0.5 rounded-full border ${sourceBadge.color}`}>
        {sourceBadge.label}
      </span>
      
      {/* {contact.isPrimary && (
        <span className="text-[10px] px-2 py-0.5 rounded-full border border-yellow-400 bg-yellow-50 text-yellow-700 font-bold">
          ★ اصلی
        </span>
      )} */}
    </div>
    <span className="text-sm text-gray-600 font-medium truncate block">
      {contact.title || 'بدون عنوان'}
    </span>
  </div>

  {/* محتوای انتهای کارت (سمت چپ): شماره تماس و دکمه زنجیر که دقیقا هم‌سطح یکدیگرند */}
  <div className="flex items-center gap-2 flex-shrink-0">
    {/* مقدار شماره اصلی */}
    <span className="font-mono text-base font-bold text-gray-800 bg-gray-100/70 px-2 py-1 rounded-md dir-ltr">
      {contact.value || '-'}
    </span>

    {/* دکمه آیکون زنجیر */}
    {hasRelativeContacts && (
      <button
        onClick={() => setIsOpen(!isOpen)}
        title={isOpen ? 'بستن اطلاعات تماس مرتبط' : 'نمایش اطلاعات تماس مرتبط'}
        className={`p-2 rounded-lg border transition-all cursor-pointer ${
          isOpen
            ? 'bg-blue-50 border-blue-300 text-blue-600 shadow-inner'
            : 'bg-gray-50 hover:bg-gray-100 border-gray-200 text-gray-400 hover:text-gray-600'
        }`}
      >
        <FaLink className="w-3.5 h-3.5" />
      </button>
    )}
  </div>
</div>

      {/* ---------- ستون شماره‌های مرتبط (سمت چپ کارت اصلی) ---------- */}
      {hasRelativeContacts && isOpen && (
        <div className="flex flex-col gap-1 w-auto animate-fadeIn">
          {relativeContacts.map((rel, idx) => (
            <div
              key={idx}
              className="flex-1 flex items-center justify-center bg-gray-100/80 border border-gray-200/80 rounded-lg px-2.5 py-1 text-xs font-mono font-semibold text-gray-600 shadow-2xs dir-ltr whitespace-nowrap"
            >
              {rel.value || '-'}
            </div>
          ))}
        </div>
      )}
    </div>
  );
};
// --- Types ---
type GroupByOption = "none" | "headOfOrganizationUnitsName" | "jobTitleName" | "locationTitle";
type SortDirection = "asc" | "desc" | null;
interface SortConfig {
  column: string;
  direction: SortDirection;
}

const SortIcon = ({ column, sortConfig }: { column: string, sortConfig: SortConfig }) => {
  if (sortConfig.column !== column) return <span className="text-gray-300 mr-1 text-[10px]">↕</span>;
  if (sortConfig.direction === "asc") return <span className="text-blue-600 mr-1 text-[10px]">▲</span>;
  if (sortConfig.direction === "desc") return <span className="text-blue-600 mr-1 text-[10px]">▼</span>;
  return <span className="text-gray-300 mr-1 text-[10px]">↕</span>;
};

export const PhoneBookPage: React.FC = () => {
  const [data, setData] = useState<PhoneBookEmploymentDto[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [globalSearch, setGlobalSearch] = useState<string>("");
  const [columnSearch, setColumnSearch] = useState<Record<string, string>>({});
  const [sortConfig, setSortConfig] = useState<SortConfig>({ column: "", direction: null });
  const [groupBy, setGroupBy] = useState<GroupByOption>("none");
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

  const toggleGroup = (groupName: string) => {
    setCollapsedGroups((prev) => {
      const next = new Set(prev);
      if (next.has(groupName)) next.delete(groupName);
      else next.add(groupName);
      return next;
    });
  };

  // const toggleRowExpand = (uniqueKey: string, hasMultiple: boolean) => {
  //   if (!hasMultiple) return;
  //   setExpandedRows((prev) => {
  //     const next = new Set(prev);
  //     if (next.has(uniqueKey)) next.delete(uniqueKey);
  //     else next.add(uniqueKey);
  //     return next;
  //   });
  // };
  const toggleRowExpand = (uniqueKey: string, hasMultiple: boolean) => {
  if (!hasMultiple) return;

  setExpandedRows((prev) => {
    const next = new Set(prev);
    const wasOpen = next.has(uniqueKey);

    if (wasOpen) {
      next.delete(uniqueKey);
    } else {
      next.add(uniqueKey);
    }

    // اگر ردیف باز شده، اسکرول را با تأخیر دو فریم انجام بده
    if (!wasOpen) {
      requestAnimationFrame(() => {
        requestAnimationFrame(() => {
          const row = document.getElementById(`row-${uniqueKey}`);
          if (row) {
            row.scrollIntoView({ behavior: 'smooth', block: 'start' });
          }
        });
      });
    }

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

  const processedData = useMemo(() => {
    let result = [...data];

    Object.entries(columnSearch).forEach(([key, term]) => {
      if (term.trim()) {
        result = result.filter((emp) => {
          const q = term.toLowerCase();
          if (key === "fullName") {
            const full = emp.fullName || `${emp.firstName || ""} ${emp.lastName || ""}`;
            return full.toLowerCase().includes(q);
          }
          const value = emp[key as keyof PhoneBookEmploymentDto];
          if (value == null) return false;
          if (Array.isArray(value)) {
            return value.some(item => typeof item === 'string' && item.toLowerCase().includes(q));
          }
          if (typeof value === 'string') {
            return value.toLowerCase().includes(q);
          }
          return false;
        });
      }
    });

    if (globalSearch.trim()) {
      const q = globalSearch.toLowerCase();
      result = result.filter((emp) => {
        const searchInString = (value?: string | null): boolean => {
          if (!value) return false;
          return value.toLowerCase().includes(q);
        };
        const searchInArray = (arr?: string[] | null): boolean => {
          if (!arr || arr.length === 0) return false;
          return arr.some(item => item && item.toLowerCase().includes(q));
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
      {/* هدر */}
      <div className="flex flex-wrap items-end justify-between gap-4 mb-6 p-4 rounded-xl border border-gray-200 shadow-sm" style={{ backgroundColor: 'rgb(0, 48, 111)' }}>
        <div className="flex items-center gap-4 mb-3">
          <img src={logo} alt="لوگو سازمان" className="h-16 md:h-20 w-auto object-contain drop-shadow-sm transition-transform duration-200 hover:scale-105" />
          <div className="h-10 md:h-12 w-[1.5px] bg-gray-300 rounded-full"></div>
          <div className="flex flex-col">
            <h1 className="font-black text-xl md:text-2xl text-white tracking-wide">سامانه جامع اطلاعات تماس همکاران</h1>
            <span className="text-xs text-blue-200 font-medium mt-0.5">دفترچه تلفن و راهنمای ارتباطات درون‌سازمانی شرکت فولاد امیرکبیر کاشان</span>
          </div>
        </div>
        <div className="flex items-center gap-4">
          <div className="flex flex-col">
            <label className="text-xs text-blue-200 mb-1">جستجوی کلی</label>
            <input type="text" placeholder="جستجو در تمام فیلدها..." value={globalSearch} onChange={(e) => setGlobalSearch(e.target.value)} className="px-4 py-2 border border-gray-300 rounded-lg text-sm w-64 focus:ring-2 focus:ring-blue-500 outline-none bg-white/90 backdrop-blur-sm" />
          </div>
          <div className="flex flex-col">
            <label className="text-xs text-blue-200 mb-1">گروه‌بندی بر اساس</label>
            <select value={groupBy} onChange={(e) => setGroupBy(e.target.value as GroupByOption)} className="px-4 py-2 border border-gray-300 rounded-lg text-sm bg-white/90 backdrop-blur-sm focus:ring-2 focus:ring-blue-500 outline-none">
              <option value="headOfOrganizationUnitsName">مدیریت</option>
              <option value="jobTitleName">عنوان شغلی</option>
              <option value="locationTitle">محل استقرار</option>
              <option value="none">بدون گروه‌بندی</option>
            </select>
          </div>
        </div>
      </div>

      {/* جدول */}
      <div className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden overflow-x-auto">
        <table className="w-full text-right border-collapse">
          <thead>
            <tr className="bg-gray-100 border-b border-gray-200 text-gray-700 text-sm">
              <th className="py-3 px-4 w-12"></th>
              <th className="py-3 px-4 w-14 text-center">تصویر</th>
              <th className="py-3 px-4 font-semibold cursor-pointer hover:bg-gray-200" onClick={() => handleSort("fullName")}>
                نام و نام خانوادگی <SortIcon column="fullName" sortConfig={sortConfig} />
              </th>
              <th className="py-3 px-4 font-semibold cursor-pointer hover:bg-gray-200" onClick={() => handleSort("headOfOrganizationUnitsName")}>
                مدیریت <SortIcon column="headOfOrganizationUnitsName" sortConfig={sortConfig} />
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
            <tr className="bg-gray-50 border-b border-gray-200">
              <th className="py-2 px-2"></th>
              <th className="py-2 px-2"></th>
              <th className="py-2 px-2 align-top">
                <input type="text" placeholder="جستجو نام..." value={columnSearch["fullName"] || ""} onChange={(e) => handleColumnSearch("fullName", e.target.value)} className="w-full mt-2 px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500" />
              </th>
              <th className="py-2 px-2 align-top">
                <input type="text" placeholder="جستجو واحد..." value={columnSearch["headOfOrganizationUnitsName"] || ""} onChange={(e) => handleColumnSearch("headOfOrganizationUnitsName", e.target.value)} className="w-full mt-2 px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500" />
              </th>
              <th className="py-2 px-2 align-top">
                <input type="text" placeholder="جستجو سمت..." value={columnSearch["jobTitleName"] || ""} onChange={(e) => handleColumnSearch("jobTitleName", e.target.value)} className="w-full mt-2 px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500" />
              </th>
              <th className="py-2 px-2 align-top">
                <input type="text" placeholder="جستجو محل..." value={columnSearch["locationTitle"] || ""} onChange={(e) => handleColumnSearch("locationTitle", e.target.value)} className="w-full mt-2 px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500" />
              </th>
              <th className="py-2 px-2 align-top">
                <input type="text" placeholder="جستجو تماس..." value={columnSearch["contactSummary"] || ""} onChange={(e) => handleColumnSearch("contactSummary", e.target.value)} className="w-full mt-2 px-2 py-1 text-xs font-normal text-gray-700 bg-white border border-gray-300 rounded focus:outline-none focus:border-blue-500" />
              </th>
            </tr>
          </thead>

          <tbody className="divide-y divide-gray-100">
            {Object.keys(processedData).length === 0 ? (
              <tr><td colSpan={7} className="text-center py-12 text-gray-500">رکوردی یافت نشد.</td></tr>
            ) : (
              Object.entries(processedData).map(([groupName, employments]) => {
                const isGroupCollapsed = collapsedGroups.has(groupName);
                return (
                  <React.Fragment key={groupName}>
                    {groupBy !== "none" && (
                      <tr className="bg-blue-50/50 hover:bg-blue-50 cursor-pointer border-t-2 border-t-blue-100" onClick={() => toggleGroup(groupName)}>
                        <td colSpan={7} className="py-3 px-4">
                          <div className="flex items-center justify-between w-full">
                            <div className="flex items-center gap-3">
                              <span className={`transform transition-transform duration-200 inline-block text-blue-600 text-xs ${isGroupCollapsed ? "rotate-90" : "rotate-0"}`}>▼</span>
                              <span className="font-bold text-gray-800">{groupName}</span>
                            </div>
                            <span className="text-xs bg-white text-blue-800 border border-blue-200 px-3 py-1 rounded-full shadow-sm">{employments.length} نفر</span>
                          </div>
                        </td>
                      </tr>
                    )}
                    {!isGroupCollapsed && employments.map((emp) => {
                      const isExpanded = expandedRows.has(emp.uniqueKey);
                      const hasMultiple = emp.contacts && emp.contacts.length > 0;

                      return (
                        <React.Fragment key={emp.uniqueKey}>
                          <tr   id={`row-${emp.uniqueKey}`}  onClick={() => toggleRowExpand(emp.uniqueKey, !!hasMultiple)} className={`transition-colors text-sm ${hasMultiple ? "cursor-pointer hover:bg-gray-50" : ""} ${isExpanded ? "bg-gray-50" : ""}`}>
                            <td className="py-3 px-4 text-center">
                              {hasMultiple ? <span className={`text-gray-400 font-bold text-[10px] inline-block transition-transform duration-200 ${isExpanded ? "rotate-[-90deg]" : "rotate-0"}`}>◀</span> : null}
                            </td>
                            <td className="py-3 px-4 text-center">
                              {emp.profilePictureUrl ? (
                                <img src={emp.profilePictureUrl} alt="پروفایل" className="w-10 h-10 rounded-full object-cover border border-gray-200" />
                              ) : (
                                <div className="w-10 h-10 rounded-full bg-gray-100 flex items-center justify-center border border-gray-200">
                                  {getGenderIcon(emp.gender)}
                                </div>
                              )}
                            </td>
                            <td className="py-3 px-4 font-medium text-gray-800">{emp.fullName || `${emp.firstName || ""} ${emp.lastName || ""}`}</td>
                            <td className="py-3 px-4 text-gray-600">{emp.headOfOrganizationUnitsName?.join(" - ") || "-"}</td>
                            <td className="py-3 px-4 text-gray-600">{emp.jobTitleName?.join(" - ") || "-"}</td>
                            <td className="py-3 px-4 text-gray-600">{emp.locationTitle?.join(" - ") || "-"}</td>
                          
                            {isExpanded ?  
                           <td className="py-3 px-4 font-mono text-gray-700 text-left dir-ltr"></td>: 
                            <td className="py-3 px-4 font-mono text-gray-700 text-left dir-ltr">
                              <div className="flex flex-wrap justify-end items-center gap-1.5 dir-ltr">
                              {emp.contacts && emp.contacts.length > 0 ? (
                                emp.contacts.map((contact, idx) => {
                                  const typeBadge = getContactTypeBadge(contact.type);
                                  return (
                                    <span
                                      key={idx}
                                      className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-md border text-xs font-medium ${typeBadge.color}`}
                                    >
                                      <span className="font-mono">{contact.value}</span>
                                      
                                      <span className="text-[10px]">{getContactIcon(contact.type)}</span>
                                    </span>
                                  );
                                })
                              ) : (
                                <span className="text-gray-400 text-sm">-</span>
                              )}
                            </div>
                          </td> 
                           }
  
                          </tr>

                          {/* زیرجدول تماس‌ها */}
                          {hasMultiple && isExpanded && (
                            <tr className="bg-gray-50">
                              <td colSpan={7} className="p-4 border-b border-gray-200">
                                <div className="bg-white border border-gray-200 rounded-lg p-4 shadow-inner">
                                  <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                                    {/* ستون اطلاعات شخصی */}
                                    <div className="md:col-span-1 flex flex-col items-center justify-center border-l border-gray-200 pl-4">
                                      <div className="w-24 h-24 rounded-full bg-gray-100 flex items-center justify-center border border-gray-300 mb-3">
                                        {emp.profilePictureUrl ? (
                                          <img src={emp.profilePictureUrl} alt="پروفایل" className="w-24 h-24 rounded-full object-cover" />
                                        ) : (
                                          <div className="w-24 h-24 rounded-full bg-gray-100 flex items-center justify-center">
                                            {getGenderIconLarge(emp.gender)}
                                          </div>
                                        )}
                                      </div>
                                      <div className="text-center">
                                        <h3 className="font-bold text-gray-800 text-base">{emp.fullName || `${emp.firstName || ""} ${emp.lastName || ""}`}</h3>
                                        <p className="text-sm text-gray-600" title="رده">{emp.jobLevelTitle?.join(" - ") || "-"}</p>
                                        <p className="text-sm text-gray-600 mt-1" title="عنوان شغلی">{emp.jobTitleName?.join(" - ") || "-"}</p>
                                        <p className="text-sm text-gray-600" title="مدیریت">{emp.headOfOrganizationUnitsName?.join(" - ") || "-"}</p>
                                        <p className="text-sm text-gray-600" title="واحد">{emp.organizationUnitsName?.join(" - ") || "-"}</p>
                                        <p className="text-sm text-gray-600" title="محل استقرار">{emp.locationTitle?.join(" - ") || "-"}</p>
                                      </div>
                                    </div>

                                    {/* ستون اطلاعات تماس */}
                                    <div className="md:col-span-2">
                                      <h4 className="text-xs font-bold text-gray-500 mb-3 border-b pb-2 flex items-center gap-2">
                                        <FaAddressCard className="text-gray-400 text-base" /> جزییات تماس
                                      </h4>
                                      <div className="space-y-3">
                                        {emp.contacts?.map((contact, index) => (
                                          <ContactItem key={index} contact={contact} />
                                        ))}
                                      </div>
                                    </div>
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