// src/modules/Authorization/components/PermissionCreateUpdate.tsx
import React, { useState, useMemo } from 'react';
import Card from '@/core/components/Card';
import Button from '@/core/components/Button';
import Input from '@/core/components/Input';
import Table from '@/core/components/Table/Table';
import { ColumnDef } from '@/core/components/Table/Table.types';
import { SingleSelect } from '@/core/components/Selection/SingleSelect';
import { usePermissionCreateUpdate } from '../../hooks/Forms/Permission/usePermissionCreateUpdate';
import { PermissionRuleFormCommand } from '../../models/PermissionRuleCommands';
import { ComparisonOperatorOptions, LogicalOperatorOptions } from '../../models/PermissionRuleEnum';
import { ActionOptions, AssignTypeOptions, EffectOptions } from '../../models/PermissionEnum';

interface PermissionCreateUpdateProps {
  permissionId?: string;
  onSuccess?: () => void;
}

const toInputDateTime = (date?: Date | null) => {
  if (!date) return "";
  const d = new Date(date);
  return isNaN(d.getTime()) ? "" : d.toISOString().slice(0, 16);
};

export const PermissionCreateUpdate: React.FC<PermissionCreateUpdateProps> = ({
  permissionId,
  onSuccess,
}) => {
  const {
    formData,
    scopesList,
    resourceList,
    assignList,
    loading,
    metadataLoading,
    error,
    isEdit,
    useDynamicFilter,
    useNavigate,
    useScope,
    ruleMode,
    selectedNav,
    joinOptions,
    handleChange,
    handleAssignTypeChange,
    handleScopesChange,
    handleAddRule,
    handleRemoveRule,
    handleRuleChange,
    handleRuleModeChange,
    handleNavigationSelect,
    getFieldOptionsForRule,
    handleSubmit,
  } = usePermissionCreateUpdate(permissionId, onSuccess);

  const [showRulesTable, setShowRulesTable] = useState<boolean>(true);

  // تعریف ستون‌های جدول قوانین
  const ruleColumns = useMemo<ColumnDef<PermissionRuleFormCommand>[]>(() => [
    {
      id: 'mode',
      header: 'نوع جوین',
      type: 'custom',
      render: (_, idx) => (
        <select
          value={ruleMode[idx] || 'local'}
          onChange={(e) => handleRuleModeChange(idx, e.target.value as 'local' | 'navigated')}
          className="select select-bordered w-28 text-sm"
        >
          <option value="local">بدون جوین</option>
          {useNavigate && <option value="navigated">با جوین</option>}
        </select>
      ),
    },
    {
      id: 'navigation',
      header: 'ناویگیشن',
      type: 'custom',
      render: (_, idx) => {
        const mode = ruleMode[idx] || 'local';
        if (mode !== 'navigated') return <span className="text-gray-400 text-sm">—</span>;
        const selectedNavValue = selectedNav[idx] || '';
        return (
          <select
            value={selectedNavValue}
            onChange={(e) => handleNavigationSelect(idx, e.target.value)}
            className="select select-bordered w-full text-sm"
            disabled={metadataLoading}
          >
            <option value="">انتخاب ناویگیشن...</option>
            {metadataLoading ? (
              <option disabled>در حال بارگذاری...</option>
            ) : joinOptions.length === 0 ? (
              <option disabled>هیچ ناویگیشنی موجود نیست</option>
            ) : (
              joinOptions.map((opt) => (
                <option key={opt.value} value={opt.value}>{opt.label}</option>
              ))
            )}
          </select>
        );
      },
    },
    {
      id: 'field',
      header: 'فیلد',
      type: 'custom',
      render: (_, idx) => {
        const mode = ruleMode[idx] || 'local';
        const selectedNavValue = selectedNav[idx] || '';
        const isDisabled = (mode === 'navigated' && !selectedNavValue) || metadataLoading;
        const fieldOpts = getFieldOptionsForRule(idx);
        return (
          <select
            value={formData.rules?.[idx]?.fieldName || ''}
            onChange={(e) => handleRuleChange(idx, 'fieldName', e.target.value)}
            className="select select-bordered w-full text-sm"
            disabled={isDisabled}
          >
            <option value="">انتخاب فیلد...</option>
            {fieldOpts.map((opt) => (
              <option key={opt.value} value={opt.value}>{opt.label}</option>
            ))}
          </select>
        );
      },
    },
    {
      id: 'operator',
      header: 'عملگر',
      type: 'select',
      options: ComparisonOperatorOptions,
      accessor: (row) => row.operator,
      onCellChange: (row, newValue, idx) => handleRuleChange(idx, 'operator', newValue),
    },
    {
      id: 'value',
      header: 'مقدار',
      type: 'custom',
      render: (row, idx) => (
        <Input
          value={row.value ?? ''}
          onChange={(e) => handleRuleChange(idx, 'value', e.target.value)}
          className="input input-bordered w-full text-sm"
        />
      ),
    },
    {
      id: 'logicalOperator',
      header: 'عملگر منطقی',
      type: 'select',
      options: LogicalOperatorOptions,
      accessor: (row) => row.logicalOperator,
      onCellChange: (row, newValue, idx) => handleRuleChange(idx, 'logicalOperator', newValue),
    },
    {
      id: 'groupOrder',
      header: 'ترتیب گروه',
      type: 'custom',
      render: (row, idx) => (
        <Input
          type="number"
          value={row.groupOrder ?? 0}
          onChange={(e) => handleRuleChange(idx, 'groupOrder', parseInt(e.target.value, 10) || 0)}
          className="input input-bordered w-20 text-sm"
        />
      ),
    },
  ], [
    ruleMode,
    handleRuleModeChange,
    useNavigate,
    selectedNav,
    handleNavigationSelect,
    metadataLoading,
    joinOptions,
    getFieldOptionsForRule,
    formData.rules,
    handleRuleChange,
  ]);

  return (
    <Card className="max-w-4xl mx-auto p-6 shadow-lg rounded-xl">
      <form onSubmit={handleSubmit} className="space-y-6">
        <h2 className="text-2xl font-bold text-gray-800 border-b pb-3 text-center">
          {isEdit ? "ویرایش مجوز" : "افزودن مجوز جدید"}
        </h2>

        {error && (
          <div className="alert alert-error text-sm font-medium shadow-sm">
            <span>{error}</span>
          </div>
        )}

        {/* ۱. بخش اطلاعات پایه */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <SingleSelect
            options={resourceList}
            value={formData.resourceId}
            onChange={(val) => handleChange('resourceId', val as string)}
            label="منبع (Resource)"
            disabled={loading}
            required={true}
          />

          <SingleSelect
            options={ActionOptions}
            value={formData.action}
            onChange={(val) => handleChange('action', val as number)}
            label="عملیات (Action)"
            disabled={loading}
          />

          <SingleSelect
            options={EffectOptions}
            value={formData.effect}
            onChange={(val) => handleChange('effect', val as number)}
            label="وضعیت دسترسی (Effect)"
            disabled={loading}
          />
        </div>

        {/* ۲. بخش تخصیص (Assignee) */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 bg-gray-50 p-4 rounded-lg border border-gray-100">
          <SingleSelect
            options={AssignTypeOptions}
            value={formData.assigneeType}
            onChange={(val) => handleAssignTypeChange(val as number)}
            label="نوع گیرنده مجوز"
            disabled={loading}
          />

          <SingleSelect
            options={assignList}
            value={formData.assigneeId}
            onChange={(val) => handleChange('assigneeId', val as string)}
            label="گیرنده مجوز"
            disabled={loading || formData.assigneeType === undefined}
          />
        </div>

        {/* ۳. بخش تاریخ و توضیحات */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">تاریخ شروع اثر:</label>
            <Input
              type="datetime-local"
              value={toInputDateTime(formData.effectiveFrom)}
              onChange={(e) => handleChange("effectiveFrom", e.target.value ? new Date(e.target.value) : null)}
              className="input input-bordered w-full"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">تاریخ انقضا:</label>
            <Input
              type="datetime-local"
              value={toInputDateTime(formData.expiresAt)}
              onChange={(e) => handleChange("expiresAt", e.target.value ? new Date(e.target.value) : null)}
              className="input input-bordered w-full"
            />
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">توضیحات:</label>
          <Input
            type="text"
            value={formData.description || ""}
            onChange={(e) => handleChange("description", e.target.value)}
            className="input input-bordered w-full"
            placeholder="توضیحات مربوط به این دسترسی..."
          />
        </div>

        <div className="flex items-center space-x-2 space-x-reverse">
          <input
            id="isActive"
            type="checkbox"
            checked={formData.isActive ?? false}
            onChange={(e) => handleChange("isActive", e.target.checked)}
            className="checkbox checkbox-primary"
          />
          <label htmlFor="isActive" className="text-sm font-medium cursor-pointer">
            مجوز فعال باشد
          </label>
        </div>

        {/* ۴. بخش محدوده‌ها (Scopes) */}
        {useScope && (
          <div className="border-t pt-4">
            <label className="block mb-3 font-semibold text-gray-800">محدوده‌ها (Scopes):</label>
            {scopesList.length === 0 ? (
              <p className="text-sm text-gray-500">در حال بارگذاری محدوده‌ها...</p>
            ) : (
              <div className="grid grid-cols-2 md:grid-cols-3 gap-3 bg-gray-50 p-3 rounded-md">
                {scopesList.map((scope) => (
                  <label key={scope.value} className="flex items-center space-x-2 space-x-reverse cursor-pointer">
                    <input
                      type="checkbox"
                      checked={formData.scopes?.includes(scope.value) || false}
                      onChange={(e) => handleScopesChange(scope.value, e.target.checked)}
                      className="checkbox checkbox-sm checkbox-secondary"
                    />
                    <span className="text-sm text-gray-700">{scope.display}</span>
                  </label>
                ))}
              </div>
            )}
          </div>
        )}

        {/* ۵. بخش قوانین فیلتر پویا (Dynamic Rules) */}
        {useDynamicFilter && (
          <div className="border-t pt-4 space-y-3">
            <div className="flex items-center justify-between">
              <h3 className="font-semibold text-gray-800">قوانین فیلتر پویا</h3>
              <label className="flex items-center space-x-2 space-x-reverse cursor-pointer text-sm">
                <input
                  type="checkbox"
                  checked={showRulesTable}
                  onChange={(e) => setShowRulesTable(e.target.checked)}
                  className="checkbox checkbox-sm"
                />
                <span>نمایش جدول قوانین</span>
              </label>
            </div>

            {showRulesTable && (
              <div className="space-y-3">
                <Table<PermissionRuleFormCommand>
                  data={formData.rules || []}
                  columns={ruleColumns}
                  keyExtractor={(_, idx) => idx}
                  onDelete={(_, idx) => handleRemoveRule(idx)}
                  pageSize={10}
                  emptyMessage="هیچ قانونی تعریف نشده است"
                />
                <Button
                  type="button"
                  color="secondary"
                  onClick={handleAddRule}
                  className="btn btn-outline btn-sm"
                >
                  + افزودن قانون جدید
                </Button>
              </div>
            )}
          </div>
        )}

        {/* دکمه عملیات ثبت */}
        <div className="pt-4 border-t">
          <Button
            type="submit"
            disabled={loading}
            className="btn btn-primary w-full text-white font-bold"
          >
            {loading ? "در حال ثبت اطلاعات..." : isEdit ? "ذخیره تغییرات" : "ثبت مجوز جدید"}
          </Button>
        </div>
      </form>
    </Card>
  );
};