// src/modules/Authorization/hooks/Forms/useUserCreateForm.ts
import { useState, useEffect } from "react";
import { userApi } from "../../api/userApi";
import { roleApi } from "../../api/roleApi"; // ← اضافه شد
import type { CreateUserCommand } from "../../models/CreateUserCommand";

export const useUserCreateForm = (onSuccess?: () => void) => {
  const [formData, setFormData] = useState<CreateUserCommand>({
    personId: null,
    phoneNumber : '',
    UserName:'',
    Email:'',
    NickName:'',
    Password:'',
    roles: []   // ← اضافه شد
  });

  const [rolesList, setRolesList] = useState<{ id: string; name: string }[]>([]); // برای دراپ‌دان نقش‌ها
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  /** دریافت لیست نقش‌ها از API */
  useEffect(() => {
    const fetchRoles = async () => {
      try {
        const res = await roleApi.getRoles();
        // فرض می‌کنیم داده برگشتی آرایه‌ای از {id,name} هست
        setRolesList(res);
      } catch (err) {
        console.error("Error fetching roles:", err);
      }
    };
    fetchRoles();
  }, []);

  const handleChange = (field: keyof CreateUserCommand, value: any) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  const handleRolesChange = (roleName: string, checked: boolean) => {
    setFormData(prev => {
      const current = prev.roles || [];
      return {
        ...prev,
        roles: checked
          ? [...current, roleName]
          : current.filter(r => r !== roleName)
      };
    });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setLoading(true);
      setError(null);

      const res = await userApi.createUser(formData);
      console.log("User created:", res);
      onSuccess?.();
    } catch (err: any) {
      console.error("Full error:", err);
      if (err?.response?.data) {
        const data = err.response.data;
        if (typeof data === 'string') setError(data);
        else if (data.title) setError(data.title);
        else if (data.message) setError(data.message);
        else setError(JSON.stringify(data));
      } else {
        setError(err?.message || "خطای ناشناخته");
      }
    } finally {
      setLoading(false);
    }
  };

  return {
    formData,
    rolesList,          // ← اضافه شد
    loading,
    error,
    handleChange,
    handleSubmit,
    handleRolesChange,  // ← اضافه شد
  };
};
