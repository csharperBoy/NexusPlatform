// models/EmploymentCommand.ts



export interface UpdateEmploymentCommand {
  id: string; // Guid
  
Phone?: string[] | null;
Address?: string[] | null;
Email?: string[] | null;
Mobile?: string[] | null;
FirstName?: string | null;
LastName?: string | null; 
BirthDate?: Date | null;
BirthPlace?: string | null;
FatherName?: string | null;
nationalCode?: string | null;
EmploymentCode?: string | null;
EmploymentTypeId?: string | null;
EmploymentStatusId?: string | null;
StartDate?: Date | null;
EndDate?: Date | null;
locationsId?: string[] | null;

  officePhone?: string[] | null;
  orgEmail?: string[] | null;
  orgMobile?: string[] | null;
  PostId?: string | null;
  AssigneeType?: string | null;
  EffectiveFrom?: Date | null;
  EffectiveTo?: Date | null;
}



export interface CreateEmploymentCommand {
  id: string; // Guid
     Phone?: string[] | null;
     Address?: string[] | null;
      Email?: string[] | null;
      Mobile?: string[] | null;
NationalCode?: string | null;
FirstName?: string | null;
LastName?: string | null;
BirthDate?: Date | null;
BirthPlace?: string | null;
FatherName?: string | null;
Gender?: string | null;
EmploymentCode?: string | null;
EmploymentTypeId?: string | null;
EmploymentStatusId?: string | null;
StartDate?: Date | null;
EndDate?: Date | null;
locationsId?: string[] | null;
OfficePhone?: string[] | null;
OrgEmail?: string[] | null;
OrgMobile?: string[] | null;


   PostId?: string | null;
   AssigneeType?: string | null; 
   EffectiveFrom?: Date | null;
   EffectiveTo?: Date | null;
}
