// src/modules/Authorization/hooks/usePermissionCreateUpdate.ts
import { useState, useEffect, useCallback, useMemo } from 'react';
import { useParams } from 'react-router-dom';
import { permissionApi } from '@/modules/Authorization/api/PermissionApi';
import { resourceApi } from '@/modules/Authorization/api/ResourceApi';
import { personApi } from '@/modules/HR/api/personApi';
import { postApi } from '@/modules/HR/api/PostApi';
import { roleApi } from '@/modules/Identity/api/roleApi';
import { userApi } from '@/modules/Identity/api/userApi';
import { SelectionListDto } from '@/core/models/SelectionListDto';
import { PermissionFormCommand   ,CreatePermissionCommand,
  UpdatePermissionCommand} from '@/modules/Authorization/models/PermissionCommands';

import {
  CreatePermissionRuleCommand,
  PermissionRuleFormCommand,
} from '@/modules/Authorization/models/PermissionRuleCommands';
import {
  ComparisonOperator,
  comparisonOperatorFromText,
  LogicalOperator,
  logicalOperatorFromText,
} from '@/modules/Authorization/models/PermissionRuleEnum';
import { comparisonScopeFromText } from '@/modules/Authorization/models/PermissionEnum';
import { resourceMetadataDto, fieldDto, joinDto } from '@/modules/Authorization/models/ResourceMetadataDto';

export interface UsePermissionCreateUpdateReturn {
  formData: PermissionFormCommand;
  scopesList: { value: number; display: string }[];
  resourceList: SelectionListDto[];
  assignList: SelectionListDto[];
  loading: boolean;
  metadataLoading: boolean;
  error: string | null;
  isEdit: boolean;
  useDynamicFilter: boolean;
  useNavigate: boolean;
  useScope: boolean;
  ruleMode: Record<number, 'local' | 'navigated'>;
  selectedNav: Record<number, string>;
  fieldOptions: { value: string; label: string }[];
  joinOptions: { value: string; label: string; joinData: joinDto }[];
  handleChange: <K extends keyof PermissionFormCommand>(field: K, value: PermissionFormCommand[K]) => void;
  handleAssignTypeChange: (newAssignType: number) => void;
  handleScopesChange: (scopeValue: number, checked: boolean) => void;
  handleAddRule: () => void;
  handleRemoveRule: (index: number) => void;
  handleRuleChange: <K extends keyof PermissionRuleFormCommand>(
    index: number,
    field: K,
    value: PermissionRuleFormCommand[K]
  ) => void;
  handleRuleModeChange: (idx: number, mode: 'local' | 'navigated') => void;
  handleNavigationSelect: (idx: number, navValue: string) => void;
  getFieldOptionsForRule: (idx: number) => { value: string; label: string }[];
  handleSubmit: (e: React.FormEvent) => Promise<void>;
}

