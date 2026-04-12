// src/modules/Authorization/hooks/Forms/useResourceManagementForm.ts
import { useState, useEffect } from "react";
import { resourceApi } from "../../api/ResourcesApi";
import type { ResourceDto } from "../../models/ResourceDto";

import { useNavigate } from 'react-router-dom';

export const useResourceManagement = () => {
  const [treeData, setTreeData] = useState<ResourceDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  
 const deleteNode = async (id: string) => {
    try {
      await resourceApi.deleteResource(id);
      await fetchTree(); // رفرش درخت بعد از حذف
    } catch (err: any) {
      throw err?.response?.data || "حذف ناموفق بود";
    }
  };
 const editNode = async (id: string) => {
    try {
       navigate(`/resources/edit/${id}`)
    } catch (err: any) {
      throw err?.response?.data || "ویرایش ناموفق بود";
    }
  };
  const fetchTree = async (rootId?: string) => {
    try {
      setLoading(true);
      const data = await resourceApi.getTree(rootId);
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
  };
};