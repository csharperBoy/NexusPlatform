// src/hooks/useResourceCreateUpdateForm.ts
import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom'; // فرض بر استفاده از react-router-dom
import { resourceApi } from '../../../api/ResourceApi'; // مسیر API کاربران
import { CreateResourceCommand, UpdateResourceCommand, ResourceFormCommand } from '../../../models/ResourceCommands'; // مسیر مدل‌ها
import { useParams } from "react-router-dom";
export const useResourceCreateUpdateForm = (resourceId?: string, onSuccess?: () => void) => {
  // state اولیه بر اساس حالت (ایجاد یا ویرایش)
  const { parentId } = useParams<{ parentId: string }>();
  const initialFormState: ResourceFormCommand = resourceId
    ? {Id :resourceId , key: "", name: "" ,category: 1 , type:1,displayOrder:0,description:"",icon:"",parentId:"",route:"" } // برای ویرایش
    : {  key: "", name: "" ,category: 1 , type:1,displayOrder:0,description:"",icon:"",parentId:parentId,route:"" }; // برای ایجاد،
  const [formData, setFormData] = useState<ResourceFormCommand>(initialFormState);

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
    if (!resourceId) return;

    const fetchResource = async () => {
      try {
        setLoading(true);
        const resource = await resourceApi.getById(resourceId);
        // اطمینان از اینکه داده‌های بارگذاری شده با نوع UpdateResourceCommand مطابقت دارند
        const resourceData: UpdateResourceCommand = {
          Id: resource.id,
          name: resource.name,
          description: resource.description || '',
          category:  categoryMap[resource.category],
          displayOrder: resource.displayOrder,
          key: resource.key,
          type: typeMap[resource.type],
          icon: resource.icon,
          parentId: resource.parentId,
          route: resource.path
          
        };
        setFormData(resourceData);
      } catch (err) {
        console.error("Failed to fetch :", err);
        setError("خطا در بارگذاری اطلاعات .");
      } finally {
        setLoading(false);
      }
    };
    fetchResource();
  }, [resourceId]);

  // مدیریت تغییرات فیلدهای فرم
  const handleChange = <K extends keyof ResourceFormCommand>(field: K, value: ResourceFormCommand[K]) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    // پاک کردن خطا هنگام تغییر
    if (error) setError(null);
  };


  // مدیریت ارسال فرم
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    // اعتبارسنجی اولیه
    if (resourceId) { // حالت ویرایش
      // در اینجا می‌توان اعتبارسنجی‌های مربوط به ویرایش را انجام داد
      // مثلاً اگر فیلدهای خاصی نباید خالی باشند
      if (!formData.key ) {
         setError("کلید الزامی هست.");
         return;
      }
       // اگر بخواهیم کاربر بتواند رمز عبور را هم در حالت ویرایش تغییر دهد
       // باید مطمئن شویم که `Password` فیلد `UpdateResourceCommand` هم هست و آن را هندل کنیم
    } else { // حالت ایجاد
      const createData = formData as CreateResourceCommand; // Cast برای اطمینان از وجود Password
      if (!createData.key) {
        setError("کلید الزامی هست.");
        return;
      }
    }

    setLoading(true);
    setError(null); // پاک کردن خطا قبل از ارسال

    try {
      if (resourceId) {
        // ارسال به API ویرایش - نیاز به type assertion داریم
        await resourceApi.updateResource( formData as UpdateResourceCommand);
      } else {
        // ارسال به API ایجاد - نیاز به type assertion داریم
        await resourceApi.createResource(formData as CreateResourceCommand);
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
    isEdit: !!resourceId, // برای استفاده در UI
  };
};
