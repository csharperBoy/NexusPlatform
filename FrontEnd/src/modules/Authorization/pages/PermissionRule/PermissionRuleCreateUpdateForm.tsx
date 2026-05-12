// src/modules/Authorization/page/PermissionRule/PermissionRuleCreateUpdateForm.tsx
import React from 'react';
import Card from '@/core/components/Card';
import Button from '@/core/components/Button';
import Input from '@/core/components/Input';
import { PermissionRuleFormCommand } from '../../models/PermissionRuleCommands';
import { ComparisonOperator , LogicalOperator } from '../../models/PermissionRuleEnum';
interface Props {
  formData: PermissionRuleFormCommand;
  joinDetailList: { value: string; display: string }[];
  operatorOptions: { value: ComparisonOperator; display: string }[];
  logicalOperatorOptions: { value: LogicalOperator; display: string }[];
  loading: boolean;
  error: string | null;
  handleChange: <K extends keyof PermissionRuleFormCommand>(field: K, value: PermissionRuleFormCommand[K]) => void;
  handleSubmit: (e: React.FormEvent) => void;
  isEdit: boolean;
}

export const PermissionRuleCreateUpdateForm: React.FC<Props> = ({
  formData,
  joinDetailList,
  operatorOptions,
  logicalOperatorOptions,
  loading,
  error,
  handleChange,
  handleSubmit,
  isEdit,
}) => {
  return (
    <Card className="max-w-2xl mx-auto p-6">
      <form onSubmit={handleSubmit} className="space-y-4">
        <h2 className="text-2xl font-bold text-center mb-4">
          {isEdit ? 'ویرایش قانون مجوز' : 'افزودن قانون مجوز جدید'}
        </h2>

        {error && <div className="alert alert-error">{error}</div>}

        <div>
          <label className="block mb-1">نام فیلد</label>
          <Input
            type="text"
            value={formData.fieldName ?? ''}
            onChange={(e) => handleChange('fieldName', e.target.value)}
            className="input input-bordered w-full"
            required
          />
        </div>

        <div>
          <label className="block mb-1">Join Detail</label>
          <select
            value={formData.joinDetailId ?? ''}
            onChange={(e) => handleChange('joinDetailId', e.target.value)}
            className="w-full p-2 border rounded"
            disabled={loading}
          >
            <option value="" disabled>
              انتخاب کنید...
            </option>
            {joinDetailList.map((item) => (
              <option key={item.value} value={item.value}>
                {item.display}
              </option>
            ))}
          </select>
        </div>

        <div>
          <label className="block mb-1">Operator</label>
          <select
            value={formData.operator ?? ComparisonOperator.Equal}
            onChange={(e) => handleChange('operator', e.target.value as unknown as ComparisonOperator)}
            className="w-full p-2 border rounded"
            disabled={loading}
          >
            {operatorOptions.map((op) => (
              <option key={op.value} value={op.value}>
                {op.display}
              </option>
            ))}
          </select>
        </div>

        <div>
          <label className="block mb-1">مقدار</label>
          <Input
            type="text"
            value={formData.value ?? ''}
            onChange={(e) => handleChange('value', e.target.value)}
            className="input input-bordered w-full"
          />
        </div>

        <div>
          <label className="block mb-1">Logical Operator</label>
          <select
            value={formData.logicalOperator ?? LogicalOperator.And}
            onChange={(e) => handleChange('logicalOperator', e.target.value as unknown as LogicalOperator)}
            className="w-full p-2 border rounded"
            disabled={loading}
          >
            {logicalOperatorOptions.map((op) => (
              <option key={op.value} value={op.value}>
                {op.display}
              </option>
            ))}
          </select>
        </div>

        <div>
          <label className="block mb-1">Group Order</label>
          <Input
            type="number"
            value={formData.groupOrder ?? 0}
            onChange={(e) => handleChange('groupOrder', parseInt(e.target.value, 10))}
            className="input input-bordered w-full"
          />
        </div>

        <Button type="submit" disabled={loading} className="btn btn-primary w-full">
          {loading
            ? 'در حال پردازش...'
            : isEdit
            ? 'ذخیره تغییرات'
            : 'افزودن قانون'}
        </Button>
      </form>
    </Card>
  );
};
