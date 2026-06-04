// src/hooks/Forms/Permission/usePermissionCreateUpdateForm.ts
import { useState, useEffect } from 'react';
import { permissionApi } from '../../../api/PermissionApi';
import {
  CreatePermissionCommand,
  UpdatePermissionCommand,
  PermissionFormCommand,
} from '../../../models/PermissionCommands';
import { CreatePermissionRuleCommand, PermissionRuleFormCommand } from '@/modules/Authorization/models/PermissionRuleCommands';
import { useParams } from 'react-router-dom';
import { SelectionListDto } from '@/core/models/SelectionListDto';
import { resourceApi } from '@/modules/Authorization/api/ResourceApi';
import { userApi } from '@/modules/Identity/api/userApi';
import { personApi } from '@/modules/HR/api/personApi';
import { positionApi } from '@/modules/HR/api/positionApi';
import { roleApi } from '@/modules/Identity/api/roleApi';
import { ComparisonOperator , LogicalOperator } from '@/modules/Authorization/models/PermissionRuleEnum';
import { resourceMetadataDto, fieldDto, joinDto }  from '@/modules/Authorization/models/ResourceMetadataDto';

export const usePermissionCreateUpdateForm = (
  permissionId?: string,
  onSuccess?: () => void
) => {
  const { resourceId } = useParams<{ resourceId: string }>();

  const initialFormState: PermissionFormCommand = permissionId
    ? {
        Id: permissionId,
        AssigneeType: 0,
        AssigneeId: '',
        Action: 1,
        ResourceId: resourceId,
        scopes: null,
        Description: '',
        effect: 1,
        IsActive: true,
        ExpiresAt: null,
        EffectiveFrom: null,
        rules: [],                           // <‑- اضافه شد
      }
    : {
        AssigneeType: 0,
        AssigneeId: '',
        Action: 1,
        ResourceId: '',
        scopes: null,
        Description: '',
        effect: 1,
        IsActive: true,
        ExpiresAt: null,
        EffectiveFrom: null,
        rules: [],                           // <‑- اضافه شد
      };

  const [formData, setFormData] = useState<PermissionFormCommand>(initialFormState);

  const [scopesList, setScopesList] = useState<{ value: number; display: string }[]>([]);
  const [resourceList, setResourceList] = useState<SelectionListDto[]>([]);
  const [assignList, setAssignList] = useState<SelectionListDto[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const [metadata, setMetadata] = useState<resourceMetadataDto | null>(null);
  const [fieldOptions, setFieldOptions] = useState<{ value: string; label: string }[]>([]);
  const [joinOptions, setJoinOptions] = useState<{ value: string; label: string; joinData: joinDto }[]>([]);
  const [metadataLoading, setMetadataLoading] = useState(false);
  const [ruleMode, setRuleMode] = useState<Record<number, 'local' | 'navigated'>>({});
const [selectedNav, setSelectedNav] = useState<Record<number, string>>({});
  /* ----------- maps ----------- */
  const assignTypeMap: Record<string, number> = {
    Person: 0,
    Position: 1,
    Role: 2,
    User: 3,
  };

  const actionMap: Record<string, number> = {
    View: 0,
    Create: 1,
    Edit: 2,
    Delete: 3,
    Export: 4,
    Full: 99,
  };

  const effectMap: Record<string, number> = { allow: 0, Deny: 1 };

  const scopeMap: Record<string, number> = {
    None: 0,
    Account: 1,
    Self: 2,
    Unit: 3,
    UnitAndBelow: 4,
    SpecificProperty: 5,
    All: 99,
  };
// مقداردهی اولیه ruleMode و selectedNav از روی قوانین موجود (برای ویرایش)
useEffect(() => {
  // بررسی می‌کنیم که rules وجود داشته باشد، آرایه باشد و حداقل یک عضو داشته باشد
  // همچنین joinOptions نیز باید بارگذاری شده باشد
  if (formData.rules && Array.isArray(formData.rules) && formData.rules.length > 0 && joinOptions.length > 0) {
    setRuleMode(prevMode => {
      const newMode = { ...prevMode };
      const newNav = { ...selectedNav };
      let changed = false;

      // حالا مطمئن هستیم rules وجود دارد و آرایه است
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
}, [formData.rules, joinOptions]); // وابستگی‌ها
  /* ----------- list fetch ----------- */
  useEffect(() => {
    const fetchAssignees = async () => {
      try {
        setLoading(true);
        let data: SelectionListDto[] = [];
        switch (formData.AssigneeType) {
          case 0:
            data = await personApi.GetSelectionList();
            break;
          case 1:
            data = await positionApi.GetSelectionList();
            break;
          case 2:
            data = await roleApi.GetSelectionList();
            break;
          case 3:
            data = await userApi.GetSelectionList();
            break;
          default:
            data = [];
        }
        setAssignList(data);
      } catch (err) {
        console.error('Failed to fetch assign list:', err);
        setError('خطا در بارگذاری لیست انتخاب‌شونده‌ها.');
      } finally {
        setLoading(false);
      }
    };
    fetchAssignees();
  }, [formData.AssigneeType]);

  useEffect(() => {
    const fetchScopes = async () => {
      try {
        const list = Object.entries(scopeMap).map(([key, value]) => ({
          value,
          display: key,
        }));
        setScopesList(list);
      } catch (err) {
        console.error('Failed to fetch scopes:', err);
        setError('خطا در بارگذاری لیست محدوده ها.');
      }
    };
    const fetchResources = async () => {
      try {
        const resources = await resourceApi.GetSelectionList();
        setResourceList(resources);
      } catch (err) {
        console.error('Failed to fetch Resource:', err);
        setError('خطا در بارگذاری لیست منابع.');
      }
    };
    fetchScopes();
    fetchResources();
  }, []);

  /* ----------- fetch permission (ویرایش) ----------- */
  useEffect(() => {
    if (!permissionId) return;
    const fetchPermission = async () => {
      try {
        setLoading(true);
        const permission = await permissionApi.getById(permissionId);

        const actionNum = actionMap[permission.action] ?? permission.action;
        const effectNum = effectMap[permission.effect] ?? permission.effect;
        const assignTypeNum = assignTypeMap[permission.assigneeType] ?? permission.assigneeType;

        // map scopes
        const scopesNum = Array.isArray(permission.scopes)
          ? permission.scopes.map((s: any) => scopeMap[s.scope])
          : null;

        // map rules + join details
        const rules = (permission.rules ?? []).map((r: any) => ({
          Id: r.id,
          FieldName: r.fieldName,
          Operator: r.operator,
          Value: r.value,
          LogicalOperator: r.logicalOperator,
          GroupOrder: r.groupOrder,
          JoinLocalKey: r.joinDetail?.joinLocalKey ?? '',
          JoinForeignKey: r.joinDetail?.joinForeignKey ?? '',
          JoinEntity: r.joinDetail?.joinEntity ?? '',
          JoinDetailId: r.joinDetail?.id ?? '',
        }));

        const permissionData: UpdatePermissionCommand = {
          Id: permission.id,
          ResourceId: permission.resourceId,
          AssigneeId: permission.assigneeId,
          AssigneeType: assignTypeNum,
          Action: actionNum,
          effect: effectNum,
          Description: permission.description,
          scopes: scopesNum,
          EffectiveFrom: permission.effectiveFrom ? new Date(permission.effectiveFrom) : null,
          ExpiresAt: permission.expiresAt ? new Date(permission.expiresAt) : null,
          IsActive: permission.isActive,
          rules: permission.rules,
        };

        setFormData(permissionData);
      } catch (err) {
        console.error('Failed to fetch permission:', err);
        setError('خطا در بارگذاری اطلاعات.');
      } finally {
        setLoading(false);
      }
    };
    fetchPermission();
  }, [permissionId]);

// useEffect برای نظارت بر تغییر ResourceId
useEffect(() => {
  if (formData.ResourceId) {
    fetchMetadata(formData.ResourceId);
  } else {
    setMetadata(null);
    setFieldOptions([]);
    setJoinOptions([]);
  }
}, [formData.ResourceId]);

  // تابع دریافت متادیتا بر اساس ResourceId
const fetchMetadata = async (resourceId: string) => {
  if (!resourceId) {
    setMetadata(null);
    setFieldOptions([]);
    setJoinOptions([]);
    return;
  }
  try {
    setMetadataLoading(true);
    // 2. دریافت متادیتا با استفاده از resourceId
    const metadataList = await resourceApi.getMetadata(resourceId);
    console.log('📦 Metadata response:', metadataList);
    if (metadataList && metadataList.length > 0) {
      const meta = metadataList[0];
      setMetadata(meta);
      // ساخت گزینه‌های فیلدهای اسکالر
      const scalarOpts = meta.scalarFields.map((f: fieldDto) => ({
        value: f.name,
        label: f.displayName || f.name,
      }));
      setFieldOptions(scalarOpts);
      // ساخت گزینه‌های جوین (ناویگیشن)
      const joinOpts = meta.joins.map((j: joinDto) => ({
        value: j.navigationName,
        label: j.navigationName,
        joinData: j,
      }));
      setJoinOptions(joinOpts);
    } else {
      setFieldOptions([]);
      setJoinOptions([]);
    }
  } catch (err) {
    console.error('Failed to fetch metadata:', err);
    setError('خطا در بارگذاری ساختار فیلدهای منبع');
  } finally {
    setMetadataLoading(false);
  }
};
  /* ----------- rule‑related helpers ----------- */
  const handleAddRule = () => {
    const newRule: CreatePermissionRuleCommand = {
    fieldName: '',
    operator: ComparisonOperator.Equal,
    value: '',
    logicalOperator: LogicalOperator.And,
    groupOrder: 0,
    // permissionId: permissionId ?? '',
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

  /* ----------- field change helpers ----------- */
  const handleChange = <K extends keyof PermissionFormCommand>(field: K, value: PermissionFormCommand[K]) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    if (error) setError(null);
  };

  const handleAssignTypeChange = (newAssignType: number) => {
    setFormData(prev => ({
      ...prev,
      AssigneeType: newAssignType,
      AssigneeId: '',
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

  /* ----------- submit ----------- */
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    // Basic validation
    if (!formData.ResourceId) {
      setError('منبع ضروری است.');
      return;
    }

    setLoading(true);
    setError(null);
    try {
      // در صورت لزوم، joinDetail را به شکل nested object تبدیل می‌کنیم
      const payload = {
        ...formData,
        rules: (formData.rules ?? []).map(r => ({
          ...r,
          // اگر می‌خواهید joinDetail به‌صورت nested ارسال شود
          // joinDetail: r.JoinLocalKey
          //   ? {
          //       joinLocalKey: r.JoinLocalKey,
          //       joinForeignKey: r.JoinForeignKey,
          //       joinEntity: r.JoinEntity,
          //     }
          //   : undefined,
        })),
      };

      if (permissionId) {
        await permissionApi.updatePermission(payload as UpdatePermissionCommand);
      } else {
        await permissionApi.createPermission(payload as CreatePermissionCommand);
      }
      onSuccess?.();
    } catch (err: any) {
      console.error('Form submission error:', err);
      setError(err.message || 'خطایی در عملیات رخ داد.');
    } finally {
      setLoading(false);
    }
  };
// تغییر نوع قانون (local/navigated)
const handleRuleModeChange = (idx: number, mode: 'local' | 'navigated') => {
  setRuleMode(prev => ({ ...prev, [idx]: mode }));
  if (mode === 'local') {
    // پاک کردن اطلاعات جوین
    handleRuleChange(idx, 'joinLocalKey', '');
    handleRuleChange(idx, 'joinForeignKey', '');
    handleRuleChange(idx, 'joinEntity', '');
    setSelectedNav(prev => ({ ...prev, [idx]: '' }));
    handleRuleChange(idx, 'fieldName', '');
  } else {
    handleRuleChange(idx, 'joinLocalKey', '');
    handleRuleChange(idx, 'joinForeignKey', '');
    handleRuleChange(idx, 'joinEntity', '');
    setSelectedNav(prev => ({ ...prev, [idx]: '' }));
    handleRuleChange(idx, 'fieldName', '');
  }
};

// انتخاب ناویگیشن
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

// تابع دریافت لیست فیلدها برای یک قانون خاص
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
  return {
    formData,
    scopesList,
    resourceList,
    assignList,
    loading,
    error,
    handleChange,
    handleScopesChange,
    handleSubmit,
    handleAssignTypeChange,
    // rule helpers
    handleAddRule,
    handleRemoveRule,
    handleRuleChange,
    isEdit: !!permissionId,
    ruleMode,
  selectedNav,
  handleRuleModeChange,
  handleNavigationSelect,
  getFieldOptionsForRule,
  fieldOptions,
  joinOptions,
  metadataLoading,
    
  };
};
