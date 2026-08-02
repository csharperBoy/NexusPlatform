import React from 'react';
import { usePermissionManagement } from '../../hooks/Forms/Permission/usePermissionManagement';
import {
  AssignTypeDisplayMap,
  ActionDisplayMap,
  EffectDisplayMap,
  AssignTypeOptions,
  ActionOptions,
  EffectOptions,
  AssignType,
  Action,
  Effect,
} from '../../models/PermissionEnum';
import { Plus, Edit3, Trash2, Search, RotateCcw, ShieldAlert } from 'lucide-react';

export const PermissionsManagementPage: React.FC = () => {
  const {
    permissions,
    loading,
    filters,
    resourcesList,
    deleteConfirmId,
    deletingId,
    setDeleteConfirmId,
    handleFilterChange,
    handleApplyFilter,
    handleResetFilter,
    handleNavigateToCreate,
    handleNavigateToEdit,
    handleDelete,
    getAssigneeName,
    getResourceName,
  } = usePermissionManagement();

  return (
    <div className="w-full space-y-6 p-6 dir-rtl text-right">
      {/* هدر صفحه و دکمه افزودن */}
      <div className="flex flex-wrap items-center justify-between gap-4 border-b pb-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-800 dark:text-white">مدیریت مجوزها</h1>
          <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">
            مشاهده و مدیریت سطح دسترسی‌ها و مجوزهای تعریف شده در سیستم
          </p>
        </div>
        <button
          onClick={() => handleNavigateToCreate()}
          className="inline-flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-blue-700"
        >
          <Plus className="h-4 w-4" />
          افزودن مجوز جدید
        </button>
      </div>

      {/* بخش فیلترها */}
      <div className="rounded-xl border border-gray-200 bg-white p-4 shadow-sm dark:border-gray-700 dark:bg-gray-800">
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 md:grid-cols-4">
          {/* نوع دریافت کننده */}
          <div>
            <label className="mb-1 block text-xs font-medium text-gray-700 dark:text-gray-300">
              نوع دریافت‌کننده
            </label>
            <select
              value={filters.assigneeType !== null && filters.assigneeType !== undefined ? filters.assigneeType : ''}
              onChange={(e) =>
                handleFilterChange(
                  'assigneeType',
                  e.target.value !== '' ? Number(e.target.value) : null
                )
              }
              className="w-full rounded-lg border border-gray-300 bg-gray-50 p-2.5 text-sm text-gray-900 focus:border-blue-500 focus:ring-blue-500 dark:border-gray-600 dark:bg-gray-700 dark:text-white"
            >
              <option value="">همه</option>
              {AssignTypeOptions.map((opt) => (
                <option key={opt.value} value={opt.value}>
                  {opt.display || opt.label}
                </option>
              ))}
            </select>
          </div>

          {/* منبع (Resource) */}
          <div>
            <label className="mb-1 block text-xs font-medium text-gray-700 dark:text-gray-300">
              منبع (Resource)
            </label>
            <select
              value={filters.resourceId || ''}
              onChange={(e) => handleFilterChange('resourceId', e.target.value || null)}
              className="w-full rounded-lg border border-gray-300 bg-gray-50 p-2.5 text-sm text-gray-900 focus:border-blue-500 focus:ring-blue-500 dark:border-gray-600 dark:bg-gray-700 dark:text-white"
            >
              <option value="">همه منابع</option>
              {resourcesList.map((res) => (
                <option key={res.value} value={res.value}>
                  {res.display || res.label}
                </option>
              ))}
            </select>
          </div>

          {/* توضیحات */}
          <div>
            <label className="mb-1 block text-xs font-medium text-gray-700 dark:text-gray-300">
              جستجو در توضیحات
            </label>
            <input
              type="text"
              placeholder="توضیحات..."
              value={filters.description || ''}
              onChange={(e) => handleFilterChange('description', e.target.value)}
              className="w-full rounded-lg border border-gray-300 bg-gray-50 p-2.5 text-sm text-gray-900 focus:border-blue-500 focus:ring-blue-500 dark:border-gray-600 dark:bg-gray-700 dark:text-white"
            />
          </div>

          {/* دکمه‌های اعمال/بازنشانی */}
          <div className="flex items-end gap-2">
            <button
              onClick={handleApplyFilter}
              className="inline-flex flex-1 items-center justify-center gap-1.5 rounded-lg bg-gray-800 px-3 py-2.5 text-sm font-medium text-white transition-colors hover:bg-gray-900 dark:bg-gray-700 dark:hover:bg-gray-600"
            >
              <Search className="h-4 w-4" />
              فیلتر
            </button>
            <button
              onClick={handleResetFilter}
              className="inline-flex items-center justify-center rounded-lg border border-gray-300 bg-white p-2.5 text-gray-700 transition-colors hover:bg-gray-100 dark:border-gray-600 dark:bg-gray-800 dark:text-gray-200 dark:hover:bg-gray-700"
              title="بازنشانی"
            >
              <RotateCcw className="h-4 w-4" />
            </button>
          </div>
        </div>
      </div>

      {/* جدول مجوزها - بدون overflow-x-auto جهت اسکرول طبیعی صفحه */}
      <div className="rounded-xl border border-gray-200 bg-white shadow-sm dark:border-gray-700 dark:bg-gray-800">
        <table className="w-full text-right text-sm text-gray-600 dark:text-gray-300">
          <thead className="bg-gray-50 text-xs font-semibold uppercase text-gray-700 dark:bg-gray-700/50 dark:text-gray-300">
            <tr>
              <th className="px-4 py-3">نوع دریافت‌کننده</th>
              <th className="px-4 py-3">دریافت‌کننده</th>
              <th className="px-4 py-3">منبع</th>
              <th className="px-4 py-3">عملیات (Action)</th>
              <th className="px-4 py-3">وضعیت (Effect)</th>
              <th className="px-4 py-3">توضیحات</th>
              <th className="px-4 py-3 text-center">عملیات</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
            {loading ? (
              <tr>
                <td colSpan={7} className="py-8 text-center text-gray-500">
                  در حال دریافت اطلاعات...
                </td>
              </tr>
            ) : permissions.length === 0 ? (
              <tr>
                <td colSpan={7} className="py-8 text-center text-gray-500">
                  هیچ مجوزی یافت نشد.
                </td>
              </tr>
            ) : (
              permissions.map((item) => {
                const assigneeTypeName =
                  AssignTypeDisplayMap[AssignType[item.assigneeType] as keyof typeof AssignType] ||
                  '-';
                const actionName =
                  ActionDisplayMap[Action[item.action] as keyof typeof Action] || '-';
                const isAllow = item.effect === Effect.allow;

                return (
                  <tr
                    key={item.id}
                    className="transition-colors hover:bg-gray-50 dark:hover:bg-gray-700/30"
                  >
                    {/* نوع دریافت‌کننده */}
                    <td className="px-4 py-3 font-medium text-gray-900 dark:text-white">
                      <span className="rounded bg-gray-100 px-2 py-1 text-xs dark:bg-gray-700">
                        {assigneeTypeName}
                      </span>
                    </td>

                    {/* عنوان دریافت‌کننده */}
                    <td className="px-4 py-3 font-medium text-gray-800 dark:text-gray-200">
                      {getAssigneeName(item.assigneeId)}
                    </td>

                    {/* منبع */}
                    <td className="px-4 py-3 text-gray-700 dark:text-gray-300">
                      {getResourceName(item.resourceId, item.resourceKey)}
                    </td>

                    {/* نوع دسترسی / اکشن */}
                    <td className="px-4 py-3">
                      <span className="rounded bg-blue-50 px-2 py-1 text-xs font-semibold text-blue-700 dark:bg-blue-900/30 dark:text-blue-300">
                        {actionName}
                      </span>
                    </td>

                    {/* وضعیت (مجاز / غیرمجاز) */}
                    <td className="px-4 py-3">
                      <span
                        className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${
                          isAllow
                            ? 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-300'
                            : 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-300'
                        }`}
                      >
                        {EffectDisplayMap[Effect[item.effect] as keyof typeof Effect] ||
                          (isAllow ? 'دسترسی' : 'عدم دسترسی')}
                      </span>
                    </td>

                    {/* توضیحات */}
                    <td className="max-w-xs truncate px-4 py-3 text-gray-500 dark:text-gray-400">
                      {item.description || '-'}
                    </td>

                    {/* کلیدهای ویرایش و حذف */}
                    <td className="px-4 py-3 text-center">
                      <div className="flex items-center justify-center gap-2">
                        <button
                          onClick={() => handleNavigateToEdit(item.id)}
                          className="rounded p-1.5 text-blue-600 transition-colors hover:bg-blue-50 hover:text-blue-800 dark:text-blue-400 dark:hover:bg-gray-700"
                          title="ویرایش مجوز"
                        >
                          <Edit3 className="h-4 w-4" />
                        </button>

                        <button
                          onClick={() => setDeleteConfirmId(item.id)}
                          className="rounded p-1.5 text-red-600 transition-colors hover:bg-red-50 hover:text-red-800 dark:text-red-400 dark:hover:bg-gray-700"
                          title="حذف مجوز"
                        >
                          <Trash2 className="h-4 w-4" />
                        </button>
                      </div>
                    </td>
                  </tr>
                );
              })
            )}
          </tbody>
        </table>
      </div>

      {/* مودال تایید حذف */}
      {deleteConfirmId && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
          <div className="w-full max-w-md rounded-xl bg-white p-6 shadow-xl dark:bg-gray-800">
            <div className="flex items-center gap-3 text-red-600">
              <ShieldAlert className="h-6 w-6" />
              <h3 className="text-lg font-bold">تایید حذف مجوز</h3>
            </div>
            <p className="mt-3 text-sm text-gray-600 dark:text-gray-300">
              آیا از حذف این مجوز اطمینان دارید؟ این عملیات قابل بازگشت نیست.
            </p>

            <div className="mt-6 flex justify-end gap-3">
              <button
                onClick={() => setDeleteConfirmId(null)}
                className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 transition-colors hover:bg-gray-50 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-700"
              >
                انصراف
              </button>
              <button
                onClick={handleDelete}
                disabled={deletingId === deleteConfirmId}
                className="rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-red-700 disabled:opacity-50"
              >
                {deletingId === deleteConfirmId ? 'در حال حذف...' : 'حذف مجوز'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default PermissionsManagementPage;