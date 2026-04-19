// src/modules/Authorization/index.ts
export * from "./api/ResourceApi";
export { authorizationPanelRoutes } from "./routes";


// ResourceManagementPage
export { ResourceManagementForm } from './Interface/Resource/IResourceManagementPage';
export type { 
  RenderFormProps as RenderResourceManagementFormProps, 
  IResourceManagementPageProps 
} from './Interface/Resource/IResourceManagementPage';
export { default as ResourceManagementPage } from './pages/Resource/ResourceManagementPage';
export { useResourceManagement } from './hooks/Forms/Resource/useResourceManagementForm';



// ResourceCreateUpdatePage
export { IResourceCreateUpdatePage } from './Interface/Resource/IResourceCreateUpdatePage';
export type { 
  RenderFormProps as RenderResourceFormProps, 
  IResourceCreateUpdatePageProps 
} from './Interface/Resource/IResourceCreateUpdatePage';
export { default as ResourceUpdatePage } from './pages/Resource/ResourceUpdatePage';
export { useResourceCreateUpdateForm } from './hooks/Forms/Resource/useResourceCreateUpdateForm';



// PermissionManagementPage
export { PermissionManagementForm } from './Interface/Permission/IPermissionManagementPage';
export type { 
  RenderFormProps as RenderPermissionManagementFormProps, 
  IPermissionManagementPageProps 
} from './Interface/Permission/IPermissionManagementPage';
export { default as PermissionManagementPage } from './pages/Permission/PermissionsManagementPage';
export { usePermissionManagement } from './hooks/Forms/Permission/usePermissionManagementForm';



// PermissionCreateUpdatePage
export { IPermissionCreateUpdatePage } from './Interface/Permission/IPermissionCreateUpdatePage';
export type { 
  RenderFormProps as RenderPermissionFormProps, 
  IPermissionCreateUpdatePageProps 
} from './Interface/Permission/IPermissionCreateUpdatePage';
export { default as PermissionUpdatePage } from './pages/Permission/PermissionUpdatePage';
export { usePermissionCreateUpdateForm } from './hooks/Forms/Permission/usePermissionCreateUpdateForm';
