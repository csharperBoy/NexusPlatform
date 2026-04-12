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
    3: 'Page',
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

// fetchResourceById باید فقط به id وابسته باشد
const fetchResourceById = useCallback(async (resourceId: string) => {
    // setInitialLoading(true); // این را به useEffect منتقل می‌کنیم تا فقط یک بار در ابتدای بارگذاری اجرا شود
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
            // این مقادیر باید بر اساس response.type و response.category مقداردهی شوند،
            // اگر response.type عدد است، از typeMapReverse استفاده کنید.
            // فرض می‌کنیم response.type و response.category از API به صورت رشته برمی‌گردند یا باید تبدیل شوند.
            type: response.type || 'Page',
            category: response.category || 'General',
            displayOrder: response.displayOrder,
        };
        setFormData(loadedData);
        // setError(null); // خطا را اینجا پاک نکنید، اگر fetch موفق بود، خطایی نیست
    } catch (err: any) {
        console.error("Error fetching resource:", err);
        setError(err?.response?.data?.message || err?.message || "خطا در بارگذاری اطلاعات منبع");
        setFormData(null); // اگر خطا رخ داد، formData را null کنید
    } finally {
        // setInitialLoading(false); // این را به useEffect منتقل می‌کنیم
    }
    // چون setInitialLoading و setError و setFormData وضعیت‌هایی هستند که بیرون این useCallback تعریف شده‌اند،
    // و ما فقط به id وابسته هستیم، نیازی به قرار دادن آن‌ها در وابستگی useCallback نیست.
}, [id]); // وابستگی اصلی id است

useEffect(() => {
    // مدیریت بارگذاری اولیه
    setInitialLoading(true);
    setError(null); // خطای قبلی را پاک می‌کنیم

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
                setInitialLoading(false); // بارگذاری اولیه تمام شد
            });
    } else {
        console.info('No ID provided, setting up for new resource creation.');
        // مقداردهی اولیه برای حالت افزودن
        setFormData({
            id: '',
            key: '',
            name: '',
            description: '',
            parentId: null,
            icon: '',
            type: 'Page', // مقدار پیش‌فرض
            category: 'Admin', // مقدار پیش‌فرض
            displayOrder: 0,
            route: '', // اضافه شده بر اساس payload
        });
        setInitialLoading(false); // بارگذاری اولیه تمام شد (چون فرم آماده است)
    }
    // وابستگی useEffect فقط id است.
    // fetchResourceById خودش یک useCallback است که به id وابسته است.
    // قرار دادن fetchResourceById در وابستگی useEffect باعث می‌شود هر بار که fetchResourceById رفرنسی می‌گیرد، useEffect دوباره اجرا شود.
}, [id, fetchResourceById]); // <<<== فقط id کافی است، اما اگر fetchResourceById را نیز اضافه کنید، این مورد مهم است که fetchResourceById فقط به id وابسته باشد

// ... بقیه هوک (handleChange, handleSubmit)

const handleChange = (field: keyof UpdateResourceRequest, value: any) => {
    // این بخش درست به نظر می‌رسد
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

        // این مپ‌ها چون ثابت هستند، بهتر است بیرون از تابع handleSubmit تعریف شوند
        // یا اگر فقط اینجا استفاده می‌شوند، همینجا باشند.
        const typeMap: Record<string, number> = {
            'Module': 0,
            'Ui': 1,
            'Data': 2,
            'Page': 3, // اضافه شده
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
            // 'Admin' باید اینجا اضافه شود اگر در formData استفاده شده
            'Admin': 8, // فرض کنید Admin کد 8 دارد
        };

        // اگر مقادیر type یا category در نقشه‌های بالا وجود نداشتند، مقدار پیش‌فرض 0 برگردانده می‌شود.
        // این ممکن است رفتار دلخواه نباشد. بهتر است خطا بدهید یا مقدار پیش‌فرض مناسب‌تری بگذارید.
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