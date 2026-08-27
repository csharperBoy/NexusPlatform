// models/PhoneBookEmploymentDto.ts

export interface PhoneBookEmploymentDto {
  uniqueKey: string; 
  employmentCode: string;
  postCode: string;
  firstName?: string | null;
  lastName?: string | null;
  fullName?: string | null;
  
  organizationUnitsName: string[];
  headOfOrganizationUnitsName: string[];
  jobTitleName: string[];
  jobLevelTitle: string[];
  locationTitle: string[];
  
  
  profilePictureUrl?: string | null;
  gender?: GenderEnum | null;

  contactSummary?: string | null;
  // hasMultipleContacts?: boolean | false;

  contacts?: ContactDetailDto[] | null;
}
export interface ContactDetailDto {
  title: string;
  value: string;
  type?: ContactTypeEnum | null;
  source?: ContactSourceEnum | null;
 

}
export enum GenderEnum {
  Male = 1,
  Female = 2,
  Other = 3
}
export enum ContactTypeEnum {
  Mobile = 1 ,
    Phone = 2,
    OfficePhone = 3,
    OrganizationMobile = 4,
    Email = 5,
 Fax = 6,            // فکس
 Website = 7,        // وب‌سایت
 WhatsApp = 8,       // واتس‌اپ
 Instagram = 9,      // اینستاگرام
 Telegram = 10,       // تلگرام
 LinkedIn = 11,      // لینکدین
 Address = 12, // آدرس پستی یا لوکیشن
 PostalCode = 13, // کد پستی
 Other = 99          // سایر راه ارتباطی
}

export enum ContactSourceEnum {
 Personal= 0,   
      post= 1, 
      employment= 2, 
      location = 3 
 
}
