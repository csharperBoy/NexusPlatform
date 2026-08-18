 //src/modules/HR/models/LocationInfoView.ts
 export interface LocationInfoView {
  id: string; // Guid
  title: string;
  
  orgMobile?: string | null;
  orgPhone?: string | null;
}


export interface Location {
  id: string;
  title: string;
  // سایر فیلدها در صورت نیاز
}