// src/modules/Authorization/hooks/Forms/useResourceCreateForm.ts
import { useState } from "react";
import { resourceApi } from "../../api/ResourcesApi";
import type { CreateResourceRequest } from "../../models/CreateResourceRequest";

export const useResourceCreateForm = (onSuccess?: () => void) => {
  const [formData, setFormData] = useState<CreateResourceRequest>({
    key: '',
    name: '',
    description: '',
    parentId: undefined,
    icon: '',
    type: 'Page',
    category: 'Admin',
    displayOrder: 0,
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleChange = (field: keyof CreateResourceRequest, value: any) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setLoading(true);
      setError(null);
      const res = await resourceApi.createResource(formData);
      console.log("Resource created:", res);
      onSuccess?.();
    } catch (err: any) {
      setError(err?.response?.data || "ایجاد منبع ناموفق بود");
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