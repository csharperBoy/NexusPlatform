// src/modules/Authorization/index.ts
export * from "./api/ResourcesApi";
export { authorizationPanelRoutes } from "./routes";

// ResourceCreatePage
export { ResourceCreateForm } from './Interface/IResourceCreatePage';
export type { 
  RenderFormProps as RenderResourceCreateFormProps, 
  IResourceCreatePageProps 
} from './Interface/IResourceCreatePage';
export { default as ResourceCreatePage } from "./pages/ResourceCreatePage";
export { useResourceCreateForm } from "./hooks/Forms/useResourceCreateForm";

// ResourceUpdatePage
export { ResourceUpdateForm } from './Interface/IResourceUpdatePage';
export type { 
  RenderFormProps as RenderResourceUpdateFormProps, 
  IResourceUpdatePageProps
} from './Interface/IResourceUpdatePage';
export { default as ResourceUpdatePage } from "./pages/ResourceUpdatePage";
export { useResourceUpdateForm } from "./hooks/Forms/useResourceUpdateForm";

// ResourceManagementPage
export { ResourceManagementForm } from './Interface/IResourceManagementPage';
export type { 
  RenderFormProps as RenderResourceManagementFormProps, 
  IResourceManagementPageProps 
} from './Interface/IResourceManagementPage';
export { default as ResourceManagementPage } from './pages/ResourceManagementPage';
export { useResourceManagement } from './hooks/Forms/useResourceManagementForm';