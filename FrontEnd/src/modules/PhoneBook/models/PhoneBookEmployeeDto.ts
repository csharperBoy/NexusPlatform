// models/PhoneBookEmployeeDto.ts

export interface PhoneBookEmployeeDto {
  EmployeeCode: string;
  postCode: string;
  FirstName?: string | null;
  LastName?: string | null;
  FullName?: string | null;
  OrganizationUnitsName?: string | null;
  JobTitleName?: string | null;
  JobLevelTitle?: string | null;
  LocationTitle?: string | null;
  
  ContactSummary?: string | null;
  HasMultipleContacts?: boolean | false;

  Contacts?: ContactDetailDto[] | null;
}
export interface ContactDetailDto {
  Title: string;
  Value: string;
  Type?: PhoneBookContactTypeEnum | null;
  Source?: PhoneBookContactSourceEnum | null;
 

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
