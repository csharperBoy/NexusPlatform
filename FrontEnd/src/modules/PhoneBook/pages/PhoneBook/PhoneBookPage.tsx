// src/modules/PhoneBook/pages/Post/PhoneBookPage.tsx

import React, { useEffect, useState, useMemo } from "react";
import { phonebookApi } from "../../api/PhoneBookApi";
import {
  PhoneBookEmployeeDto,
  PhoneBookContactTypeEnum,
  PhoneBookContactSourceEnum,
  ContactDetailDto,
} from "../../models/PhoneBookEmployeeDto";

// نگاشت Enumهای نوع تماس به عنوان فارسی و رنگ Badge
const getContactTypeBadge = (type?: PhoneBookContactTypeEnum | null) => {
  switch (type) {
    case PhoneBookContactTypeEnum.Mobile:
      return { label: "موبایل", color: "bg-blue-100 text-blue-800 border-blue-200" };
    case PhoneBookContactTypeEnum.Phone:
      return { label: "تلفن ثابت", color: "bg-green-100 text-green-800 border-green-200" };
    case PhoneBookContactTypeEnum.Email:
      return { label: "ایمیل", color: "bg-purple-100 text-purple-800 border-purple-200" };
    case PhoneBookContactTypeEnum.Fax:
      return { label: "فکس", color: "bg-orange-100 text-orange-800 border-orange-200" };
    case PhoneBookContactTypeEnum.Address:
      return { label: "آدرس", color: "bg-gray-100 text-gray-800 border-gray-200" };
    default:
      return { label: "تماس", color: "bg-gray-100 text-gray-700 border-gray-200" };
  }
};

type GroupByOption = "none" | "organizationUnitsName" | "jobTitleName" | "locationTitle";

