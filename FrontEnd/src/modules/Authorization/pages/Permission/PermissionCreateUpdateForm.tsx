// src/components/PermissionCreateUpdateForm.tsx
import React, { useState } from 'react';
import { CreatePermissionCommand, PermissionFormCommand } from '../../models/PermissionCommands'; 
import Card from '@/core/components/Card';
import Button from '@/core/components/Button';
import Input from '@/core/components/Input';
import { data } from 'react-router-dom';
import { SelectionListDto } from '@/core/models/SelectionListDto';
import { PermissionRuleFormCommand } from '../../models/PermissionRuleCommands';
import { ComparisonOperator, LogicalOperator } from '../../models/PermissionRuleEnum';
import { PermissionCreateUpdateFormProps } from '../../Interface/Permission/IPermissionCreateUpdatePage';

const operatorOptions: { value: ComparisonOperator; display: string }[] = [
  { value: ComparisonOperator.Equal, display: 'Equal' },
  { value: ComparisonOperator.GreaterThan, display: 'GreaterThan' },
  { value: ComparisonOperator.LessThan, display: 'LessThan' },
  { value: ComparisonOperator.NotEqual, display: 'NotEqual' },
];

const logicalOperatorOptions: { value: LogicalOperator; display: string }[] = [
  { value: LogicalOperator.And, display: 'AND' },
  { value: LogicalOperator.Or, display: 'OR' },
];
const toInputDateTime = (date?: Date | null) => {
  if (!date) return "";
  // toISOString() already returns UTC. Slice to "YYYY-MM-DDTHH:mm"
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
  selectedNav
}) => {
   const [showRules, setShowRules] = useState<boolean>(true);

  return (
    
        <Card className="max-w-2xl mx-auto p-6">
          <form onSubmit={handleSubmit} className="p-4 space-y-4">
            <h2 className="text-2xl font-bold text-center mb-4">
              {isEdit ? "ویرایش مجوز" : "افزودن مجوز جدید"}
            </h2>

            {error && <div className="alert alert-error">{error}</div>}

            {/* ----- فیلدهای فرم ----- */}
            <div className="mb-4">
              <label className="block mb-1">منبع</label>
              <select
                value={formData.ResourceId}
                onChange={(e) => handleChange("ResourceId", e.target.value)}
                className="w-full p-2 border rounded"
                disabled={loading}
                required
              >
                <option value="" disabled>
                  انتخاب کنید...
                </option>
                {resourceList.map((item) => (
                  <option key={item.value} value={item.value}>
                    {item.display}
                  </option>
                ))}
              </select>
            </div>


       <div className="mb-4">
              <label className="block mb-1">عملیات</label>
              <select
                value={formData.Action}
                onChange={(e) => handleChange('Action',Number(e.target.value))}
                className="w-full p-2 border rounded"
                disabled={loading}
              >
          
                <option value='0' label="View">مشاهده (View)</option>
                <option value='1' label="Create">ایجاد (Create)</option>
                <option value='2' label="Edit">ویرایش (Edit)</option>
                
                <option value='3' label="Delete">حذف (Delete)</option>
                
                <option value='4' label="Export">خروجی (Export)</option>
                
                <option value='99' label="Full">کامل (Full)</option>
              </select>
       </div>
       <div className="mb-4">
              <label className="block mb-1">مجاز / غیر مجاز</label>
              <select
                value={formData.effect}
                onChange={(e) => handleChange('effect',Number(e.target.value))}
                className="w-full p-2 border rounded"
                disabled={loading}
              >
                <option value='0' label="allow">مجاز (Module)</option>
                <option value='1' label="Deny">غیر مجاز (Ui)</option>
              </select>
       </div>
      <div className="mb-4">
              <label className="block mb-1">نوع</label>
              <select
                value={formData.AssigneeType}
                onChange={(e) => handleAssignTypeChange(Number(e.target.value))}
                className="w-full p-2 border rounded"
                disabled={loading}
              >
                <option value='0' label="Person">شخص (Person)</option>
                <option value='1' label="Position">موقعیت (Position)</option>
                <option value='2' label="Role">نقش (Role)</option>
                <option value='3' label="User">کاربر (User)</option>
              </select>
       </div>
       <div className="mb-4">
              <label className="block mb-1">AssigneeId</label>
              <select
                value={formData.AssigneeId}
                onChange={(e) => handleChange('AssigneeId',e.target.value)}
                className="w-full p-2 border rounded"
                disabled={loading}
                required={true}
              >
          
                 <option value="" disabled>انتخاب کنید...</option>
                  {assignList.map((item) => (
                    <option key={item.value} value={item.value}>
                      {item.display}
                    </option>
                  ))}
                
              </select>
       </div>
      
 

 <div>
        <label htmlFor="Description"> توضیحات:</label>
        <Input
          id="Description"
          type="text"
          value={formData.Description || ""}
          onChange={(e) => handleChange("Description", e.target.value)}
          className="input input-bordered w-full"
        />
      </div>
      

       
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

      
      <div>
        <label htmlFor="EffectiveFrom">تاریخ اعمال:</label>
        <Input
          id="EffectiveFrom"
          type="datetime-local"
          value={toInputDateTime(formData.EffectiveFrom)}
          onChange={(e) =>
            handleChange(
              "EffectiveFrom",
              e.target.value ? new Date(e.target.value) : null
            )
          }
          className="input input-bordered w-full"
        />
      </div>

      <div>
        <label htmlFor="ExpiresAt">تاریخ انقضا:</label>
        <Input
          id="ExpiresAt"
          type="datetime-local"
          value={toInputDateTime(formData.ExpiresAt)}
          onChange={(e) =>
            handleChange(
              "ExpiresAt",
              e.target.value ? new Date(e.target.value) : null
            )
          }
          className="input input-bordered w-full"
        />
      </div>

      
      
      {/* بخش انتخاب نقش‌ها */}
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
{/* ===== Rules section ===== */}
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
            {formData.rules && formData.rules.length > 0 ? (
              <div className="overflow-x-auto">
                <table className="table-auto w-full border-collapse border border-gray-300">
                  <thead className="bg-gray-100">
                    <tr>
                      <th className="border p-2">نوع</th>
                      <th className="border p-2">ناویگیشن</th>
                      <th className="border p-2">فیلد</th>
                      <th className="border p-2">عملگر</th>
                      <th className="border p-2">مقدار</th>
                      <th className="border p-2">عملگر منطقی</th>
                      <th className="border p-2">ترتیب گروه</th>
                      <th className="border p-2">عملیات</th>
                    </tr>
                  </thead>
                  <tbody>
                    {formData.rules.map((rule, idx) => {
                      const currentMode = ruleMode[idx] || 'local';
                      const selectedNavValue = selectedNav[idx] || '';
                      const fieldOpts = getFieldOptionsForRule(idx);
                      const isFieldDisabled = (currentMode === 'navigated' && !selectedNavValue) || metadataLoading;

                      return (
                        <tr key={idx} className="border-b">
                          {/* ستون نوع */}
                          <td className="border p-2">
                            <select
                              value={currentMode}
                              onChange={(e) => handleRuleModeChange(idx, e.target.value as 'local' | 'navigated')}
                              className="select select-bordered w-28"
                            >
                              <option value="local">بدون جوین</option>
                              <option value="navigated">با جوین</option>
                            </select>
                          </td>

                          {/* ستون ناویگیشن (فقط در حالت navigated) */}
                          <td className="border p-2">
                            {currentMode === 'navigated' ? (
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
                            ) : (
                              <span className="text-gray-400 text-sm">—</span>
                            )}
                          </td>

                          {/* ستون فیلد */}
                          <td className="border p-2">
                            <select
                              value={rule.fieldName || ''}
                              onChange={(e) => handleRuleChange(idx, 'fieldName', e.target.value)}
                              className="select select-bordered w-full"
                              disabled={isFieldDisabled}
                            >
                              <option value="">انتخاب فیلد...</option>
                              {fieldOpts.map((opt) => (
                                <option key={opt.value} value={opt.value}>{opt.label}</option>
                              ))}
                            </select>
                          </td>

                          {/* ستون عملگر */}
                          <td className="border p-2">
                            <select
                              value={rule.operator ?? ComparisonOperator.Equal}
                              onChange={(e) => handleRuleChange(idx, 'operator', Number(e.target.value))}
                              className="select select-bordered w-full"
                            >
                              {operatorOptions.map((opt) => (
                                <option key={opt.value} value={opt.value}>{opt.display}</option>
                              ))}
                            </select>
                          </td>

                          {/* ستون مقدار */}
                          <td className="border p-2">
                            <Input
                              value={rule.value ?? ''}
                              onChange={(e) => handleRuleChange(idx, 'value', e.target.value)}
                              className="input input-bordered w-full"
                            />
                          </td>

                          {/* ستون عملگر منطقی */}
                          <td className="border p-2">
                            <select
                              value={rule.logicalOperator ?? LogicalOperator.And}
                              onChange={(e) => handleRuleChange(idx, 'logicalOperator', Number(e.target.value))}
                              className="select select-bordered w-full"
                            >
                              {logicalOperatorOptions.map((opt) => (
                                <option key={opt.value} value={opt.value}>{opt.display}</option>
                              ))}
                            </select>
                          </td>

                          {/* ستون ترتیب گروه */}
                          <td className="border p-2">
                            <Input
                              type="number"
                              value={rule.groupOrder ?? 0}
                              onChange={(e) => handleRuleChange(idx, 'groupOrder', parseInt(e.target.value, 10))}
                              className="input input-bordered w-20"
                            />
                          </td>

                          {/* ستون عملیات (حذف) */}
                          <td className="border p-2 text-center">
                            <Button type="button" color="error" size="sm" onClick={() => handleRemoveRule(idx)}>
                              حذف
                            </Button>
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
            ) : (
              <p className="text-center text-gray-500 py-4">هیچ قانونی اضافه نشده است.</p>
            )}
            <Button type="button" color="primary" onClick={handleAddRule} className="mt-2">
              افزودن قانون جدید
            </Button>
          </>
        )}
      {/* ۴. دکمه‌ی submit */}
            <Button
              type="submit"
              disabled={loading}
              className="btn btn-primary w-full"
            >
              {loading
                ? "در حال پردازش..."
                : isEdit
                ? "ذخیره تغییرات"
                : "افزودن مجوز"}
            </Button>
          </form>
        </Card>
     
  );
};