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
