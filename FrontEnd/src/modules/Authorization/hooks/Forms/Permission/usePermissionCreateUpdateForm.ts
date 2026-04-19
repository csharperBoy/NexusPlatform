// src/hooks/usePermissionCreateUpdateForm.ts
import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom'; // فرض بر استفاده از react-router-dom
import { permissionApi } from '../../../api/PermissionApi'; // مسیر API کاربران
import { CreatePermissionCommand, UpdatePermissionCommand, PermissionFormCommand } from '../../../models/PermissionCommands'; // مسیر مدل‌ها
import { useParams } from "react-router-dom";
export const usePermissionCreateUpdateForm = (permissionId?: string, onSuccess?: () => void) => {
  // state اولیه بر اساس حالت (ایجاد یا ویرایش)
  const { parentId } = useParams<{ parentId: string }>();
  const initialFormState: PermissionFormCommand = permissionId
    ? {Id :permissionId , key: "", name: "" ,category: 1 , type:1,displayOrder:0,description:"",icon:"",parentId:"",route:"" } // برای ویرایش
    : {  key: "", name: "" ,category: 1 , type:1,displayOrder:0,description:"",icon:"",parentId:parentId,route:"" }; // برای ایجاد،
  const [formData, setFormData] = useState<PermissionFormCommand>(initialFormState);

  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null); // برای مدیریت خطا

  const typeMap: Record<string, number> = {
        'Module': 0,
        'Ui': 1,
        'Data': 2,
      };
  const categoryMap: Record<string, number> = {
        'General': 0,
        'System': 1,
        'Module': 2,
        'Menu': 3,
        'Page': 4,
        'Component': 5,
        'DatabaseTable': 6,
        'RowInTable': 7,
      };

  // بارگذاری اطلاعات کاربر در صورت ویرایش
  useEffect(() => {
    if (!permissionId) return;

    const fetchPermission = async () => {
      try {
        setLoading(true);
        const permission = await permissionApi.getById(permissionId);
        // اطمینان از اینکه داده‌های بارگذاری شده با نوع UpdatePermissionCommand مطابقت دارند
        const permissionData: UpdatePermissionCommand = {
          Id: permission.id,
          name: permission.name,
          description: permission.description || '',
          category:  categoryMap[permission.category],
          displayOrder: permission.displayOrder,
          key: permission.key,
          type: typeMap[permission.type],
          icon: permission.icon,
          parentId: permission.parentId,
          route: permission.path
          
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


  // مدیریت ارسال فرم
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    // اعتبارسنجی اولیه
    if (permissionId) { // حالت ویرایش
      // در اینجا می‌توان اعتبارسنجی‌های مربوط به ویرایش را انجام داد
      // مثلاً اگر فیلدهای خاصی نباید خالی باشند
      if (!formData.key ) {
         setError("کلید الزامی هست.");
         return;
      }
       // اگر بخواهیم کاربر بتواند رمز عبور را هم در حالت ویرایش تغییر دهد
       // باید مطمئن شویم که `Password` فیلد `UpdatePermissionCommand` هم هست و آن را هندل کنیم
    } else { // حالت ایجاد
      const createData = formData as CreatePermissionCommand; // Cast برای اطمینان از وجود Password
      if (!createData.key) {
        setError("کلید الزامی هست.");
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
    loading,
    error,
    handleChange,
    handleSubmit,
    isEdit: !!permissionId, // برای استفاده در UI
  };
};
