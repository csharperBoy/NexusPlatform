// src/hooks/useUserCreateUpdateForm.ts
import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom'; // فرض بر استفاده از react-router-dom
import { roleApi } from '../../../api/roleApi'; // مسیر API نقش‌ها
import { userApi } from '../../../api/userApi'; // مسیر API کاربران
import { CreateUserCommand, UpdateUserCommand, UserFormCommand } from '../../../models/UserCommands'; // مسیر مدل‌ها

export const useUserCreateUpdateForm = (userId?: string, onSuccess?: () => void) => {
  // state اولیه بر اساس حالت (ایجاد یا ویرایش)
  const initialFormState: UserFormCommand = userId
    ? { UserName: "", Email: "" } // برای ویرایش، Password اجباری نیست
    : { UserName: "", Email: "", Password: "" }; // برای ایجاد، Password اجباری است

  const [formData, setFormData] = useState<UserFormCommand>(initialFormState);
  const [rolesList, setRolesList] = useState<{ id: string; name: string }[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null); // برای مدیریت خطا

  // بارگذاری لیست نقش‌ها
  useEffect(() => {
    const fetchRoles = async () => {
      try {
        const res = await roleApi.getRoles();
        setRolesList(res);
      } catch (err) {
        console.error("Failed to fetch roles:", err);
        setError("خطا در بارگذاری لیست نقش‌ها.");
      }
    };
    fetchRoles();
  }, []);

  // بارگذاری اطلاعات کاربر در صورت ویرایش
  useEffect(() => {
    if (!userId) return;

    const fetchUser = async () => {
      try {
        setLoading(true);
        const user = await userApi.getById(userId);
        // اطمینان از اینکه داده‌های بارگذاری شده با نوع UpdateUserCommand مطابقت دارند
        const userData: UpdateUserCommand = {
          UserName: user.userName,
          Email: user.email || '',
          NickName: user.nickName,
          phoneNumber: user.phoneNumber,
          personId: user.person?.id,
          roles: user.Roles || [],
          // Password در حالت ویرایش ارسال نمی‌شود مگر اینکه کاربر بخواهد آن را تغییر دهد
          // اگر لازم بود Password هم در UpdateUserCommand باشد و کاربر بتواند آن را هم تغییر دهد، باید در API و UI هم لحاظ شود
        };
        setFormData(userData);
      } catch (err) {
        console.error("Failed to fetch user:", err);
        setError("خطا در بارگذاری اطلاعات کاربر.");
      } finally {
        setLoading(false);
      }
    };
    fetchUser();
  }, [userId]);

  // مدیریت تغییرات فیلدهای فرم
  const handleChange = <K extends keyof UserFormCommand>(field: K, value: UserFormCommand[K]) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    // پاک کردن خطا هنگام تغییر
    if (error) setError(null);
  };

  // مدیریت تغییرات انتخاب نقش‌ها
  const handleRolesChange = (roleId: string, checked: boolean) => {
    setFormData(prev => {
      const currentRoles = prev.roles || [];
      const newRoles = checked
        ? [...currentRoles, roleId]
        : currentRoles.filter(r => r !== roleId);
      return { ...prev, roles: newRoles };
    });
  };

  // مدیریت ارسال فرم
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    // اعتبارسنجی اولیه
    if (userId) { // حالت ویرایش
      // در اینجا می‌توان اعتبارسنجی‌های مربوط به ویرایش را انجام داد
      // مثلاً اگر فیلدهای خاصی نباید خالی باشند
      if (!formData.UserName || !formData.Email) {
         setError("نام کاربری و ایمیل الزامی هستند.");
         return;
      }
       // اگر بخواهیم کاربر بتواند رمز عبور را هم در حالت ویرایش تغییر دهد
       // باید مطمئن شویم که `Password` فیلد `UpdateUserCommand` هم هست و آن را هندل کنیم
    } else { // حالت ایجاد
      const createData = formData as CreateUserCommand; // Cast برای اطمینان از وجود Password
      if (!createData.UserName || !createData.Email || !createData.Password) {
        setError("نام کاربری، ایمیل و رمز عبور الزامی هستند.");
        return;
      }
    }

    setLoading(true);
    setError(null); // پاک کردن خطا قبل از ارسال

    try {
      if (userId) {
        // ارسال به API ویرایش - نیاز به type assertion داریم
        await userApi.updateUser( formData as UpdateUserCommand);
      } else {
        // ارسال به API ایجاد - نیاز به type assertion داریم
        await userApi.createUser(formData as CreateUserCommand);
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
    rolesList,
    loading,
    error,
    handleChange,
    handleRolesChange,
    handleSubmit,
    isEdit: !!userId, // برای استفاده در UI
  };
};
