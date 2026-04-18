// // src/modules/Authorization/models/UpdateResourceRequest.ts
// export interface UpdateResourceRequest {
//   id: string;
//   key: string;
//   name: string;
//   description?: string;
//   parentId?: string | null;
//   icon?: string;
//   type: string;      // "Module", "Ui", "Data"
//   category: string;  // "General", "System", "Module", "Menu", "Page", "Component", "DatabaseTable", "RowInTable"
//   displayOrder: number;
//   route?: string;    // اضافه شد
// }

// export interface UpdateResourceApiRequest {
//   id: string;
//   key: string;
//   name: string;
//   type: number;       // 0=Module, 1=Ui, 2=Data
//   category: number;   // 0=General, 1=System, 2=Module, 3=Menu, 4=Page, 5=Component, 6=DatabaseTable, 7=RowInTable
//   parentId?: string | null;
//   description?: string;
//   displayOrder: number;
//   icon?: string;
//   route?: string;
// }