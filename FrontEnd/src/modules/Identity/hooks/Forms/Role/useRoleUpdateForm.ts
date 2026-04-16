import { useState, useEffect, useCallback } from "react";
import { roleApi } from "../../../api/roleApi";
import type { UpdateRoleCommand } from "../../../models/UpdateRoleCommand";
import { useParams } from "react-router-dom";

export const useRoleUpdateForm = (onSuccess?: () => void) => {
const { id } = useParams<{ id: string }>();
const [formData, setFormData] = useState<UpdateRoleCommand | null>(null);
const [loading, setLoading] = useState(false);
const [error, setError] = useState<string | null>(null);
const [initialLoading, setInitialLoading] = useState(true);

const fetchRoleById = useCallback(async (RoleId: string) => {
    setError(null);
    try {
        const response = await roleApi.getById(RoleId);
        const loadedData: UpdateRoleCommand = {
            Id: response.id,
            Name: response.name,
            Description: response.description,
            OrderNum:response.orderNum ,
        };
        setFormData(loadedData);
    } catch (err: any) {
        console.error("Error fetching Role:", err);
        setError(err?.response?.data?.message || err?.message || "خطا در بارگذاری اطلاعات نقش");
        setFormData(null); 
    } finally {
        
    }
}, [id]); 
useEffect(() => {
    setInitialLoading(true);
    setError(null); 

    if (id) {
        console.info('Fetching Role with ID:', id);
        fetchRoleById(id)
            .then(() => {
                // اینجا لازم نیست کاری انجام دهیم، چون fetchRoleById خودش setFormData را انجام می‌دهد
            })
            .catch(() => {
                // خطا قبلاً در fetchRoleById مدیریت شده است
            })
            .finally(() => {
                setInitialLoading(false); 
            });
    } else {
        console.info('No ID provided, setting up for new Role creation.');
        setFormData({
            Id: '',
           Name :'',
           Description:'',
           OrderNum:0,
        });
        setInitialLoading(false); 
    }
    }, [id, fetchRoleById]); 

const handleChange = (field: keyof UpdateRoleCommand, value: any) => {
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
console.log("Role :", formData);
        const res = await roleApi.updateRole(formData);
        console.log("Role Updated:", res);
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