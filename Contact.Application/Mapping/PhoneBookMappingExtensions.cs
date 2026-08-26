using Contact.Application.DTOs;
using Contact.Domain.Entities;
using Contact.Domain.Enums;
using Core.Shared.DTOs.Contact;
using Core.Shared.DTOs.HR;
using Core.Shared.Enums;
using Core.Shared.Enums.Contact;
using System.Net.Mime;


namespace Contact.Application.Mapping
{

    public static class PhoneBookMappingExtensions
    {
        public static List<PhoneBookEmploymentDto> ToPhoneBookDtos(
    this IEnumerable<EmploymentFullDto> rawList,
    List<ContactItemDto> contactList)
        {
            // ایجاد Dictionary برای دسترسی سریع به کانتکت‌های هر ProfileId
            var contactsLookup = contactList
                .GroupBy(c => c.ProfileId)
                .ToDictionary(g => g.Key, g => g.ToList());

            return rawList.Select(emp =>
            {
                // جمع‌آوری تمام ProfileIdهای مرتبط با این کارمند
                var relevantProfileIds = new HashSet<Guid>();
                if (emp.ProfileId.HasValue) relevantProfileIds.Add(emp.ProfileId.Value);
                if (emp.PartyProfileId.HasValue) relevantProfileIds.Add(emp.PartyProfileId.Value);
                foreach (var post in emp.posts) relevantProfileIds.Add(post.ProfileId);
                foreach (var loc in emp.empLocations) relevantProfileIds.Add(loc.ProfileId);
                foreach (var loc in emp.postLocations) relevantProfileIds.Add(loc.ProfileId);

                // استخراج کانتکت‌ها از Lookup و حذف تکراری‌ها
                var contacts = relevantProfileIds
                    .SelectMany(id => contactsLookup.GetValueOrDefault(id, new List<ContactItemDto>()))
                    .GroupBy(c => new { c.ContactType, c.Value, c.Source }) // حذف تکراری‌ها
                    .Select(g => g.First())
                    .Select(c => new ContactDetailDto
                    {
                        //Title = c.Label ?? c.ContactType.ToString(), // عنوان پیش‌فرض
                        Title = string.IsNullOrWhiteSpace(c.Label) ? c.ContactType.GetPersianDescription() : c.Label,

                        Value = c.Value,
                        Type = c.ContactType,
                        Source = c.Source
                    })
                    .ToList();

                // ترکیب عناوین مکان‌ها (بدون تکرار)
                var locationTitles = emp.empLocations.Select(l => l.Title)
                    .Concat(emp.postLocations.Select(l => l.Title))
                    .Distinct();

                return new PhoneBookEmploymentDto
                {
                    EmploymentCode = emp.EmploymentCode,
                    FirstName = emp.FirstName,
                    LastName = emp.LastName,
                    OrganizationUnitsName = emp.posts.Select(p => p.OrganizationUnitsName).Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList(),
                    HeadOfOrganizationUnitsName = emp.posts.Select(p => p.HeadOfOrganizationUnitsName).Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList(),
                    JobTitleName = emp.posts.Select(p => p.JobTitleName).Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList(),
                    JobLevelTitle = emp.posts.Select(p => p.JobLevelTitle).Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList(),
                    LocationTitle = emp.empLocations.Select(l => l.Title)
                                    .Concat(emp.postLocations.Select(l => l.Title))
                                    .Where(s => !string.IsNullOrEmpty(s))
                                    .Distinct()
                                    .ToList(),
                    Contacts = contacts
                };
            }).ToList();
        }
        //public static List<PhoneBookEmploymentDto> ToPhoneBookDtos(this IEnumerable<PhoneBookInfoView> rawList)
        //{
        //    return rawList
        //        .GroupBy(x => x.EmploymentCode)
        //        .Select(group =>
        //        {
        //            var emp = group.First();

        //            var employment = new PhoneBookEmploymentDto
        //            {
        //                EmploymentCode = emp.EmploymentCode,
        //                FirstName = emp.FirstName,
        //                LastName = emp.LastName,
        //                OrganizationUnitsName = emp.OrganizationUnitsName,
        //                JobTitleName = emp.JobTitleName,
        //                JobLevelTitle = emp.JobLevelTitle,
        //                LocationTitle = emp.LocationTitle,
        //                Contacts = new List<ContactDetailDto>()
        //            };

        //            foreach (var row in group)
        //            {
        //                // ۱. افزودن تماس‌های شخصی
        //                //AddContact(employment.Contacts, "موبایل شخصی", row.PartyMobile, PhoneBookContactTypeEnum.Mobile, PhoneBookContactSourceEnum.Personal);
        //                //AddContact(employment.Contacts, "تلفن ثابت شخصی", row.PartyPhone, PhoneBookContactTypeEnum.Phone, PhoneBookContactSourceEnum.Personal);
        //                //AddContact(employment.Contacts, "ایمیل شخصی", row.PartyEmail, PhoneBookContactTypeEnum.Email, PhoneBookContactSourceEnum.Personal);
        //                //AddContact(employment.Contacts, "آدرس منزل", row.PartyAddress, PhoneBookContactTypeEnum.Address, PhoneBookContactSourceEnum.Personal);

