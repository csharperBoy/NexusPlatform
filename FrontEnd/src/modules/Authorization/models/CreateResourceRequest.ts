// src/modules/Authorization/models/CreateResourceRequest.ts
export interface CreateResourceRequest {
  key: string;
  name: string;
  description?: string;
  parentId?: string;
  icon?: string;
  type: string;      // "Module", "Page", "Action", ...
  category: string;  // "Admin", "Public", ...
  displayOrder: number;
}