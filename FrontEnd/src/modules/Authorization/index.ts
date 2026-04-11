// src/modules/Authorization/index.ts
export * from "./api/ResourcesApi";
export { authorizationPanelRoutes } from "./routes";

// ResourceCreatePage
export { ResourceCreateWithCustomForm } from './components/CustomPage/ResourceCreatePage';
export type { 
  RenderFormProps as RenderResourceCreateFormProps, 
  ResourceCreatePageWithCustomFormProps 
} from './components/CustomPage/ResourceCreatePage';
export { default as ResourceCreatePage } from "./pages/ResourceCreatePage";
export { useResourceCreateForm } from "./hooks/Forms/useResourceCreateForm";

// ResourceUpdatePage
export { ResourceUpdateWithCustomForm } from './components/CustomPage/ResourceUpdatePage';
export type { 
  RenderFormProps as RenderResourceUpdateFormProps, 
  ResourceUpdatePageWithCustomFormProps 
} from './components/CustomPage/ResourceUpdatePage';
export { default as ResourceUpdatePage } from "./pages/ResourceUpdatePage";
export { useResourceUpdateForm } from "./hooks/Forms/useResourceUpdateForm";

// ResourceManagementPage
export { ResourceManagementWithCustomForm } from './components/CustomPage/ResourceManagementPage';
export type { 
  RenderFormProps as RenderResourceManagementFormProps, 
  ResourceManagementPageWithCustomFormProps 
} from './components/CustomPage/ResourceManagementPage';
export { default as ResourceManagementPage } from './pages/ResourceManagementPage';
export { useResourceManagement } from './hooks/Forms/useResourceManagementForm';