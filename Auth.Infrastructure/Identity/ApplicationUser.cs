using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string? DisplayName { get; set; }
        // در صورت نیاز فیلدهای دامنه‌ای اضافه کنید
    }
}
