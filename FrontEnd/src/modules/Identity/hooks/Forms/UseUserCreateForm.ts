// src/modules/Authorization/hooks/Forms/useUserCreateForm.ts
import { useState } from "react";
import { userApi } from "../../api/userApi";
import type { CreateUserCommand } from "../../models/CreateUserCommand";
import { useParams } from "react-router-dom";

export const useUserCreateForm = (onSuccess?: () => void) => {
  
  const { parentId } = useParams<{ parentId: string }>();
  const [formData, setFormData] = useState<CreateUserCommand>({
    personId: null,
    phoneNumber : '',
    UserName:'',
    Email:'',
    NickName:'',
    Password:''
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleChange = (field: keyof CreateUserCommand, value: any) => {
    setFormData(prev => ({ ...prev, [field]: value }));
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
    loading,
    error,
    handleChange,
    handleSubmit,
  };
};