// src/modules/Authorization/hooks/Forms/useResourceCreateForm.ts
import { useState } from "react";
import { resourceApi } from "../../api/ResourcesApi";
import type { CreateResourceRequest,CreateResourceApiRequest } from "../../models/CreateResourceRequest";
import { useParams } from "react-router-dom";

export const useResourceCreateForm = (onSuccess?: () => void) => {
  
  const { parentId } = useParams<{ parentId: string }>();
  const [formData, setFormData] = useState<CreateResourceRequest>({
    key: '',
    name: '',
    description: '',
    parentId: parentId,
    icon: '',
    type: 'Module',
    category: 'General',
    displayOrder: 0,
    route: '',
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

      // مپ کردن مقادیر رشته‌ای به اعداد enum
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

      const typeValue = typeMap[formData.type] ?? 0; 
      const categoryValue = categoryMap[formData.category] ?? 0; 

      // ساخت payload منطبق با بک‌اند
      const payload: CreateResourceApiRequest = {
        key: formData.key,
        name: formData.name,
        type: typeValue,
        category: categoryValue,
        parentId: formData.parentId || null,
        description: formData.description || "",
        displayOrder: formData.displayOrder,
        icon: formData.icon || "",
        route: formData.route || "",
      };

      const res = await resourceApi.createResource(payload);
      console.log("Resource created:", res);
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