using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Domain.Enums
{
   
    public enum ContactRelationTypeEnum
    {
        PublicPhoneNumber = 1, // نگاشت به شماره همگانی/عمومی
        ExtensionOf = 2,    // داخلیِ متصل به خط اصلی
        Alternative = 3  // کانال جایگزین / پشتیبان
    }
}
