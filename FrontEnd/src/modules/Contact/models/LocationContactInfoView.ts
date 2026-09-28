import { HierarchicalEntity } from "@/core/models/HierarchicalEntity";

 //src/modules/HR/models/LocationContactInfoView.ts
 export interface LocationContactInfoView extends HierarchicalEntity {
  
  title: string;
  
  orgMobile?: string[] | null;
  orgPhone?: string[] | null;
}


