//src/modules/contact/models/ContactResourceInfoView.ts
import { HierarchicalEntity } from "@/core/models/HierarchicalEntity";
import { ContactRelationTypeEnum, ContactTypeEnum } from "./ContactResourceCommand";

 export interface ContactResourceInfoView extends HierarchicalEntity {
     contactType: ContactTypeEnum ;
     value: string ;
         label?: string| null ;
       isPrimary: boolean ;
         sortOrder?: number| null ;
         relationType?: ContactRelationTypeEnum| null ;
}