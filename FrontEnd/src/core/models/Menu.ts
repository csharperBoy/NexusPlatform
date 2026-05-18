export interface MenuDto {
  id: string;
  title: string;
  description: string;
  path: string;
  icon: string; // مثلاً "fa-solid:folder"
  order: number;
  parentId?: string | null;
  children?: MenuDto[] | null;
}