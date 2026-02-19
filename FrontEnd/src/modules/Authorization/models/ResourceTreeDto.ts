// src/modules/Authorization/models/ResourceTreeDto.ts
export interface ResourceTreeDto {
  id: string;
  key: string;
  name: string;
  description: string;
  parentId?: string;
  icon?: string;
  type: string;
  category: string;
  displayOrder: number;
  isActive: boolean;
  path?: string;
  children?: ResourceTreeDto[]; // برای ساختار درختی
}