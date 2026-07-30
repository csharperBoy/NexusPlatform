// models/PhoneBookEmployeeDto.ts

export interface PhoneBookEmployeeDto {
  employeeCode: string;
  postCode: string;
  firstName?: string | null;
  lastName?: string | null;
  fullName?: string | null;
  organizationUnitsName?: string | null;
  jobTitleName?: string | null;
  jobLevelTitle?: string | null;
  locationTitle?: string | null;
  
  contactSummary?: string | null;
  hasMultipleContacts?: boolean | false;

  contacts?: ContactDetailDto[] | null;
}
export interface ContactDetailDto {
  title: string;
  value: string;
  type?: PhoneBookContactTypeEnum | null;
  source?: PhoneBookContactSourceEnum | null;
 

}

export enum PhoneBookContactTypeEnum {
  Mobile,
    Phone,
    Email,
    Fax,
    Address
 

}

export enum PhoneBookContactSourceEnum {
 Personal,   
      Organizational 
 

}
