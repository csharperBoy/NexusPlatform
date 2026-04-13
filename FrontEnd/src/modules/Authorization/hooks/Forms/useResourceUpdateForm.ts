import { useState, useEffect, useCallback } from "react";
import { resourceApi } from "../../api/ResourcesApi";
import type { UpdateResourceRequest, UpdateResourceApiRequest } from "../../models/UpdateResourceRequest";
import { useParams } from "react-router-dom";

export const useResourceUpdateForm = (onSuccess?: () => void) => {
const { id } = useParams<{ id: string }>();
const [formData, setFormData] = useState<UpdateResourceRequest | null>(null);
const [loading, setLoading] = useState(false);
const [error, setError] = useState<string | null>(null);
const [initialLoading, setInitialLoading] = useState(true);

// این مپ‌ها چون ثابت هستند، نیازی نیست در وابستگی useCallback باشند
const typeMapReverse: Record<number, string> = {
    0: 'Module',
    1: 'Ui',
    2: 'Data',
};
const categoryMapReverse: Record<number, string> = {
    0: 'General',
    1: 'System',
    2: 'Module',
    3: 'Menu',
    4: 'Page',
    5: 'Component',
    6: 'DatabaseTable',
    7: 'RowInTable',
};
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
const fetchResourceById = useCallback(async (resourceId: string) => {
    setError(null);
    try {
        const response = await resourceApi.getById(resourceId);
        const loadedData: UpdateResourceRequest = {
            id: response.id,
            key: response.key,
            name: response.name,
            description: response.description || "",
            parentId: response.parentId,
            icon: response.icon || "",
            type: typeMapReverse[response.type] , 
            category: categoryMapReverse[response.category] , 
               
            displayOrder: response.displayOrder,
        };
        console.info(response.type);
        setFormData(loadedData);
    } catch (err: any) {
        console.error("Error fetching resource:", err);
        setError(err?.response?.data?.message || err?.message || "خطا در بارگذاری اطلاعات منبع");
        setFormData(null); 
    } finally {
        
    }
}, [id]); 
useEffect(() => {
    setInitialLoading(true);
    setError(null); 

    if (id) {
        console.info('Fetching resource with ID:', id);
        fetchResourceById(id)
            .then(() => {
                // اینجا لازم نیست کاری انجام دهیم، چون fetchResourceById خودش setFormData را انجام می‌دهد
            })
            .catch(() => {
                // خطا قبلاً در fetchResourceById مدیریت شده است
            })
            .finally(() => {
                setInitialLoading(false); 
            });
    } else {
        console.info('No ID provided, setting up for new resource creation.');
        setFormData({
            id: '',
            key: '',
            name: '',
            description: '',
            parentId: null,
            icon: '',
            type: 'Page', 
            category: 'Admin', 
            displayOrder: 0,
            route: '', 
        });
        setInitialLoading(false); 
    }
    }, [id, fetchResourceById]); 

const handleChange = (field: keyof UpdateResourceRequest, value: any) => {
    setFormData(prev => {
        if (!prev) return null;
        return { ...prev, [field]: value };
    });
};

const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!formData) {
        setError("لطفاً اطلاعات منبع را بررسی کنید.");
        return;
    }
    try {
        setLoading(true);
        setError(null);

      

       const typeValue = typeMap[formData.type] !== undefined ? typeMap[formData.type] : 0;
        const categoryValue = categoryMap[formData.category] !== undefined ? categoryMap[formData.category] : 0;


        const payload: UpdateResourceApiRequest = {
            id: formData.id,
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

        const res = await resourceApi.updateResource(payload);
        console.log("Resource Updated:", res);
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
    initialLoading,
    handleChange,
    handleSubmit,
};


};