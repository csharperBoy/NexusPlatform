// src/modules/Authorization/hooks/Forms/useRoleCreateForm.ts
import { useState } from "react";
import { roleApi } from "../../../api/roleApi";
import type { CreateRoleCommand } from "../../../models/CreateRoleCommand";
import { useParams } from "react-router-dom";

export const useRoleCreateForm = (onSuccess?: () => void) => {
  
  const { parentId } = useParams<{ parentId: string }>();
  const [formData, setFormData] = useState<CreateRoleCommand>({
    Name: '',
    Description : '',
    OrderNum:0,
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleChange = (field: keyof CreateRoleCommand, value: any) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setLoading(true);
      setError(null);


      const res = await roleApi.createRole(formData);
      console.log("Role created:", res);
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