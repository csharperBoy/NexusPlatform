using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Shared.Enums.Contact
{
    public enum ContactTypeEnum
    {
        Mobile = 1,         // موبایل
        Phone = 2,       // تلفن ثابت
        OfficePhone = 3,      // شماره داخلی
        OrganizationMobile = 4,
        Email = 5,          // ایمیل
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
}
