//src/modules/contact/models/ContactResourceCommand.ts
export interface UpdateContactResourceCommand {
  id: string; // Guid
  value?: string| null;
  label?: string | null;
  ContactType?: ContactTypeEnum| null;
  IsPrimary?: boolean| null;
  SortOrder?: number | null;
  RelationType?: ContactRelationTypeEnum | null;
  parentId?:string | null;
}

       


export interface CreateContactResourceCommand {
  value: string;
  label?: string | null;
  ContactType: ContactTypeEnum;
  IsPrimary: boolean;
  SortOrder?: number | null;
  RelationType?: ContactRelationTypeEnum | null;
  parentId?:string | null;
}


export enum ContactTypeEnum {
 Mobile = 1,

  Phone = 2,

 OfficePhone = 3,

 OrganizationMobile = 4,

 Email = 5,

 Fax = 6,

 Website = 7,

 WhatsApp = 8,

 Instagram = 9,

 Telegram = 10,

 LinkedIn = 11,

 Address = 12,

 PostalCode = 13,

 Eitaa = 14,

 x = 15,

 Other = 99
}


export enum ContactRelationTypeEnum {
    PublicPhoneNumber = 1, // نگاشت به شماره همگانی/عمومی
 ExtensionOf = 2,    // داخلیِ متصل به خط اصلی
 Alternative = 3  // کانال جایگزین / پشتیبان
    
}