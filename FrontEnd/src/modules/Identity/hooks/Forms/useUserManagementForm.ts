// src/modules/Identity/hooks/Forms/useUserManagementForm.tsx
import { useState, useEffect } from "react";
import { userApi } from "../../api/userApi";
import type { UserDto } from "../../models/UserDto";

import { useNavigate } from 'react-router-dom';
import { GetUsersQuery } from "../../models/GetUsersQuery";

export const useUserManagement = () => {
  const [Data, setData] = useState<UserDto[]>([]);
  const [filters, setFilters] = useState<GetUsersQuery | null>({
      UserName: '',
      phoneNumber: '',
      NickName: '',
    });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  
 const deleteAction = async (id: string) => {
    try {
       await userApi.deleteUser(id);
      await fetchData(); // رفرش درخت بعد از حذف
    } catch (err: any) {
      throw err?.response?.data || "حذف ناموفق بود";
    }
  };
 const editAction = async (id: string) => {
    try {
       navigate(`/user/edit/${id}`)
    } catch (err: any) {
      throw err?.response?.data || "ویرایش ناموفق بود";
    }
  };
   const addAction = async (id: string) => {
    try {
       navigate(`/user/create/${id}`)
    } catch (err: any) {
      throw err?.response?.data || "ویرایش ناموفق بود";
    }
  };
  const fetchData = async (req?: GetUsersQuery) => {
    try {
      setLoading(true);
      const data = await userApi.getUsers(filters);
      setData(data);
    } catch (err: any) {
      setError(err?.response?.data || "دریافت منابع ناموفق بود");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  return {
    Data,
    filters,
    loading,
    error,
    refresh: fetchData,
    deleteAction,
    editAction,
    addAction,
  };
};