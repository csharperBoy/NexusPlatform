using Contact.Domain.Enums;
using Core.Shared.Enums.Contact;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Application.DTOs
{
    
    public class ContactDetailDto
    {
        public string Title { get; set; } = null!;
        public string Value { get; set; } = null!;
        public ContactTypeEnum Type { get; set; }
        public ContactProfileTypeEnum Source { get; set; }
        
    }

    public class PhoneBookEmploymentDto
    {
        public string EmploymentCode { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string FullName => $"{FirstName} {LastName}".Trim();

        // اطلاعات سازمانی و شغلی
        public string? OrganizationUnitsName { get; set; }
        public string? HeadOfOrganizationUnitsName { get; set; }
        public string? JobTitleName { get; set; }
        public string? JobLevelTitle { get; set; }
        public string? LocationTitle { get; set; }

        // لیست کامل راه‌های ارتباطی
        public List<ContactDetailDto> Contacts { get; set; } = new();

        // ۱. رشته ترکیب‌شده شماره‌ها برای سطر اصلی (با -)
        public string ContactSummary => string.Join(" - ", Contacts.Where(t=>t.Type == ContactTypeEnum.Phone 
        || t.Type == ContactTypeEnum.Mobile
        || t.Type == ContactTypeEnum.OrganizationMobile
        || t.Type == ContactTypeEnum.OfficePhone
        ).Select(c => c.Value));

        // ۲. فلاگ کنترل‌کننده آکاردئون در فرانت‌اند
        public bool HasMultipleContacts => Contacts.Count > 1;
    }
}