        //                // ۲. افزودن تماس‌های پست سازمانی
        //                AddContact(employment.Contacts, "داخلی", row.PostContactPhone, PhoneBookContactTypeEnum.Phone, PhoneBookContactSourceEnum.post);
        //                AddContact(employment.Contacts, "موبایل سازمانی", row.PostContactMobile, PhoneBookContactTypeEnum.Mobile, PhoneBookContactSourceEnum.post);
        //                //AddContact(employment.Contacts, "ایمیل سازمانی", row.PostContactEmail, PhoneBookContactTypeEnum.Email, PhoneBookContactSourceEnum.Organizational);
        //                //AddContact(employment.Contacts, "فکس", row.PostContactFax, PhoneBookContactTypeEnum.Fax, PhoneBookContactSourceEnum.Organizational);

        //                // 3. افزودن تماس‌های سازمانی کارمند
        //                AddContact(employment.Contacts, "داخلی", row.EmploymentContactPhone, PhoneBookContactTypeEnum.Phone, PhoneBookContactSourceEnum.employment);
        //                AddContact(employment.Contacts, "موبایل سازمانی", row.EmploymentContactMobile, PhoneBookContactTypeEnum.Mobile, PhoneBookContactSourceEnum.employment);
        //                //AddContact(employment.Contacts, "ایمیل سازمانی", row.PostContactEmail, PhoneBookContactTypeEnum.Email, PhoneBookContactSourceEnum.Organizational);
        //                //AddContact(employment.Contacts, "فکس", row.PostContactFax, PhoneBookContactTypeEnum.Fax, PhoneBookContactSourceEnum.Organizational);

        //                // 4. افزودن تماس‌های مکان کارمند
        //                AddContact(employment.Contacts, $"داخلی ({row.LocationTitle})", row.EmpLocationContactPhone, PhoneBookContactTypeEnum.Phone, PhoneBookContactSourceEnum.employment);
        //                AddContact(employment.Contacts, $"موبایل سازمانی ({row.LocationTitle})", row.EmpLocationContactMobile, PhoneBookContactTypeEnum.Mobile, PhoneBookContactSourceEnum.employment);
        //                //AddContact(employment.Contacts, "ایمیل سازمانی", row.PostContactEmail, PhoneBookContactTypeEnum.Email, PhoneBookContactSourceEnum.Organizational);
        //                //AddContact(employment.Contacts, "فکس", row.PostContactFax, PhoneBookContactTypeEnum.Fax, PhoneBookContactSourceEnum.Organizational);

        //                // 5. افزودن تماس‌های مکان پست سازمانی
        //                AddContact(employment.Contacts, $"داخلی ({row.PostLocationTitle})", row.PostLocationContactPhone, PhoneBookContactTypeEnum.Phone, PhoneBookContactSourceEnum.post);
        //                AddContact(employment.Contacts, $"موبایل سازمانی ({row.PostLocationTitle})", row.PostLocationContactMobile, PhoneBookContactTypeEnum.Mobile, PhoneBookContactSourceEnum.post);
        //                //AddContact(employment.Contacts, "ایمیل سازمانی", row.PostContactEmail, PhoneBookContactTypeEnum.Email, PhoneBookContactSourceEnum.Organizational);
        //                //AddContact(employment.Contacts, "فکس", row.PostContactFax, PhoneBookContactTypeEnum.Fax, PhoneBookContactSourceEnum.Organizational);


        //            }

        //            return employment;
        //        })
        //        .ToList();
        //}
        /*  
         public static List<PhoneBookEmploymentDto> ToPhoneBookDtos(this IEnumerable<EmploymentFullDto> rawList , List<ContactItemDto> contactList)
          {

              return rawList.Select(emp => new PhoneBookEmploymentDto
                      {                       
                         EmploymentCode = emp.EmploymentCode,
                         FirstName = emp.FirstName,
                         JobLevelTitle = string.Join(" - ", emp.posts.Select(a=>a.JobLevelTitle).ToList()),
                         HeadOfOrganizationUnitsName = string.Join(" - ", emp.posts.Select(a => a.HeadOfOrganizationUnitsName).ToList()),
                         JobTitleName = string.Join(" - ", emp.posts.Select(a => a.JobTitleName).ToList()),
                         LastName = emp.LastName,
                         LocationTitle =  string.Join(" - ", emp.empLocations.Select(a => a.Title).ToList(), emp.postLocations.Select(a => a.Title).ToList()),
                          OrganizationUnitsName = string.Join(" - ", emp.posts.Select(a => a.OrganizationUnitsName).ToList()),


                          Contacts = contactList.Where(p=> p.ProfileId == emp.ProfileId || p.ProfileId == emp.PartyProfileId || emp.posts.Any(a=>a.ProfileId == p.ProfileId) || emp.postLocations.Any(a => a.ProfileId == p.ProfileId) || emp.empLocations.Any(a => a.ProfileId == p.ProfileId)).Select(c => new ContactDetailDto
                          {
                              Title = c.Label ?? "",
                              Value = c.Value,
                              Type = c.ContactType,
                              Source = c.Source
                          }).ToList()
                      })
                  .ToList();
          }
         */
        //private static void AddContact(
        //    List<ContactDetailDto> list,
        //    string title,
        //    string? value,
        //    ContactTypeEnum type,
        //    ContactProfileTypeEnum source)
        //{
        //    if (string.IsNullOrWhiteSpace(value)) return;

        //    var cleanValue = value.Trim();

        //    // جلوگیری از تکراری شدن مقدار در صورتی که ویو چند سطر تکراری تولید کرده باشد
        //    if (!list.Any(c => c.Type == type && c.Source == source && c.Value == cleanValue))
        //    {
        //        list.Add(new ContactDetailDto
        //        {
        //            Title = title,
        //            Value = cleanValue,
        //            Type = type,
        //            Source = source
        //        });
        //    }
        //}
    }
}
