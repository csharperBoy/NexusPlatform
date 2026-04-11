// src/modules/Authorization/hooks/Forms/useResourceManagementForm.ts
import { useState, useEffect } from "react";
import { resourceApi } from "../../api/ResourcesApi";
import type { ResourceDto } from "../../models/ResourceDto";

export const useResourceManagement = () => {
  const [treeData, setTreeData] = useState<ResourceDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

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
  };
};