import { useState, useEffect, useCallback } from "react";
import { userApi } from "../../api/userApi";
import type { UpdateUserCommand } from "../../models/UpdateUserCommand";
import { useParams } from "react-router-dom";

export const useUserUpdateForm = (onSuccess?: () => void) => {
const { id } = useParams<{ id: string }>();
const [formData, setFormData] = useState<UpdateUserCommand | null>(null);
const [loading, setLoading] = useState(false);
const [error, setError] = useState<string | null>(null);
const [initialLoading, setInitialLoading] = useState(true);

const fetchUserById = useCallback(async (UserId: string) => {
    setError(null);
    try {
        const response = await userApi.getById(UserId);
        const loadedData: UpdateUserCommand = {
            Id: response.id,
            UserName: response.userName,
            Email: response.email,
            NickName:response.nickName,
            phoneNumber: response.phoneNumber,
            personId:response.person?.id || null
        };
        setFormData(loadedData);
    } catch (err: any) {
        console.error("Error fetching User:", err);
        setError(err?.response?.data?.message || err?.message || "خطا در بارگذاری اطلاعات کاربر");
        setFormData(null); 
    } finally {
        
    }
}, [id]); 
useEffect(() => {
    setInitialLoading(true);
    setError(null); 

    if (id) {
        console.info('Fetching User with ID:', id);
        fetchUserById(id)
            .then(() => {
                // اینجا لازم نیست کاری انجام دهیم، چون fetchUserById خودش setFormData را انجام می‌دهد
            })
            .catch(() => {
                // خطا قبلاً در fetchUserById مدیریت شده است
            })
            .finally(() => {
                setInitialLoading(false); 
            });
    } else {
        console.info('No ID provided, setting up for new User creation.');
        setFormData({
            Id: '',
           UserName :'',
           Email:'',
           NickName:'',
           Password:'',
           phoneNumber: '',
           personId: null
        });
        setInitialLoading(false); 
    }
    }, [id, fetchUserById]); 

const handleChange = (field: keyof UpdateUserCommand, value: any) => {
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
console.log("User :", formData);
        const res = await userApi.updateUser(formData);
        console.log("User Updated:", res);
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