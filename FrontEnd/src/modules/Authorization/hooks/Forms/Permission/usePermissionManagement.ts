import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { permissionApi } from '@/modules/Authorization/api/PermissionApi';
import { PermissionDto } from '@/modules/Authorization/models/PermissionDto';
import { GetPermissionsQuery } from '@/modules/Authorization/models/PermissionQuery';
import { SelectionListDto } from '@/core/models/SelectionListDto';
import { personApi } from '@/modules/HR/api/personApi';
import { postApi } from '@/modules/HR/api/PostApi';
import { roleApi } from '@/modules/Identity/api/roleApi';
import { userApi } from '@/modules/Identity/api/userApi';
import { resourceApi } from '@/modules/Authorization/api/ResourceApi';

export const usePermissionManagement = () => {
  const navigate = useNavigate();

  // States
  const [permissions, setPermissions] = useState<PermissionDto[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [deleteConfirmId, setDeleteConfirmId] = useState<string | null>(null);

  // Filter State
  const [filters, setFilters] = useState<GetPermissionsQuery>({
    assigneeType: null,
    assigneeId: null,
    resourceId: null,
    description: '',
  });

  // Maps for displaying labels instead of raw IDs
  const [assigneeMaps, setAssigneeMaps] = useState<Record<string, string>>({});
  const [resourceMap, setResourceMap] = useState<Record<string, string>>({});
  const [resourcesList, setResourcesList] = useState<SelectionListDto[]>([]);

  // 1. Fetch Resources List for Filter & Mapping
  const fetchResources = useCallback(async () => {
    try {
      if (resourceApi && typeof resourceApi.GetSelectionList === 'function') {
        const data = await resourceApi.GetSelectionList();
        setResourcesList(data || []);
        const map: Record<string, string> = {};
        (data || []).forEach((item) => {
          map[item.value] = item.display || item.label;
        });
        setResourceMap(map);
      }
    } catch (error) {
      console.error('خطا در دریافت لیست منابع:', error);
    }
  }, []);

  // 2. Fetch Assignee Labels (Persons, Posts, Roles, Users)
  const fetchAssigneesMap = useCallback(async () => {
    try {
      const [persons, posts, roles, users] = await Promise.allSettled([
        personApi.GetSelectionList(),
        postApi.GetSelectionList(),
        roleApi.GetSelectionList(),
        userApi.GetSelectionList(),
      ]);

      const newMap: Record<string, string> = {};

      const processResult = (result: PromiseSettledResult<SelectionListDto[]>) => {
        if (result.status === 'fulfilled' && Array.isArray(result.value)) {
          result.value.forEach((item) => {
            newMap[item.value] = item.display || item.label;
          });
        }
      };

      processResult(persons);
      processResult(posts);
      processResult(roles);
      processResult(users);

      setAssigneeMaps(newMap);
    } catch (error) {
      console.error('خطا در دریافت اطلاعات دریافت‌کنندگان دسترسی:', error);
    }
  }, []);

  // 3. Fetch Permissions List
  const fetchPermissions = useCallback(async (queryFilters?: GetPermissionsQuery) => {
    setLoading(true);
    try {
      const targetQuery = queryFilters !== undefined ? queryFilters : filters;
      const cleanQuery: GetPermissionsQuery = {
        assigneeType: targetQuery.assigneeType !== null ? Number(targetQuery.assigneeType) : null,
        assigneeId: targetQuery.assigneeId || null,
        resourceId: targetQuery.resourceId || null,
        description: targetQuery.description || null,
      };

      const data = await permissionApi.getPermissions(cleanQuery);
      setPermissions(data || []);
    } catch (error) {
      console.error('خطا در دریافت لیست مجوزها:', error);
    } finally {
      setLoading(false);
    }
  }, [filters]);

  useEffect(() => {
    fetchResources();
    fetchAssigneesMap();
    fetchPermissions();
  }, []);

  // Actions
  const handleFilterChange = (field: keyof GetPermissionsQuery, value: any) => {
    setFilters((prev) => ({ ...prev, [field]: value }));
  };

  const handleApplyFilter = () => {
    fetchPermissions(filters);
  };

  const handleResetFilter = () => {
    const emptyFilters: GetPermissionsQuery = {
      assigneeType: null,
      assigneeId: null,
      resourceId: null,
      description: '',
    };
    setFilters(emptyFilters);
    fetchPermissions(emptyFilters);
  };

  const handleNavigateToCreate = (resourceId?: string) => {
    if (resourceId) {
      navigate(`/authorization/permissions/create/${resourceId}`);
    } else {
      navigate('/authorization/permissions/create');
    }
  };

  const handleNavigateToEdit = (id: string) => {
    navigate(`/authorization/permissions/edit/${id}`);
  };

  const handleDelete = async () => {
    if (!deleteConfirmId) return;
    setDeletingId(deleteConfirmId);
    try {
      const success = await permissionApi.deletePermission(deleteConfirmId);
      if (success) {
        setPermissions((prev) => prev.filter((p) => p.id !== deleteConfirmId));
      }
    } catch (error) {
      console.error('خطا در حذف مجوز:', error);
    } finally {
      setDeletingId(null);
      setDeleteConfirmId(null);
    }
  };

  // Helper functions to get display names
  const getAssigneeName = (assigneeId: string) => {
    return assigneeMaps[assigneeId] || assigneeId;
  };

  const getResourceName = (resourceId?: string, resourceKey?: string) => {
    if (resourceKey) return resourceKey;
    if (resourceId && resourceMap[resourceId]) return resourceMap[resourceId];
    return resourceId || '-';
  };

  return {
    permissions,
    loading,
    filters,
    resourcesList,
    deleteConfirmId,
    deletingId,
    setDeleteConfirmId,
    handleFilterChange,
    handleApplyFilter,
    handleResetFilter,
    handleNavigateToCreate,
    handleNavigateToEdit,
    handleDelete,
    getAssigneeName,
    getResourceName,
  };
};