// src/modules/Authorization/hooks/Forms/usePermissionManagementForm.ts
import { useState, useEffect } from "react";
import { permissionApi } from "../../../api/PermissionApi";
import type { PermissionDto } from "../../../models/PermissionDto";

import { useNavigate } from 'react-router-dom';
import { GetPermissionsQuery } from "@/modules/Authorization/models/PermissionQuery";

export const usePermissionManagement = () => {
  const [FormData, setFormData] = useState<PermissionDto[]>([]);
  const [filters, setFilters] = useState<GetPermissionsQuery | null>({
        AssigneeId: '',
        description: '',
        AssigneeType: null,
        ResourceId: null
      });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  
 const deleteNode = async (id: string) => {
    try {
      await permissionApi.deletePermission(id);
      await fetchFormData(); // رفرش درخت بعد از حذف
    } catch (err: any) {
      throw err?.response?.data || "حذف ناموفق بود";
    }
  };
 const editNode = async (id: string) => {
    try {
       navigate(`/permissions/edit/${id}`)
    } catch (err: any) {
      throw err?.response?.data || "ویرایش ناموفق بود";
    }
  };
   const addNode = async (id: string) => {
    try {
       navigate(`/permissions/create/${id}`)
    } catch (err: any) {
      throw err?.response?.data || "ویرایش ناموفق بود";
    }
  };
  const fetchFormData = async (req?: GetPermissionsQuery) => {
    try {
      setLoading(true);
      const data = await permissionApi.getPermissions(filters);
      setFormData(data);
    } catch (err: any) {
      setError(err?.response?.data || "دریافت  ناموفق بود");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchFormData();
  }, []);

  return {
    FormData,
    filters,
    loading,
    error,
    refresh: fetchFormData,
    deleteNode,
    editNode,
    addNode,
  };
};