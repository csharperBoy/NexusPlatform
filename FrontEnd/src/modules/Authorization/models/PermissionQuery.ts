// modules/identity/models/PermissionQuery.ts
export interface GetPermissionsQuery {
  assigneeType?: number | null;
  assigneeId?: string | null;
  resourceId?: string | null;
  description?: string | null;
}
