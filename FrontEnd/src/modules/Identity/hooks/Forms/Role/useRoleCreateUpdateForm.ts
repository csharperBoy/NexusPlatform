// src/hooks/useRoleCreateUpdateForm.ts
import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom'; // فرض بر استفاده از react-router-dom
import { roleApi } from '../../../api/roleApi'; // مسیر API کاربران
import { CreateRoleCommand, UpdateRoleCommand, RoleFormCommand } from '../../../models/RoleCommands'; // مسیر مدل‌ها

export const useRoleCreateUpdateForm = (roleId?: string, onSuccess?: () => void) => {
  // state اولیه بر اساس حالت (ایجاد یا ویرایش)
  const initialFormState: RoleFormCommand = roleId
    ? {Id :roleId , Name: "", Description: "" ,OrderNum: 1 } // برای ویرایش
    : {  Name: "", Description: "" ,OrderNum: 1 }; // برای ایجاد،
  const [formData, setFormData] = useState<RoleFormCommand>(initialFormState);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null); // برای مدیریت خطا


  // بارگذاری اطلاعات کاربر در صورت ویرایش
  useEffect(() => {
    if (!roleId) return;

    const fetchRole = async () => {
      try {
        setLoading(true);
        const role = await roleApi.getById(roleId);
        // اطمینان از اینکه داده‌های بارگذاری شده با نوع UpdateRoleCommand مطابقت دارند
        const roleData: UpdateRoleCommand = {
            Id: role.id,
          Name: role.name,
          Description: role.description || '',
          OrderNum: role.orderNum,
          
        };
        setFormData(roleData);
      } catch (err) {
        console.error("Failed to fetch :", err);
        setError("خطا در بارگذاری اطلاعات .");
      } finally {
        setLoading(false);
      }
    };
    fetchRole();
  }, [roleId]);

  // مدیریت تغییرات فیلدهای فرم
  const handleChange = <K extends keyof RoleFormCommand>(field: K, value: RoleFormCommand[K]) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    // پاک کردن خطا هنگام تغییر
    if (error) setError(null);
  };


  // مدیریت ارسال فرم
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    // اعتبارسنجی اولیه
    if (roleId) { // حالت ویرایش
      // در اینجا می‌توان اعتبارسنجی‌های مربوط به ویرایش را انجام داد
      // مثلاً اگر فیلدهای خاصی نباید خالی باشند
      if (!formData.Name ) {
         setError("نام الزامی هست.");
         return;
      }
       // اگر بخواهیم کاربر بتواند رمز عبور را هم در حالت ویرایش تغییر دهد
       // باید مطمئن شویم که `Password` فیلد `UpdateRoleCommand` هم هست و آن را هندل کنیم
    } else { // حالت ایجاد
      const createData = formData as CreateRoleCommand; // Cast برای اطمینان از وجود Password
      if (!createData.Name) {
        setError("نام الزامی هست.");
        return;
      }
    }

    setLoading(true);
    setError(null); // پاک کردن خطا قبل از ارسال

    try {
      if (roleId) {
        // ارسال به API ویرایش - نیاز به type assertion داریم
        await roleApi.updateRole( formData as UpdateRoleCommand);
      } else {
        // ارسال به API ایجاد - نیاز به type assertion داریم
        await roleApi.createRole(formData as CreateRoleCommand);
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
    loading,
    error,
    handleChange,
    handleSubmit,
    isEdit: !!roleId, // برای استفاده در UI
  };
};
