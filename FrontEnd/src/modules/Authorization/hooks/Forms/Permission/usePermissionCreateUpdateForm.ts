// src/hooks/usePermissionCreateUpdateForm.ts
import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom'; // فرض بر استفاده از react-router-dom
import { permissionApi } from '../../../api/PermissionApi'; // مسیر API کاربران
import { CreatePermissionCommand, UpdatePermissionCommand, PermissionFormCommand } from '../../../models/PermissionCommands'; // مسیر مدل‌ها
import { useParams } from "react-router-dom";
export const usePermissionCreateUpdateForm = (permissionId?: string, onSuccess?: () => void) => {
  // state اولیه بر اساس حالت (ایجاد یا ویرایش)
  // const { userId } = useParams<{ userId: string }>();
  // const { roleId } = useParams<{ roleId: string }>();
  const { resourceId } = useParams<{ resourceId: string }>();
  const initialFormState: PermissionFormCommand = permissionId
    ? {Id :permissionId , AssigneeType: 0, AssigneeId: "" ,Action: 1 , ResourceId:resourceId ,scopes:null , Description:"",effect:1,IsActive:true,ExpiresAt:null , EffectiveFrom: null} // برای ویرایش
    : {  AssigneeType: 0, AssigneeId: "" ,Action: 1 , ResourceId:"",scopes:null , Description:"",effect:1,IsActive:true,ExpiresAt:null , EffectiveFrom: null }; // برای ایجاد،
  const [formData, setFormData] = useState<PermissionFormCommand>(initialFormState);

  const [scopesList, setScopesList] = useState<{ value: number; display: string }[]>([]);
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

      fetchScopes();
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
          Action: permission.Action,
          AssigneeType:  permission.AssigneeType,
          effect:  permission.Effect,
          AssigneeId: permission.AssigneeId,
          Description: permission.description,
          scopes: permission.Scopes || [],
          EffectiveFrom: permission.EffectiveFrom,
          ExpiresAt: permission.ExpiresAt,
          IsActive: permission.isActive,
          ResourceId:permission.ResourceId
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
    if (permissionId) { // حالت ویرایش
      // در اینجا می‌توان اعتبارسنجی‌های مربوط به ویرایش را انجام داد
      // مثلاً اگر فیلدهای خاصی نباید خالی باشند
      if (!formData.ResourceId ) {
         setError(" الزامی هست.");
         return;
      }
       // اگر بخواهیم کاربر بتواند رمز عبور را هم در حالت ویرایش تغییر دهد
       // باید مطمئن شویم که `Password` فیلد `UpdatePermissionCommand` هم هست و آن را هندل کنیم
    } else { // حالت ایجاد
      const createData = formData as CreatePermissionCommand; // Cast برای اطمینان از وجود Password
      if (!createData.ResourceId) {
        setError(" الزامی هست.");
        return;
      }
    }

    setLoading(true);
    setError(null); // پاک کردن خطا قبل از ارسال

    try {
      if (permissionId) {
        // ارسال به API ویرایش - نیاز به type assertion داریم
        await permissionApi.updatePermission( formData as UpdatePermissionCommand);
      } else {
        // ارسال به API ایجاد - نیاز به type assertion داریم
        await permissionApi.createPermission(formData as CreatePermissionCommand);
      }
      onSuccess?.(); // اجرای callback موفقیت (مثلاً بازگشت به صفحه قبل)
    } catch (err: any) {
      console.error("Form submission error:", err);
      // نمایش خطای بازگشتی از API یا خطای عمومی
      setError(err.message || "خطایی در عملیات رخ داد.");
    } finally {
      setLoading(false);
    }
  };

  return {
    formData,
    scopesList,
    loading,
    error,
    handleChange,
    handleScopesChange,
    handleSubmit,
    isEdit: !!permissionId, // برای استفاده در UI
  };
};
