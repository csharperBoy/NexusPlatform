using PhoneBook.Application.DTOs;
using PhoneBook.Domain.Entities;
using PhoneBook.Domain.Enums;
using System.Net.Mime;


namespace PhoneBook.Application.Mapping
{
    
    public static class PhoneBookMappingExtensions
    {
        public static List<PhoneBookEmployeeDto> ToPhoneBookDtos(this IEnumerable<PhoneBookInfoView> rawList)
        {
            return rawList
                .GroupBy(x => x.EmployeeCode)
                .Select(group =>
                {
                    var first = group.First();

                    var employee = new PhoneBookEmployeeDto
                    {
                        EmployeeCode = first.EmployeeCode,
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
                        AddContact(employee.Contacts, "موبایل شخصی", row.PartyMobile, PhoneBookContactTypeEnum.Mobile, PhoneBookContactSourceEnum.Personal);
                        AddContact(employee.Contacts, "تلفن ثابت شخصی", row.PartyPhone, PhoneBookContactTypeEnum.Phone, PhoneBookContactSourceEnum.Personal);
                        //AddContact(employee.Contacts, "ایمیل شخصی", row.PartyEmail, PhoneBookContactTypeEnum.Email, PhoneBookContactSourceEnum.Personal);
                        //AddContact(employee.Contacts, "آدرس منزل", row.PartyAddress, PhoneBookContactTypeEnum.Address, PhoneBookContactSourceEnum.Personal);

                        // ۲. افزودن تماس‌های پست سازمانی
                        AddContact(employee.Contacts, "داخلی", row.PostContactPhone, PhoneBookContactTypeEnum.Phone, PhoneBookContactSourceEnum.post);
                        AddContact(employee.Contacts, "موبایل سازمانی", row.PostContactMobile, PhoneBookContactTypeEnum.Mobile, PhoneBookContactSourceEnum.post);
                        //AddContact(employee.Contacts, "ایمیل سازمانی", row.PostContactEmail, PhoneBookContactTypeEnum.Email, PhoneBookContactSourceEnum.Organizational);
                        //AddContact(employee.Contacts, "فکس", row.PostContactFax, PhoneBookContactTypeEnum.Fax, PhoneBookContactSourceEnum.Organizational);

                        // 3. افزودن تماس‌های سازمانی کارمند
                        AddContact(employee.Contacts, "داخلی", row.EmploymentContactPhone, PhoneBookContactTypeEnum.Phone, PhoneBookContactSourceEnum.employment);
                        AddContact(employee.Contacts, "موبایل سازمانی", row.EmploymentContactMobile, PhoneBookContactTypeEnum.Mobile, PhoneBookContactSourceEnum.employment);
                        //AddContact(employee.Contacts, "ایمیل سازمانی", row.PostContactEmail, PhoneBookContactTypeEnum.Email, PhoneBookContactSourceEnum.Organizational);
                        //AddContact(employee.Contacts, "فکس", row.PostContactFax, PhoneBookContactTypeEnum.Fax, PhoneBookContactSourceEnum.Organizational);


                    }

                    return employee;
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
