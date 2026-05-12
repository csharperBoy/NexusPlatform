// src/modules/Authorization/hooks/PermissionRule/usePermissionRuleCreateUpdateForm.ts
import { useState, useEffect } from 'react';
import { PermissionRuleFormCommand, CreatePermissionRuleCommand, UpdatePermissionRuleCommand } from '@/modules/Authorization/models/PermissionRuleCommands';
import { permissionRuleApi } from '@/modules/Authorization/api/PermissionRuleApi';
import { JoinDetailApi } from '@/modules/Authorization/api/JoinDetailApi';
import { ComparisonOperator , LogicalOperator } from '@/modules/Authorization/models/PermissionRuleEnum';
import { useParams } from 'react-router-dom';

export const usePermissionRuleCreateUpdateForm = (
  permissionRuleId?: string,
  onSuccess?: () => void
) => {
      const { permissionId } = useParams<{ permissionId: string }>();
  const [formData, setFormData] = useState<PermissionRuleFormCommand>({
    fieldName: '',
    joinDetailId: '',
    operator: ComparisonOperator.Equal,
    value: '',
    logicalOperator: LogicalOperator.And,
    groupOrder: 0,
    id : permissionRuleId || '',
    permissionId: permissionId
  });

  const [joinDetailList, setJoinDetailList] = useState<{ value: string; display: string }[]>([]);

  const [operatorOptions] = useState<{ value: ComparisonOperator; display: string }[]>([
    { value: ComparisonOperator.Equal, display: '=' },
    { value: ComparisonOperator.LessThan, display: '<' },
    { value: ComparisonOperator.GreaterThan, display: '>' },
    { value: ComparisonOperator.NotEqual, display: '!=' },
  ]);

  const [logicalOperatorOptions] = useState<{ value: LogicalOperator; display: string }[]>([
    { value: LogicalOperator.And, display: 'AND' },
    { value: LogicalOperator.Or, display: 'OR' },
  ]);

  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  /* ---------  دریافت JoinDetail  --------- */
  useEffect(() => {
    const fetchJoinDetails = async () => {
      try {
        setLoading(true);
        const details = await JoinDetailApi.GetSelectionList();
        const list = details.map((d: any) => ({
          value: d.id.toString(),
          display: `${d.joinEntity} (${d.joinLocalKey} -> ${d.joinForeignKey})`,
        }));
        setJoinDetailList(list);
      } catch (err) {
        console.error('Failed to fetch JoinDetails', err);
        setError('خطا در بارگذاری لیست JoinDetail.');
      } finally {
        setLoading(false);
      }
    };
    fetchJoinDetails();
  }, []);

  /* ---------  دریافت قانون در حالت ویرایش  --------- */
  useEffect(() => {
    if (!permissionRuleId) return;

    const fetchRule = async () => {
      try {
        setLoading(true);
        const rule = await permissionRuleApi.getById(permissionRuleId);
        const form: PermissionRuleFormCommand = {
          id: rule.id,
          fieldName: rule.fieldName,
          joinDetailId: rule.joinDetailId,
          operator: rule.operator,
          value: rule.value,
          logicalOperator: rule.logicalOperator,
          groupOrder: rule.groupOrder,
        };
        setFormData(form);
      } catch (err) {
        console.error('Failed to load rule', err);
        setError('خطا در بارگذاری قانون مجوز.');
      } finally {
        setLoading(false);
      }
    };
    fetchRule();
  }, [permissionRuleId]);

  /* ---------  تغییرات فرم  --------- */
  const handleChange = <K extends keyof PermissionRuleFormCommand>(
    field: K,
    value: PermissionRuleFormCommand[K]
  ) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
    if (error) setError(null);
  };

  /* ---------  ثبت فرم  --------- */
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    // اعتبارسنجی ساده
    if (!formData.fieldName || !formData.operator) {
      setError('لطفاً فیلدهای ضروری را تکمیل کنید.');
      return;
    }

    setLoading(true);
    setError(null);
    try {
      if (permissionRuleId) {
        await permissionRuleApi.updatePermissionRule({
          id: permissionRuleId,
          ...formData,
        } as UpdatePermissionRuleCommand);
      } else {
        if (!permissionId) throw new Error('PermissionId missing.');
        await permissionRuleApi.createPermissionRule({
          permissionId: permissionId,
          ...formData,
        } as CreatePermissionRuleCommand);
      }
      onSuccess?.();
    } catch (err: any) {
      console.error('Submit error', err);
      setError(err.message || 'خطا در عملیات.');
    } finally {
      setLoading(false);
    }
  };

  return {
    formData,
    joinDetailList,
    operatorOptions,
    logicalOperatorOptions,
    loading,
    error,
    handleChange,
    handleSubmit,
    isEdit: !!permissionRuleId,
  };
};
