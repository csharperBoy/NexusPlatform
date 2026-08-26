using Core.Shared.Enums.Contact;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Shared.DTOs.Contact
{
    public class ContactItemDto
    {

        public Guid ProfileId { get;  set; }
        public ContactTypeEnum ContactType { get;  set; }
        public ContactProfileTypeEnum Source { get;  set; }
        /// <summary>
        /// مقدار راه ارتباطی (شماره تلفن، آدرس ایمیل، آیدی اینستاگرام، لینک و...)
        /// </summary>
        public string Value { get;  set; }
        /// <summary>
        /// عنوان اختصاصی آیتم (مثلاً: "موبایل شخصی"، "واتس‌اپ کاری"، "ایمیل پشتیبانی")
        /// </summary>
        public string? Label { get;  set; }
        public DateTime? EffectiveFrom { get;  set; }
        public DateTime? EffectiveTo { get;  set; }
        public bool IsCurrent { get;  set; }
        public ICollection<ContactItemDto>? ChildContactItems { get; set; } = null;
    }
}
