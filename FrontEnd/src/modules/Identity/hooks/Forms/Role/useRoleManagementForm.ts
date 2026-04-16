// src/modules/Identity/hooks/Forms/useRoleManagementForm.tsx
import { useState, useEffect } from "react";
import { roleApi } from "../../../api/roleApi";
import type { RoleDto } from "../../../models/RoleDto";

import { useNavigate } from 'react-router-dom';
import { GetRolesQuery } from "../../../models/GetRolesQuery";

export const useRoleManagement = () => {
  const [Data, setData] = useState<RoleDto[]>([]);
  const [filters, setFilters] = useState<GetRolesQuery | null>({
      Name: '',
      description: '',
    });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  
 const deleteAction = async (id: string) => {
    try {
       await roleApi.deleteRole(id);
      await fetchData(); // رفرش درخت بعد از حذف
    } catch (err: any) {
      throw err?.response?.data || "حذف ناموفق بود";
    }
  };
 const editAction = async (id: string) => {
    try {
       navigate(`/role/edit/${id}`)
    } catch (err: any) {
      throw err?.response?.data || "ویرایش ناموفق بود";
    }
  };
   const addAction = async (id: string) => {
    try {
       navigate(`/role/create/${id}`)
    } catch (err: any) {
      throw err?.response?.data || "ویرایش ناموفق بود";
    }
  };
  const fetchData = async (req?: GetRolesQuery) => {
    try {
      setLoading(true);
      const data = await roleApi.getRoles(filters);
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