export const usePermissionCreateUpdate = (
  permissionIdParam?: string,
  onSuccess?: () => void
): UsePermissionCreateUpdateReturn => {
  const { id: routeId } = useParams<{ id: string }>();
  const permissionId = permissionIdParam || routeId;
  const isEdit = !!permissionId;

  const initialFormState: PermissionFormCommand = useMemo(() => ({
    id: permissionId ?? '',
    resourceId: '',
    assigneeId: '',
    assigneeType: undefined,
    action: undefined,
    effect: undefined,
    effectiveFrom: null,
    expiresAt: null,
    isActive: true,
    description: '',
    scopes: [],
    rules: [],
  }), [permissionId]);

  const [formData, setFormData] = useState<PermissionFormCommand>(initialFormState);
  const [scopesList, setScopesList] = useState<{ value: number; display: string }[]>([]);
  const [resourceList, setResourceList] = useState<SelectionListDto[]>([]);
  const [assignList, setAssignList] = useState<SelectionListDto[]>([]);
  
  const [loading, setLoading] = useState<boolean>(false);
  const [metadataLoading, setMetadataLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const [metadata, setMetadata] = useState<resourceMetadataDto | null>(null);
  const [fieldOptions, setFieldOptions] = useState<{ value: string; label: string }[]>([]);
  const [joinOptions, setJoinOptions] = useState<{ value: string; label: string; joinData: joinDto }[]>([]);
  
  const [ruleMode, setRuleMode] = useState<Record<number, 'local' | 'navigated'>>({});
  const [selectedNav, setSelectedNav] = useState<Record<number, string>>({});

  const [useDynamicFilter, setUseDynamicFilter] = useState<boolean>(false);
  const [useNavigate, setUseNavigate] = useState<boolean>(false);
  const [useScope, setUseScope] = useState<boolean>(false);

  // ۱. دریافت لیست اسکوپ‌ها و منابع اصلی در ابتدا
  useEffect(() => {
    const fetchInitialData = async () => {
      try {
        const scopes = Object.entries(comparisonScopeFromText).map(([key, value]) => ({
          value,
          display: key,
        }));
        setScopesList(scopes);

        const resources = await resourceApi.GetSelectionList();
        setResourceList(resources);
      } catch (err) {
        console.error('Error fetching initial lookups:', err);
        setError('خطا در بارگذاری اطلاعات اولیه (منابع و محدوده‌ها)');
      }
    };
    fetchInitialData();
  }, []);

  // ۲. دریافت لیست انتخاب شونده‌ها بر اساس نوع گیرنده (assigneeType)
  useEffect(() => {
    if (formData.assigneeType === undefined || formData.assigneeType === null) {
      setAssignList([]);
      return;
    }

    const fetchAssignees = async () => {
      try {
        setLoading(true);
        let data: SelectionListDto[] = [];
        switch (formData.assigneeType) {
          case 0: // Person
            data = await personApi.GetSelectionList();
            break;
          case 1: // Position/Post
            data = await postApi.GetSelectionList();
            break;
          case 2: // Role
            data = await roleApi.GetSelectionList();
            break;
          case 3: // User
            data = await userApi.GetSelectionList();
            break;
          default:
            data = [];
        }
        setAssignList(data);
      } catch (err) {
        console.error('Failed to fetch assignee list:', err);
        setError('خطا در بارگذاری لیست انتخاب گیرنده.');
      } finally {
        setLoading(false);
      }
    };

    fetchAssignees();
  }, [formData.assigneeType]);

  // ۳. دریافت اطلاعات متادیتا با تغییر ResourceId
  const fetchMetadata = useCallback(async (resId: string) => {
    if (!resId) {
      setMetadata(null);
      setFieldOptions([]);
      setJoinOptions([]);
      setUseDynamicFilter(false);
      setUseNavigate(false);
      setUseScope(false);
      return;
    }

    try {
      setMetadataLoading(true);
      const metadataList = await resourceApi.getMetadata(resId);
      if (metadataList && metadataList.length > 0) {
        const meta = metadataList[0];
        setMetadata(meta);

        const scalarOpts = meta.scalarFields.map((f: fieldDto) => ({
          value: f.name,
          label: f.displayName || f.name,
        }));
        setFieldOptions(scalarOpts);

        const joinOpts = meta.joins.map((j: joinDto) => ({
          value: j.navigationName,
          label: j.navigationName,
          joinData: j,
        }));
        setJoinOptions(joinOpts);

        setUseDynamicFilter(meta.useDynamicFilter);
        setUseNavigate(meta.useNavigate);
        setUseScope(meta.useScope);
      } else {
        setFieldOptions([]);
        setJoinOptions([]);
        setUseDynamicFilter(false);
        setUseNavigate(false);
        setUseScope(false);
      }
    } catch (err) {
      console.error('Failed to fetch resource metadata:', err);
      setFieldOptions([]);
      setJoinOptions([]);
      setUseDynamicFilter(false);
      setUseNavigate(false);
      setUseScope(false);
    } finally {
      setMetadataLoading(false);
    }
  }, []);

  useEffect(() => {
    if (formData.resourceId) {
      fetchMetadata(formData.resourceId);
    }
  }, [formData.resourceId, fetchMetadata]);

  // ۴. دریافت اطلاعات مجوز جهت ویرایش
useEffect(() => {
  if (!permissionId) return;

  const fetchPermission = async () => {
    try {
      setLoading(true);
      const permission: any = await permissionApi.getById(permissionId);

      // ۱. استخراج آی‌دی با پشتیبانی از PascalCase و fallback به آی‌دی روت
      const pId = permission.id ?? permission.Id ?? permissionId;
      const pResourceId = permission.resourceId ?? permission.ResourceId ?? '';
      const pAssigneeId = permission.assigneeId ?? permission.AssigneeId ?? '';

      // ۲. استخراج و تبدیل مطمئن AssigneeType (به عدد)
      let rawAssigneeType = permission.assigneeType ?? permission.AssigneeType;
      if (rawAssigneeType !== undefined && rawAssigneeType !== null) {
        rawAssigneeType = Number(rawAssigneeType);
      }

      // ۳. مپ کردن قوانین
      const rawRules = permission.rules ?? permission.Rules ?? [];
      const mappedRules: CreatePermissionRuleCommand[] = rawRules.map((r: any) => ({
        fieldName: r.fieldName ?? r.FieldName ?? '',
        operator: typeof (r.operator ?? r.Operator) === 'string'
          ? (comparisonOperatorFromText[r.operator ?? r.Operator] ?? ComparisonOperator.Equal)
          : (r.operator ?? r.Operator ?? ComparisonOperator.Equal),
        value: r.value ?? r.Value ?? '',
        logicalOperator: typeof (r.logicalOperator ?? r.LogicalOperator) === 'string'
          ? (logicalOperatorFromText[r.logicalOperator ?? r.LogicalOperator] ?? LogicalOperator.And)
          : (r.logicalOperator ?? r.LogicalOperator ?? LogicalOperator.And),
        groupOrder: r.groupOrder ?? r.GroupOrder ?? 0,
        joinLocalKey: r.joinDetail?.joinLocalKey ?? r.joinDetail?.JoinLocalKey ?? r.JoinDetail?.JoinLocalKey ?? '',
        joinForeignKey: r.joinDetail?.joinForeignKey ?? r.joinDetail?.JoinForeignKey ?? r.JoinDetail?.JoinForeignKey ?? '',
        joinEntity: r.joinDetail?.joinEntity ?? r.joinDetail?.JoinEntity ?? r.JoinDetail?.JoinEntity ?? '',
      }));

      const permissionData: UpdatePermissionCommand = {
        id: pId,
        resourceId: pResourceId,
        assigneeId: pAssigneeId,
        assigneeType: rawAssigneeType,
        action: permission.action ?? permission.Action,
        effect: permission.effect ?? permission.Effect,
        description: permission.description ?? permission.Description ?? '',
        scopes: permission.scopes ?? permission.Scopes ?? [],
        effectiveFrom: (permission.effectiveFrom ?? permission.EffectiveFrom)
          ? new Date(permission.effectiveFrom ?? permission.EffectiveFrom)
          : null,
        expiresAt: (permission.expiresAt ?? permission.ExpiresAt)
          ? new Date(permission.expiresAt ?? permission.ExpiresAt)
          : null,
        isActive: permission.isActive ?? permission.IsActive ?? true,
        rules: mappedRules,
      };

      setFormData(permissionData);
    } catch (err) {
      console.error('Failed to fetch permission details:', err);
      setError('خطا در بارگذاری اطلاعات مجوز.');
    } finally {
      setLoading(false);
    }
  };

  fetchPermission();
}, [permissionId]);

  // ۵. مقداردهی اولیه حالت قوانین (Navigated vs Local) پس از بارگذاری داده‌ها
  useEffect(() => {
    if (formData.rules && Array.isArray(formData.rules) && formData.rules.length > 0 && joinOptions.length > 0) {
      setRuleMode(prevMode => {
        const newMode = { ...prevMode };
        const newNav = { ...selectedNav };
        let changed = false;

        formData.rules!.forEach((rule, idx) => {
          if (newMode[idx] === undefined) {
            changed = true;
            if (rule.joinEntity && rule.joinEntity.trim() !== '') {
              newMode[idx] = 'navigated';
              const matchedNav = joinOptions.find(j =>
                j.joinData.targetEntity === rule.joinEntity || j.value === rule.joinEntity
              );
              newNav[idx] = matchedNav ? matchedNav.value : rule.joinEntity;
            } else {
              newMode[idx] = 'local';
            }
          }
        });

        if (changed) {
          setSelectedNav(newNav);
          return newMode;
        }
        return prevMode;
      });
    }
  }, [formData.rules, joinOptions, selectedNav]);

  // ۶. ایونت هاندرها (Event Handlers)
  const handleChange = <K extends keyof PermissionFormCommand>(field: K, value: PermissionFormCommand[K]) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    if (error) setError(null);
  };

  const handleAssignTypeChange = (newAssignType: number) => {
    setFormData(prev => ({
      ...prev,
      assigneeType: newAssignType,
      assigneeId: '',
    }));
    if (error) setError(null);
  };

  const handleScopesChange = (scopeValue: number, checked: boolean) => {
    setFormData(prev => {
      const currentScopes = prev.scopes ?? [];
      const newScopes = checked
        ? [...currentScopes, scopeValue]
        : currentScopes.filter(r => r !== scopeValue);
      return { ...prev, scopes: newScopes };
    });
  };

  const handleAddRule = () => {
    const newRule: CreatePermissionRuleCommand = {
      fieldName: '',
      operator: ComparisonOperator.Equal,
      value: '',
      logicalOperator: LogicalOperator.And,
      groupOrder: 0,
      joinLocalKey: '',
      joinForeignKey: '',
      joinEntity: '',
    };

    setFormData(prev => ({
      ...prev,
      rules: [...(prev.rules ?? []), newRule] as CreatePermissionRuleCommand[],
    }));
  };

  const handleRemoveRule = (index: number) => {
    setFormData(prev => ({
      ...prev,
      rules: (prev.rules ?? []).filter((_, i) => i !== index),
    }));
  };

  const handleRuleChange = <K extends keyof PermissionRuleFormCommand>(
    index: number,
    field: K,
    value: PermissionRuleFormCommand[K]
  ) => {
    setFormData(prev => ({
      ...prev,
      rules: (prev.rules ?? []).map((r, i) =>
        i === index ? { ...r, [field]: value } : r
      ),
    }));
  };

  const handleRuleModeChange = (idx: number, mode: 'local' | 'navigated') => {
    setRuleMode(prev => ({ ...prev, [idx]: mode }));
    handleRuleChange(idx, 'joinLocalKey', '');
    handleRuleChange(idx, 'joinForeignKey', '');
    handleRuleChange(idx, 'joinEntity', '');
    setSelectedNav(prev => ({ ...prev, [idx]: '' }));
    handleRuleChange(idx, 'fieldName', '');
  };

  const handleNavigationSelect = (idx: number, navValue: string) => {
    const nav = joinOptions.find(j => j.value === navValue);
    if (nav) {
      handleRuleChange(idx, 'joinLocalKey', nav.joinData.currentKey);
      handleRuleChange(idx, 'joinForeignKey', nav.joinData.targetKey);
      handleRuleChange(idx, 'joinEntity', nav.joinData.targetEntity);
      setSelectedNav(prev => ({ ...prev, [idx]: navValue }));
      handleRuleChange(idx, 'fieldName', '');
    } else {
      handleRuleChange(idx, 'joinLocalKey', '');
      handleRuleChange(idx, 'joinForeignKey', '');
      handleRuleChange(idx, 'joinEntity', '');
      setSelectedNav(prev => ({ ...prev, [idx]: '' }));
      handleRuleChange(idx, 'fieldName', '');
    }
  };

  const getFieldOptionsForRule = (idx: number) => {
    const mode = ruleMode[idx];
    if (mode === 'navigated' && selectedNav[idx]) {
      const nav = joinOptions.find(j => j.value === selectedNav[idx]);
      if (nav?.joinData.targetScalarFields) {
        return nav.joinData.targetScalarFields.map((f: any) => ({
          value: f.name,
          label: f.displayName || f.name,
        }));
      }
    }
    return fieldOptions;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!formData.resourceId) {
      setError('انتخاب منبع الزامی است.');
      return;
    }
    if (formData.assigneeType === undefined) {
      setError('لطفاً نوع گیرنده مجوز را انتخاب کنید.');
      return;
    }
    if (formData.action === undefined) {
      setError('لطفاً نوع عملیات را انتخاب کنید.');
      return;
    }
    if (formData.effect === undefined) {
      setError('لطفاً وضعیت مجاز یا غیرمجاز بودن را تعیین کنید.');
      return;
    }

    setLoading(true);
    setError(null);
    try {
      const payload = {
        ...formData,
        scopes: formData.scopes ?? [],
        rules: (formData.rules ?? []).map(r => ({ ...r })),
      };

      if (isEdit) {
        await permissionApi.updatePermission(payload as UpdatePermissionCommand);
      } else {
        await permissionApi.createPermission(payload as CreatePermissionCommand);
      }
      onSuccess?.();
    } catch (err: any) {
      console.error('Form submission error:', err);
      setError(err.message || 'خطایی در ثبت اطلاعات مجوز رخ داد.');
    } finally {
      setLoading(false);
    }
  };

  return {
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
    fieldOptions,
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
  };
};