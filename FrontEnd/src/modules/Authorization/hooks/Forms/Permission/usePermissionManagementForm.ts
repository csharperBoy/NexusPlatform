// src/modules/Authorization/hooks/Forms/usePermissionManagementForm.ts
import { useState, useEffect } from "react";
import { permissionApi } from "../../../api/PermissionApi";
import type { PermissionDto } from "../../../models/PermissionDto";

import { useNavigate } from 'react-router-dom';

export const usePermissionManagement = () => {
  const [treeData, setTreeData] = useState<PermissionDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  
 const deleteNode = async (id: string) => {
    try {
      await permissionApi.deletePermission(id);
      await fetchTree(); // رفرش درخت بعد از حذف
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
  const fetchTree = async (rootId?: string) => {
    try {
      setLoading(true);
      const data = await permissionApi.getTree(rootId);
      setTreeData(data);
    } catch (err: any) {
      setError(err?.response?.data || "دریافت منابع ناموفق بود");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchTree();
  }, []);

  return {
    treeData,
    loading,
    error,
    refresh: fetchTree,
    deleteNode,
    editNode,
    addNode,
  };
};