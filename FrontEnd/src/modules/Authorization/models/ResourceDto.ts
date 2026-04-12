// src/modules/Authorization/models/ResourceDto.ts
export interface ResourceDto {
  id: string;
  key: string;
  name: string;
  description: string;
  parentId?: string;
  icon?: string;
  type: number;
  category: number;
  displayOrder: number;
  isActive: boolean;
  path?: string;
  children?: ResourceDto[]; // برای ساختار درختی
}