// src/modules/Authorization/hooks/Forms/usePermissionRuleManagementForm.ts
import { useState, useEffect } from "react";
import { permissionRuleApi } from "../../../api/PermissionRuleApi";
import type { PermissionRuleDto } from "../../../models/PermissionRuleDto";

import { useNavigate } from 'react-router-dom';
import { GetPermissionsRuleQuery } from "@/modules/Authorization/models/PermissionRuleQuery";

export const usePermissionRuleManagement = () => {
  const [FormData, setFormData] = useState<PermissionRuleDto[]>([]);
  const [filters, setFilters] = useState<GetPermissionsRuleQuery | null>({
        PermissionId: ''
      });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  
 const deleteNode = async (id: string) => {
    try {
      await permissionRuleApi.deletePermissionRule(id);
      await fetchFormData(); // رفرش درخت بعد از حذف
    } catch (err: any) {
      throw err?.response?.data || "حذف ناموفق بود";
    }
  };
 const editNode = async (id: string) => {
    try {
       navigate(`/authorization/PermissionRules/edit/${id}`)
    } catch (err: any) {
      throw err?.response?.data || "ویرایش ناموفق بود";
    }
  };
   const addNode = async (id: string) => {
    try {
       navigate(`/authorization/PermissionRules/create/${id}`)
    } catch (err: any) {
      throw err?.response?.data || "ویرایش ناموفق بود";
    }
  };
  const fetchFormData = async (req?: GetPermissionsRuleQuery) => {
    try {
      setLoading(true);
      const data = await permissionRuleApi.getPermissionRules(filters);
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