// src/components/PermissionCreateUpdateForm.tsx
// src/components/PermissionCreateUpdateForm.tsx
import React, { useState, useMemo } from 'react';
import { PermissionFormCommand } from '../../models/PermissionCommands';
import Card from '@/core/components/Card';
import Button from '@/core/components/Button';
import Input from '@/core/components/Input';
import { SelectionListDto } from '@/core/models/SelectionListDto';
import { PermissionRuleFormCommand } from '../../models/PermissionRuleCommands';
import { ComparisonOperator, ComparisonOperatorOptions, LogicalOperator, LogicalOperatorOptions } from '../../models/PermissionRuleEnum';
import { PermissionCreateUpdateFormProps } from '../../Interface/Permission/IPermissionCreateUpdatePage';
import { SingleSelect } from '@/core/components/Selection/SingleSelect';
import Table from '@/core/components/Table/Table';
import { ColumnDef } from '@/core/components/Table/Table.types';
import { ActionOptions, AssignTypeOptions, EffectOptions } from '../../models/PermissionEnum';

const toInputDateTime = (date?: Date | null) => {
  if (!date) return "";
  return date.toISOString().slice(0, 16);
};

export const PermissionCreateUpdateForm: React.FC<PermissionCreateUpdateFormProps> = ({
  formData,
  scopesList,
  resourceList,
  assignList,
  loading,
  error,
  isEdit,
  handleChange,
  handleScopesChange,
  handleSubmit,
  handleAssignTypeChange,
  handleAddRule,
  handleRemoveRule,
  handleRuleChange,
  fieldOptions,
  joinOptions,
  metadataLoading,
  ruleMode,
  getFieldOptionsForRule,
  handleNavigationSelect,
  handleRuleModeChange,
  selectedNav,
  useDynamicFilter,
  useNavigate,
  useScope
}) => {
  const [showRules, setShowRules] = useState<boolean>(true);

  

  // تعریف ستون‌های جدول قوانین
  const ruleColumns = useMemo<ColumnDef<PermissionRuleFormCommand>[]>(() => [
    {
      id: 'mode',
      header: 'نوع',
      type: 'custom',
      render: (_, idx) => (
        <select
          value={ruleMode[idx] || 'local'}
          onChange={(e) => handleRuleModeChange(idx, e.target.value as 'local' | 'navigated')}
          className="select select-bordered w-28"
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
            className="select select-bordered w-full"
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
            className="select select-bordered w-full"
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
          className="input input-bordered w-full"
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
          onChange={(e) => handleRuleChange(idx, 'groupOrder', parseInt(e.target.value, 10))}
          className="input input-bordered w-20"
        />
      ),
    },
  ], [
    ruleMode, handleRuleModeChange, useNavigate,
    selectedNav, handleNavigationSelect, metadataLoading, joinOptions,
    getFieldOptionsForRule, formData.rules, handleRuleChange,
    ComparisonOperatorOptions, LogicalOperatorOptions
  ]);

  return (
    <Card className="max-w-2xl mx-auto p-6">
      <form onSubmit={handleSubmit} className="p-4 space-y-4">
        <h2 className="text-2xl font-bold text-center mb-4">
          {isEdit ? "ویرایش مجوز" : "افزودن مجوز جدید"}
        </h2>

        {error && <div className="alert alert-error">{error}</div>}

        {/* منبع */}
        <SingleSelect
          options={resourceList}
          value={formData.ResourceId}
          onChange={(val) => handleChange('ResourceId', val as string)}
          label="منبع"
          disabled={loading}
          required={true}
        />

        {/* عملیات */}
        <SingleSelect
          options={ActionOptions}
          value={formData.Action}
          onChange={(val) => handleChange('Action', val as number)}
          label="عملیات"
          disabled={loading}
        />

        {/* مجاز / غیر مجاز */}
        <SingleSelect
          options={EffectOptions}
          value={formData.effect}
          onChange={(val) => handleChange('effect', val as number)}
          label="مجاز / غیر مجاز"
          disabled={loading}
        />

        {/* نوع گیرنده */}
        <SingleSelect
          options={AssignTypeOptions}
          value={formData.AssigneeType}
          onChange={(val) => handleAssignTypeChange(val as number ?? 0)}
          label="نوع"
          disabled={loading}
        />

        {/* گیرنده مجوز */}
        <SingleSelect
          options={assignList}
          value={formData.AssigneeId}
          onChange={(val) => handleChange('AssigneeId', val as string)}
          label="گیرنده مجوز"
          disabled={loading}
        />

        {/* توضیحات */}
        <div>
          <label htmlFor="Description">توضیحات:</label>
          <Input
            id="Description"
            type="text"
            value={formData.Description || ""}
            onChange={(e) => handleChange("Description", e.target.value)}
            className="input input-bordered w-full"
          />
        </div>

        {/* فعال / غیرفعال */}
        <div>
          <label htmlFor="IsActive">فعال / غیرفعال :</label>
          <Input
            id="IsActive"
            type="checkbox"
            checked={formData.IsActive ?? false}
            onChange={(e) => handleChange("IsActive", e.target.checked)}
            className="input input-bordered w-full"
          />
        </div>

        {/* تاریخ اعمال */}
        <div>
          <label htmlFor="EffectiveFrom">تاریخ اعمال:</label>
          <Input
            id="EffectiveFrom"
            type="datetime-local"
            value={toInputDateTime(formData.EffectiveFrom)}
            onChange={(e) => handleChange("EffectiveFrom", e.target.value ? new Date(e.target.value) : null)}
            className="input input-bordered w-full"
          />
        </div>

        {/* تاریخ انقضا */}
        <div>
          <label htmlFor="ExpiresAt">تاریخ انقضا:</label>
          <Input
            id="ExpiresAt"
            type="datetime-local"
            value={toInputDateTime(formData.ExpiresAt)}
            onChange={(e) => handleChange("ExpiresAt", e.target.value ? new Date(e.target.value) : null)}
            className="input input-bordered w-full"
          />
        </div>

        {/* محدوده‌ها (در صورت فعال بودن useScope) */}
        {useScope && (
          <div>
            <label className="block mb-2 font-medium">محدوده ها:</label>
            {scopesList.length === 0 && !error && <p>در حال بارگذاری محدوده ها...</p>}
            {scopesList.map(scope => (
              <div key={scope.value} className="flex items-center space-x-2 mb-1">
                <input
                  type="checkbox"
                  id={`${scope.value}`}
                  checked={formData.scopes?.includes(scope.value) || false}
                  onChange={(e) => handleScopesChange(scope.value, e.target.checked)}
                  className="mr-2"
                />
                <label htmlFor={`${scope.value}`}>{scope.display}</label>
              </div>
            ))}
          </div>
        )}

        {/* قوانین مجوز (در صورت فعال بودن useDynamicFilter) */}
        {useDynamicFilter && (
          <>
            <div className="flex items-center justify-between mt-6 mb-2">
              <label className="font-medium">قوانین مجوز</label>
              <label className="flex items-center space-x-2">
                <input
                  type="checkbox"
                  checked={showRules}
                  onChange={e => setShowRules(e.target.checked)}
                  className="checkbox"
                />
                <span>نمایش جدول</span>
              </label>
            </div>

            {showRules && (
              <>
                <Table<PermissionRuleFormCommand>
                  data={formData.rules || []}
                  columns={ruleColumns}
                  keyExtractor={(row, idx) => idx}
                  onDelete={(row, idx) => handleRemoveRule(idx)}
                  pageSize={10}
                  emptyMessage="هیچ قانونی اضافه نشده است"
                />
                <Button type="button" color="primary" onClick={handleAddRule} className="mt-2">
                  افزودن قانون جدید
                </Button>
              </>
            )}
          </>
        )}

        {/* دکمه ثبت */}
        <Button
          type="submit"
          disabled={loading}
          className="btn btn-primary w-full"
        >
          {loading ? "در حال پردازش..." : (isEdit ? "ذخیره تغییرات" : "افزودن مجوز")}
        </Button>
      </form>
    </Card>
  );
};