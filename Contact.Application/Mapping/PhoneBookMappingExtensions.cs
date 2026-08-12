using Contact.Application.DTOs;
using Contact.Domain.Entities;
using Contact.Domain.Enums;
using System.Net.Mime;


namespace Contact.Application.Mapping
{
    
    public static class PhoneBookMappingExtensions
    {
        public static List<PhoneBookEmploymentDto> ToPhoneBookDtos(this IEnumerable<PhoneBookInfoView> rawList)
        {
            return rawList
                .GroupBy(x => x.EmploymentCode)
                .Select(group =>
                {
                    var first = group.First();

                    var employment = new PhoneBookEmploymentDto
                    {
                        EmploymentCode = first.EmploymentCode,
                        FirstName = first.FirstName,
                        LastName = first.LastName,
                        OrganizationUnitsName = first.OrganizationUnitsName,
                        JobTitleName = first.JobTitleName,
                        JobLevelTitle = first.JobLevelTitle,
                        LocationTitle = first.LocationTitle,
                        Contacts = new List<ContactDetailDto>()
                    };

                    foreach (var row in group)
                    {
                        // ۱. افزودن تماس‌های شخصی
                        //AddContact(employment.Contacts, "موبایل شخصی", row.PartyMobile, PhoneBookContactTypeEnum.Mobile, PhoneBookContactSourceEnum.Personal);
                        //AddContact(employment.Contacts, "تلفن ثابت شخصی", row.PartyPhone, PhoneBookContactTypeEnum.Phone, PhoneBookContactSourceEnum.Personal);
                        //AddContact(employment.Contacts, "ایمیل شخصی", row.PartyEmail, PhoneBookContactTypeEnum.Email, PhoneBookContactSourceEnum.Personal);
                        //AddContact(employment.Contacts, "آدرس منزل", row.PartyAddress, PhoneBookContactTypeEnum.Address, PhoneBookContactSourceEnum.Personal);

                        // ۲. افزودن تماس‌های پست سازمانی
                        AddContact(employment.Contacts, "داخلی", row.PostContactPhone, PhoneBookContactTypeEnum.Phone, PhoneBookContactSourceEnum.post);
                        AddContact(employment.Contacts, "موبایل سازمانی", row.PostContactMobile, PhoneBookContactTypeEnum.Mobile, PhoneBookContactSourceEnum.post);
                        //AddContact(employment.Contacts, "ایمیل سازمانی", row.PostContactEmail, PhoneBookContactTypeEnum.Email, PhoneBookContactSourceEnum.Organizational);
                        //AddContact(employment.Contacts, "فکس", row.PostContactFax, PhoneBookContactTypeEnum.Fax, PhoneBookContactSourceEnum.Organizational);

                        // 3. افزودن تماس‌های سازمانی کارمند
                        AddContact(employment.Contacts, "داخلی", row.EmploymentContactPhone, PhoneBookContactTypeEnum.Phone, PhoneBookContactSourceEnum.employment);
                        AddContact(employment.Contacts, "موبایل سازمانی", row.EmploymentContactMobile, PhoneBookContactTypeEnum.Mobile, PhoneBookContactSourceEnum.employment);
                        //AddContact(employment.Contacts, "ایمیل سازمانی", row.PostContactEmail, PhoneBookContactTypeEnum.Email, PhoneBookContactSourceEnum.Organizational);
                        //AddContact(employment.Contacts, "فکس", row.PostContactFax, PhoneBookContactTypeEnum.Fax, PhoneBookContactSourceEnum.Organizational);


                    }

                    return employment;
                })
                .ToList();
        }

        private static void AddContact(
            List<ContactDetailDto> list,
            string title,
            string? value,
            PhoneBookContactTypeEnum type,
            PhoneBookContactSourceEnum source)
        {
            if (string.IsNullOrWhiteSpace(value)) return;

            var cleanValue = value.Trim();

            // جلوگیری از تکراری شدن مقدار در صورتی که ویو چند سطر تکراری تولید کرده باشد
            if (!list.Any(c => c.Type == type && c.Source == source && c.Value == cleanValue))
            {
                list.Add(new ContactDetailDto
                {
                    Title = title,
                    Value = cleanValue,
                    Type = type,
                    Source = source
                });
            }
        }
    }
}
