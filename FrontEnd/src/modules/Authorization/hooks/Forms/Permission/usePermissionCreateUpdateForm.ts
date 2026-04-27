// src/hooks/usePermissionCreateUpdateForm.ts
import { useState, useEffect } from 'react';
import { permissionApi } from '../../../api/PermissionApi'; 
import { CreatePermissionCommand, UpdatePermissionCommand, PermissionFormCommand } from '../../../models/PermissionCommands'; 
import { useParams } from "react-router-dom";
import { SelectionListDto } from '@/core/models/SelectionListDto';
import { resourceApi } from '@/modules/Authorization/api/ResourceApi';
import { userApi } from '@/modules/Identity/api/userApi';
import { personApi}from '@/modules/HR/api/personApi';
import { positionApi}from '@/modules/HR/api/positionApi';
import { roleApi } from '@/modules/Identity/api/roleApi'; 

export const usePermissionCreateUpdateForm = (permissionId?: string, onSuccess?: () => void) => {
  const { resourceId } = useParams<{ resourceId: string }>();
  const initialFormState: PermissionFormCommand = permissionId
    ? {Id :permissionId , AssigneeType: 0, AssigneeId: "" ,Action: 1 , ResourceId:resourceId ,scopes:null , Description:"",effect:1,IsActive:true,ExpiresAt:null , EffectiveFrom: null} // برای ویرایش
    : {  AssigneeType: 0, AssigneeId: "" ,Action: 1 , ResourceId:"",scopes:null , Description:"",effect:1,IsActive:true,ExpiresAt:null , EffectiveFrom: null }; // برای ایجاد،
  const [formData, setFormData] = useState<PermissionFormCommand>(initialFormState);

  const [scopesList, setScopesList] = useState<{ value: number; display: string }[]>([]);
  
  const [resourceList, setResourceList] = useState<SelectionListDto[]>([]);
  
  const [assignList, setAssignList] = useState<SelectionListDto[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null); // برای مدیریت خطا

  const assignTypeMap: Record<string, number> = {
        'Person': 0,
        'Position': 1,
        'Role': 2,
        'User'  :3,  
  
      };
  const actionMap: Record<string, number> = {
        'View': 0,
        'Create' : 1,
        'Edit' :2,
        'Delete' : 3,
        'Export' : 4,
        'Full': 99
      };

      const effectMap: Record<string, number> = {
        'allow': 0,    
        'Deny': 1  
      };

      const scopeMap: Record<string, number> = {
        'None' : 0,
        'Account' : 1,
        'Self' :2,
        'Unit' : 3,
        'UnitAndBelow': 4,
        'SpecificProperty': 5,
        'All' : 99
      };

useEffect(() => {
    const fetchAssignees = async () => {
      try {
        setLoading(true); // شروع بارگذاری
        let data: SelectionListDto[] = [];

        switch (formData.AssigneeType) {
          case 0: // Person
            data = await personApi.GetSelectionList();
            break;
          case 1: // Position
            data = await positionApi.GetSelectionList();
            break;
          case 2: // Role
            data = await roleApi.GetSelectionList();
            break;
          case 3: // User
            data = await userApi.GetSelectionList();
            break;
          default:
            // اگر AssigneeType نامعتبر بود، لیست خالی باشد
            data = [];
        }
        setAssignList(data);
      } catch (err) {
        console.error("Failed to fetch assign list:", err);
        setError("خطا در بارگذاری لیست انتخاب‌شونده‌ها.");
        setAssignList([]); // در صورت خطا، لیست را خالی کن
      } finally {
        setLoading(false); // پایان بارگذاری
      }
    };

    // این useEffect باید زمانی اجرا شود که formData.AssigneeType تغییر می‌کند
    // یا زمانی که داده‌ها در حالت ویرایش لود می‌شوند و AssigneeType مقدار اولیه دارد
    fetchAssignees();

    // نکته: اگر در حالت ویرایش، AssigneeId هم باید پر شود، باید یک useEffect جداگانه
    // یا منطقی در useEffect بالا داشته باشیم که AssigneeId را بر اساس داده‌های دریافتی تنظیم کند.
    // اما فعلا تمرکز روی تغییر نوع است.

  }, [formData.AssigneeType]); // این useEffect به تغییر AssigneeType واکنش نشان می‌دهد


      useEffect(() => {
      const fetchScopes = async () => {
        try {
          const list = Object.entries(scopeMap).map(([key, value]) => ({
            value,
            display: key
          }));

          setScopesList(list);
        } catch (err) {
          console.error("Failed to fetch scopes:", err);
          setError("خطا در بارگذاری لیست محدوده ها.");
        }
      };
       const fetchResources = async () => {
        try {
          const resources = await resourceApi.GetSelectionList();
          setResourceList(resources);
        } catch (err) {
          console.error("Failed to fetch Resource:", err);
          setError("خطا در بارگذاری لیست منابع.");
        }
      };
 


      fetchScopes();
      fetchResources();
    }, []);

  // بارگذاری اطلاعات در صورت ویرایش
  useEffect(() => {
    if (!permissionId) return;

    const fetchPermission = async () => {
      try {
        setLoading(true);
        const permission = await permissionApi.getById(permissionId);
        // اطمینان از اینکه داده‌های بارگذاری شده با نوع UpdatePermissionCommand مطابقت دارند
        const permissionData: UpdatePermissionCommand = {
          Id: permission.id,
          Action: permission.action,
          AssigneeType:  permission.assigneeType,
          effect:  permission.effect,
          AssigneeId: permission.assigneeId,
          Description: permission.description,
          scopes: permission.scopes || [],
          EffectiveFrom: permission.effectiveFrom,
          ExpiresAt: permission.expiresAt,
          IsActive: permission.isActive,
          ResourceId:permission.resourceId
        };
        setFormData(permissionData);
      } catch (err) {
        console.error("Failed to fetch :", err);
        setError("خطا در بارگذاری اطلاعات .");
      } finally {
        setLoading(false);
      }
    };
    fetchPermission();
  }, [permissionId]);

  // مدیریت تغییرات فیلدهای فرم
  const handleChange = <K extends keyof PermissionFormCommand>(field: K, value: PermissionFormCommand[K]) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    // پاک کردن خطا هنگام تغییر
    if (error) setError(null);
  };

  const handleAssignTypeChange = (newAssignType: number) => {
    // ابتدا نوع را در formData به‌روز کن
    setFormData(prev => ({
      ...prev,
      AssigneeType: newAssignType,
      AssigneeId: "" // AssigneeId را هنگام تغییر نوع، خالی کن
    }));
    // نیازی به صدا زدن مستقیم fetchAssignees نیست، چون useEffect بالا به تغییر
    // formData.AssigneeType واکنش نشان می‌دهد و لیست را لود می‌کند.
    // همچنین خطا را پاک می‌کنیم اگر وجود داشت.
    if (error) setError(null);
  };

  // مدیریت تغییرات انتخاب محدوده ها
  const handleScopesChange = (scopeValue: number, checked: boolean) => {
    setFormData(prev => {
      const currentScopes = prev.scopes || [];
      const newScopes = checked
        ? [...currentScopes, scopeValue]
        : currentScopes.filter(r => r !== scopeValue);
      return { ...prev, scopes: newScopes };
    });
  };

  // مدیریت ارسال فرم
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    // اعتبارسنجی اولیه
    if (permissionId) { 
      if (!formData.ResourceId ) {
         setError(" الزامی هست.");
         return;
      }
       } else { 
      const createData = formData as CreatePermissionCommand; 
      if (!createData.ResourceId) {
        setError(" الزامی هست.");
        return;
      }
    }

    setLoading(true);
    setError(null); 
    try {
      if (permissionId) {
        await permissionApi.updatePermission( formData as UpdatePermissionCommand);
      } else {
        await permissionApi.createPermission(formData as CreatePermissionCommand);
      }
      onSuccess?.(); 
    } catch (err: any) {
      console.error("Form submission error:", err);
      setError(err.message || "خطایی در عملیات رخ داد.");
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
    error,
    handleChange,
    handleScopesChange,
    handleSubmit,
    handleAssignTypeChange,
    isEdit: !!permissionId,
  };
};
