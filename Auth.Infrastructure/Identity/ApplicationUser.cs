using Microsoft.AspNetCore.Identity;

namespace Auth.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string? DisplayName { get; set; }
        // در صورت نیاز فیلدهای دامنه‌ای اضافه کنید
    }
}
