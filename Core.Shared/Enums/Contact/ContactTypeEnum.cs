using Core.Shared.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Shared.Enums.Contact
{
   
    public enum ContactTypeEnum
    {
        [PersianDescription("موبایل")]
        Mobile = 1,

        [PersianDescription("تلفن ثابت")]
        Phone = 2,

        [PersianDescription("شماره داخلی")]
        OfficePhone = 3,

        [PersianDescription("موبایل سازمانی")]
        OrganizationMobile = 4,

        [PersianDescription("ایمیل")]
        Email = 5,

        [PersianDescription("فکس")]
        Fax = 6,

        [PersianDescription("وب‌سایت")]
        Website = 7,

        [PersianDescription("واتس‌اپ")]
        WhatsApp = 8,

        [PersianDescription("اینستاگرام")]
        Instagram = 9,

        [PersianDescription("تلگرام")]
        Telegram = 10,

        [PersianDescription("لینکدین")]
        LinkedIn = 11,

        [PersianDescription("آدرس")]
        Address = 12,

        [PersianDescription("کد پستی")]
        PostalCode = 13,

        [PersianDescription("سایر")]
        Other = 99
    }
}