export const PhoneBookPage: React.FC = () => {
  const [data, setData] = useState<PhoneBookEmployeeDto[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  // استیت‌های فیلتر و گروه‌بندی
  const [searchQuery, setSearchQuery] = useState<string>("");
  const [groupBy, setGroupBy] = useState<GroupByOption>("organizationUnitsName");

  // استیت سطرهای بازشده (Expand شده)
  const [expandedRows, setExpandedRows] = useState<Set<string>>(new Set());

  useEffect(() => {
    fetchPhoneBook();
  }, []);

  const fetchPhoneBook = async () => {
    try {
      setLoading(true);
      setError(null);
      const result = await phonebookApi.GetList();
      setData(result || []);
    } catch (err: any) {
      setError(err?.message || "خطا در دریافت اطلاعات دفترچه تلفن");
    } finally {
      setLoading(false);
    }
  };

  // مدیریت باز و بسته‌شدن سطرها
  const toggleRowExpand = (employeeCode: string, hasMultiple: boolean) => {
    if (!hasMultiple) return; // اگر فقط یک شماره دارد، باز نشود
    setExpandedRows((prev) => {
      const next = new Set(prev);
      if (next.has(employeeCode)) {
        next.delete(employeeCode);
      } else {
        next.add(employeeCode);
      }
      return next;
    });
  };

  // فیلتر و گروه‌بندی داده‌ها در Memory برای کارایی بالا
  const groupedData = useMemo(() => {
    // ۱. فیلتر سرچ
    const filtered = data.filter((emp) => {
      const q = searchQuery.trim().toLowerCase();
      if (!q) return true;
      return (
        emp.firstName?.toLowerCase().includes(q) ||
        emp.lastName?.toLowerCase().includes(q) ||
        emp.fullName?.toLowerCase().includes(q) ||
        emp.employeeCode?.includes(q) ||
        emp.organizationUnitsName?.toLowerCase().includes(q) ||
        emp.jobTitleName?.toLowerCase().includes(q) ||
        emp.contactSummary?.includes(q)
      );
    });

    // ۲. گروه‌بندی
    if (groupBy === "none") {
      return { "همه اعضا": filtered };
    }

    return filtered.reduce((groups, emp) => {
      const key = (emp[groupBy] as string) || "تعریف نشده";
      if (!groups[key]) groups[key] = [];
      groups[key].push(emp);
      return groups;
    }, {} as Record<string, PhoneBookEmployeeDto[]>);
  }, [data, searchQuery, groupBy]);

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <div className="text-gray-500 font-medium">در حال دریافت دفترچه تلفن...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="p-4 bg-red-50 text-red-700 rounded-lg border border-red-200">
        <span>{error}</span>
        <button
          onClick={fetchPhoneBook}
          className="mr-4 underline text-sm hover:text-red-900"
        >
          تلاش مجدد
        </button>
      </div>
    );
  }

  return (
    <div className="p-6 dir-rtl text-right font-sans">
      {/* هدر و اکشن‌ها */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-800">دفترچه تلفن سازمانی</h1>
          <p className="text-sm text-gray-500 mt-1">
            مجموع پرسنل: {data.length} نفر
          </p>
        </div>

        {/* فیلترها و گروه‌بندی */}
        <div className="flex flex-wrap items-center gap-3">
          {/* باکس سرچ */}
          <input
            type="text"
            placeholder="جستجوی نام، کد پرسنلی، شماره..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="px-4 py-2 border border-gray-300 rounded-lg text-sm w-64 focus:outline-none focus:ring-2 focus:ring-blue-500"
          />

          {/* انتخاب گروه‌بندی */}
          <div className="flex items-center gap-2">
            <label className="text-sm text-gray-600 whitespace-nowrap">گروه‌بندی بر اساس:</label>
            <select
              value={groupBy}
              onChange={(e) => setGroupBy(e.target.value as GroupByOption)}
              className="px-3 py-2 border border-gray-300 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="OrganizationUnitsName">واحد سازمانی</option>
              <option value="JobTitleName">عنوان شغلی</option>
              <option value="LocationTitle">محل خدمت</option>
              <option value="none">بدون گروه‌بندی</option>
            </select>
          </div>
        </div>
      </div>

      {/* نمایش لیست گروه‌بندی شده */}
      <div className="space-y-6">
        {Object.keys(groupedData).length === 0 ? (
          <div className="text-center py-12 bg-white rounded-lg border border-gray-200 text-gray-500">
            هیچ رکوردی یافت نشد.
          </div>
        ) : (
          Object.entries(groupedData).map(([groupTitle, employees]) => (
            <div key={groupTitle} className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
              {/* هدر هر گروه */}
              <div className="bg-gray-50 px-5 py-3 border-b border-gray-200 flex items-center justify-between">
                <span className="font-semibold text-gray-700 text-base">
                  {groupTitle}
                </span>
                <span className="text-xs font-medium bg-gray-200 text-gray-700 px-2.5 py-1 rounded-full">
                  {employees.length} نفر
                </span>
              </div>

              {/* جدول پرسنل این گروه */}
              <div className="overflow-x-auto">
                <table className="w-full text-right border-collapse text-sm">
                  <thead>
                    <tr className="border-b border-gray-200 bg-gray-50/50 text-gray-500 text-xs">
                      <th className="py-3 px-4 w-10"></th>
                      <th className="py-3 px-4 font-medium">کد پرسنلی</th>
                      <th className="py-3 px-4 font-medium">نام و نام خانوادگی</th>
                      <th className="py-3 px-4 font-medium">سمت شغلی</th>
                      <th className="py-3 px-4 font-medium">اطلاعات تماس</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100">
                    {employees.map((emp) => {
                      const isExpanded = expandedRows.has(emp.employeeCode);
                      const hasMultiple = emp.hasMultipleContacts ?? (emp.contacts && emp.contacts.length > 1);

                      return (
                        <React.Fragment key={emp.employeeCode}>
                          {/* سطر اصلی کارمند */}
                          <tr
                            onClick={() => toggleRowExpand(emp.employeeCode, !!hasMultiple)}
                            className={`transition-colors ${
                              hasMultiple ? "cursor-pointer hover:bg-blue-50/40" : "hover:bg-gray-50"
                            } ${isExpanded ? "bg-blue-50/30" : ""}`}
                          >
                            {/* آیکون آکاردئون */}
                            <td className="py-3 px-4 text-center">
                              {hasMultiple ? (
                                <span className="text-gray-400 font-bold text-xs inline-block transition-transform duration-200">
                                  {isExpanded ? "▼" : "◀"}
                                </span>
                              ) : null}
                            </td>

                            <td className="py-3 px-4 font-mono text-gray-600">
                              {emp.employeeCode}
                            </td>
                            <td className="py-3 px-4 font-medium text-gray-800">
                              {emp.fullName || `${emp.firstName || ""} ${emp.lastName || ""}`}
                            </td>
                            <td className="py-3 px-4 text-gray-600">
                              {emp.jobTitleName || "-"}
                            </td>
                            <td className="py-3 px-4 font-mono text-gray-700 text-left dir-ltr">
                              {emp.contactSummary || "-"}
                            </td>
                          </tr>

                          {/* سطر زیرمجموعه (Sub-Grid) در صورت چند شماره‌ای بودن و باز شدن سطر */}
                          {hasMultiple && isExpanded && (
                            <tr className="bg-gray-50/80">
                              <td colSpan={5} className="p-4 pr-12">
                                <div className="bg-white border border-gray-200 rounded-lg p-3 shadow-inner">
                                  <h4 className="text-xs font-bold text-gray-500 mb-2">
                                    جزییات راه‌های ارتباطی:
                                  </h4>
                                  <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
                                    {emp.contacts?.map((contact: ContactDetailDto, index: number) => {
                                      const badge = getContactTypeBadge(contact.type);
                                      const isOrg = contact.source === PhoneBookContactSourceEnum.Organizational;

                                      return (
                                        <div
                                          key={index}
                                          className="flex items-center justify-between p-2.5 bg-gray-50 rounded-md border border-gray-100"
                                        >
                                          <div className="flex items-center gap-2">
                                            <span
                                              className={`text-xs px-2 py-0.5 rounded border ${badge.color}`}
                                            >
                                              {badge.label}
                                            </span>
                                            <span className="text-xs text-gray-500 font-medium">
                                              {contact.title}
                                            </span>
                                          </div>

                                          <div className="flex items-center gap-2">
                                            <span className="font-mono text-sm font-semibold text-gray-800 dir-ltr">
                                              {contact.value}
                                            </span>
                                            <span
                                              className={`text-[10px] px-1.5 py-0.5 rounded ${
                                                isOrg
                                                  ? "bg-amber-50 text-amber-700 border border-amber-200"
                                                  : "bg-gray-100 text-gray-600"
                                              }`}
                                            >
                                              {isOrg ? "سازمانی" : "شخصی"}
                                            </span>
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
                  </tbody>
                </table>
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
};

export default PhoneBookPage;