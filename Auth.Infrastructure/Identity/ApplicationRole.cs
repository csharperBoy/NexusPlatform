using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Infrastructure.Identity
{
    /*
     اضافه شدن توضیحات و ترتیب نقش‌ها (برای پنل مدیریت نقش‌ها).

    قابل گسترش برای سیستم سلسله‌مراتب نقش‌ها در آینده.
     */
    public class ApplicationRole : IdentityRole<Guid>
    {
        public string? Description { get; set; }
        public int? OrderNum { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
