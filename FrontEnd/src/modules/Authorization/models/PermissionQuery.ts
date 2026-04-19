// modules/identity/models/PermissionQuery.ts
export interface GetPermissionsQuery {
  AssigneeType?: number | null;
  AssigneeId?: string | null;
  ResourceId?: string | null;
  description?: string | null;
}
